using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using VoidZero.Core;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States.Stages;
using VoidZero.States.Stages.VoidZero.States.Stages;
using static VoidZero.Core.GameManager;

namespace VoidZero.States
{
    public class PlayState : GameState
    {
        private readonly InputManager _input;
        public GameWindow Window { get; private set; }
        private readonly GameStateManager _gameStateManager;
        private readonly GameManager _gameManager;
        private StageComposer _stageComposer;
        public BulletManager Bullets { get; } = new();

        public Background _background { get; }
        public List<Entity> Entities { get; } = new();

        public Player _player { get; }
        private Shield _playerShield;
        public int StageIndex { get; }
        private bool _isDying = false;
        private float _deathTimer = 0f;
        private const float DeathAnimDuration = 1f;
        private const float BulletSlowFactor = 0.6f;



        public PlayState(GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm, int stageIndex = 1)
        {
            _gameStateManager = gsm;
            Window = window;
            _input = input;
            _background = bg;
            _gameManager = gm;
            StageIndex = stageIndex;
            IStageDefinition stage = StageIndex switch
            {
                1 => new Stage1(),
                2 => new Stage2(),
                3 => new Stage3(),
                _ => new Stage1()
            };

            _stageComposer = stage.Build();

            Texture2D playerTexture = GameServices.Instance.Content.GetTexture("player");
            _player = new Player(playerTexture, new Vector2(500, 500), _input, Bullets);
            _player.SetPosition(_player.Position);

            Texture2D shieldTexture = GameServices.Instance.Content.GetTexture("shield");
            _playerShield = new Shield(shieldTexture, _player);

            Entities.Add(_player);
            Entities.Add(_playerShield);
            StageIndex = stageIndex;
            _isDying = false;
        }

        public override void Update(float dt)
        {
            if (!_isDying && _input.ConsumePausePressed())
            {
                _gameManager.EnterPause();
                _gameStateManager.ChangeState(
                    new PauseState(_gameStateManager, Window, _input, this, _gameManager)
                );
                return;
            }

            if (_gameManager.CurrentMode == GameMode.Paused)
                return;

            // Bullets always update (unless paused)
            float bulletDt = dt;
            if (_isDying)
            {
                bulletDt *= BulletSlowFactor;
            }

            Bullets.Update(bulletDt);
            GameServices.Instance.ParticleSystem.Update(dt);

            // No collisions once dying
            if (!_isDying)
            {
                HandleBulletHits(dt);
            }

            // death sequence
            if (_isDying)
            {
                _deathTimer += dt;

                if (_deathTimer >= DeathAnimDuration)
                {
                    _gameStateManager.ChangeState(
                        new DeathState(_gameStateManager, Window, _gameManager)
                    );
                }

                // Update entities for death animation only
                foreach (var entity in Entities.ToList())
                {
                    entity.Update(dt);
                }

                return;
            }

            _stageComposer.Update(dt, this);

            foreach (var entity in Entities.ToList())
            {
                entity.Update(dt);

                bool remove =
                    entity.IsDead ||
                    entity.Components.OfType<MovementLifecycleComponent>().Any(c => c.IsExpired);

                if (remove)
                {
                    Entities.Remove(entity);
                }
            }
        }



        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Entity entity in Entities)
            {
                entity.Draw(spriteBatch);
            }

            Bullets.Draw(spriteBatch);
        }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
        }

        private void HandleBulletHits(float dt)
        {
            for (int i = Bullets.Bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = Bullets.Bullets[i];

                // Player bullets hitting enemies
                if (bullet.Owner == BulletOwner.Player)
                {
                    foreach (Entity entity in Entities)
                    {
                        // Check if entity is any type of enemy
                        if (entity is Enemy enemy)
                        {
                            if (enemy.IsDying)
                                continue;
                            if (bullet.HitEntities.Contains(enemy))  // Already hit this enemy
                                continue;
                            if (bullet.Hitbox.IntersectsWith(entity.Hitbox))
                            {
                                float healthBefore = entity.CurrentHealth;
                                entity.CurrentHealth -= bullet.Damage;
                                float healthAfter = entity.CurrentHealth;

                                Console.WriteLine($"[HIT] {enemy.GetType().Name} | Damage: {bullet.Damage} | HP: {healthBefore} -> {healthAfter}");

                                bullet.HitEntities.Add(enemy);

                                Vector2 impactDir = bullet.Velocity.Normalized();

                                GameServices.Instance.ParticleSystem.SpawnSparks(enemy.VisualCenter, impactDir, 25);

                                Bullets.Bullets.RemoveAt(i);

                                if (entity.CurrentHealth <= 0)
                                {
                                    entity.Kill();
                                }

                                break;
                            }
                        }
                    }
                }
                // Enemy bullets hitting player
                else if (bullet.Owner == BulletOwner.Enemy)
                {
                    bool shieldAbsorbs =
                        bullet.Energy == _player.ActiveShield &&
                        bullet.Energy != BulletEnergy.Purple;

                    bool damageHit = _player.Hitbox.IntersectsWith(bullet.Hitbox);
                    bool grazeHit = bullet.GrazeHitbox.IntersectsWith(_player.Hitbox) && !damageHit && !shieldAbsorbs;

                    if (grazeHit)
                    {
                        _player.RegisterGraze();
                    }

                    if (damageHit)
                    {
                        if (_player.IsInvulnerable)
                        {
                            Bullets.Bullets.RemoveAt(i);
                            continue;
                        }

                        if (shieldAbsorbs)
                        {
                            _playerShield.Flash();
                            _player.FillAbilityBar(_player.AbsorbFillAmount);
                        }
                        else
                        {
                            _player.CurrentHealth -= bullet.Damage;

                            if (_player.CurrentHealth <= 0)
                            {
                                _player.CurrentHealth = 0;
                                _player.Kill();
                                OnPlayerDied();
                            }
                            else
                            {
                                _player.OnDamaged();
                                _gameManager.Shake(0.25f, 30f);
                            }
                        }

                        Bullets.Bullets.RemoveAt(i);
                    }
                }
            }
        }

        private void OnPlayerDied()
        {
            if (_isDying)
            {
                return;
            }

            _isDying = true;
            _playerShield.Kill();
            _gameManager.EnterDeath();
        }
    }
}