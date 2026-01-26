using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Game.Entities;
using VoidZero.Game.Input;
using OpenTK.Windowing.Desktop;
using VoidZero.Core;
using System.Drawing;
using System.Collections.Generic;
using VoidZero.Game.Combat;
using System;
using VoidZero.Game.Entities.Enemies;
using System.Linq;
using VoidZero.States.Stages;
using VoidZero.States.Stages.VoidZero.States.Stages;
using VoidZero.Game.Entities.Components;
using static VoidZero.Core.GameManager;

namespace VoidZero.States
{
    public class PlayState : GameState, IResizableState
    {
        private readonly InputManager _input;
        public GameWindow Window { get; private set; }
        private readonly GameStateManager _gameStateManager;
        private readonly GameManager _gameManager;
        private StageComposer _stageComposer;
        private float _criticalBlinkTimer = 0f;
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
            _player.SetPositionRelative(_player.Position, Window.Size.X, Window.Size.Y);

            Texture2D shieldTexture = GameServices.Instance.Content.GetTexture("shield");
            _playerShield = new Shield(shieldTexture, _player);

            Entities.Add(_player);
            Entities.Add(_playerShield);
            StageIndex = stageIndex;
            _isDying = false;
        }

        public override void Update(float dt)
        {
            // 🔑 ESC works ONLY while alive
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

            // --- bullets always update (unless paused) ---
            float bulletDt = dt;
            if (_isDying)
                bulletDt *= 0.6f;

            Bullets.Update(bulletDt);

            // No collisions once dying
            if (!_isDying)
                HandleBulletHits(dt);

            // --- death sequence ---
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
                    entity.Update(dt);

                return;
            }

            // --- normal gameplay ---
            _stageComposer.Update(dt, this);

            foreach (var entity in Entities.ToList())
            {
                entity.Update(dt);

                bool remove =
                    entity.IsDead ||
                    entity.Components.OfType<TimedExitComponent>().Any(c => c.IsExpired);

                if (remove)
                    Entities.Remove(entity);
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
            DrawAbilityBar(spriteBatch);
            DrawGrazingBar(spriteBatch);
            if (_player.IsCriticalHealth)
            {
                DrawCriticalBorder(spriteBatch, dt);
            }
        }

        public void OnResize(int newWidth, int newHeight)
        {
            foreach (Entity entity in Entities)
            {
                entity.OnResize(newWidth, newHeight);
            }
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
                            if (bullet.Hitbox.IntersectsWith(entity.Hitbox))
                            {
                                entity.CurrentHealth -= bullet.Damage;
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
                        bullet.Energy != BulletEnergy.Green;

                    bool damageHit = _player.Hitbox.IntersectsWith(bullet.Hitbox);
                    bool grazeHit =
                        bullet.GrazeHitbox.IntersectsWith(_player.Hitbox) &&
                        !damageHit &&
                        !shieldAbsorbs;

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
                                _gameManager.Shake(0.25f, 20f);
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
                return;

            _isDying = true;
            _playerShield.Kill();
            _gameManager.EnterDeath();
        }

        private void DrawCriticalBorder(SpriteBatch spriteBatch, float dt)
        {
            if (_player.IsCriticalHealth)
            {
                _criticalBlinkTimer += dt;

                // Red blinking factor
                float blink = (MathF.Sin(_criticalBlinkTimer * 6f) + 1f) * 0.5f; // 0..1
                blink = MathHelper.Lerp(0.2f, 0.6f, blink); // min/max alpha

                // Only fade based on health regen if player is regenerating from 1 HP
                float healthFade = 1f; // fully visible by default
                if (_player.CurrentHealth < _player.MaxHealth)
                {
                    healthFade = 1f - _player.HealthRegenProgress; // fade as color returns
                }

                float alpha = blink * healthFade;

                Color borderColor = Color.FromArgb(
                    (int)(alpha * 255),
                    255, 50, 50
                );

                RectangleF rect = new RectangleF(0, 0,
                    GameServices.Instance.Settings.Width,
                    GameServices.Instance.Settings.Height);

                float thickness = 15f;
                spriteBatch.DrawRectangle(rect, borderColor, false, thickness);
            }
            else
            {
                // Reset blink timer if player is not at critical HP
                _criticalBlinkTimer = 0f;
            }
        }

        public void DrawAbilityBar(SpriteBatch spritebatch)
        {
            // Example position and size
            float barX = 20f;
            float barY = 20f;
            float barWidth = 200f;
            float barHeight = 20f;

            // Draw background
            spritebatch.DrawRectangle(new RectangleF(barX, barY, barWidth, barHeight), Color.Gray, true);

            // Determine fill color based on level
            Color fillColor = Color.LightBlue;
            if (_player._abilityBar >= _player.Level3Threshold) fillColor = Color.Red;
            else if (_player._abilityBar >= _player.Level2Threshold) fillColor = Color.Orange;
            else if (_player._abilityBar >= _player.Level1Threshold) fillColor = Color.Yellow;

            float fill = _player._abilityBar / _player.MaxAbilityBar;
            spritebatch.DrawRectangle(new RectangleF(barX, barY, barWidth * fill, barHeight), Color.Cyan, true);

            float[] thresholds = { _player.Level1Threshold, _player.Level2Threshold, _player.Level3Threshold };

            foreach (float threshold in thresholds)
            {
                float markerX = barX + (threshold / _player.MaxAbilityBar) * barWidth;
                // Draw a thin line as marker
                spritebatch.DrawRectangle(new RectangleF(markerX - 1, barY, 2, barHeight), Color.Yellow, true);
            }
        }

        public void DrawGrazingBar(SpriteBatch spritebatch)
        {
            float barX = 20f;
            float barY = 20f;
            float barWidth = 200f;
            float barHeight = 20f;
            float spacing = 10f;

            float grazeBarY = barY + barHeight + spacing;
            float grazeBonus = _player.DamageMultiplier - 1f;
            float maxBonus = _player.MaxGrazeMultiplier - 1f;

            float grazeFill = grazeBonus / maxBonus;
            grazeFill = Math.Clamp(grazeFill, 0f, 1f);

            // Draw background
            spritebatch.DrawRectangle(new RectangleF(barX, grazeBarY, barWidth, barHeight), Color.DarkGray, true);

            // Draw fill
            spritebatch.DrawRectangle(new RectangleF(barX, grazeBarY, barWidth * grazeFill, barHeight), Color.LightBlue, true);
        }
    }
}