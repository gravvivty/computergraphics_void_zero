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

namespace VoidZero.States
{
    public class PlayState : GameState, IResizableState
    {
        private readonly InputManager _input;
        private readonly GameWindow _window;
        private readonly GameStateManager _gameStateManager;
        private readonly GameManager _gameManager;
        private float _criticalBlinkTimer = 0f;
        public BulletManager Bullets { get; } = new();

        public Background _background { get; }
        public List<Entity> Entities { get; } = new();

        public Player _player { get; }
        private Shield _playerShield;


        public PlayState(GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm)
        {
            _gameStateManager = gsm;
            _window = window;
            _input = input;
            _background = bg;
            _gameManager = gm;

            Texture2D playerTexture = GameServices.Instance.Content.GetTexture("player");
            _player = new Player(playerTexture, new Vector2(500, 500), _input, Bullets);
            _player.SetPositionRelative(_player.Position,window.Size.X, window.Size.Y);

            Texture2D shieldTexture = GameServices.Instance.Content.GetTexture("shield");
            _playerShield = new Shield(shieldTexture, _player);

            Texture2D stationaryEnemyTexture = GameServices.Instance.Content.GetTexture("witch");
            StationaryEnemy stationaryEnemy = new StationaryEnemy(stationaryEnemyTexture, new Vector2(750, 0), Bullets);
            stationaryEnemy.SetPositionRelative(stationaryEnemy.Position, _window.Size.X, _window.Size.Y);

            Texture2D rotatingEnemyTexture = GameServices.Instance.Content.GetTexture("witch");
            RotatingEnemy rotatingEnemy = new RotatingEnemy(rotatingEnemyTexture, new Vector2(350, 0), Bullets, 1);
            rotatingEnemy.SetPositionRelative(rotatingEnemy.Position, _window.Size.X, _window.Size.Y);
            rotatingEnemy.Movement = new MovementComponent(Vector2.UnitX, 200f, 5f);

            Entities.Add(stationaryEnemy);
            Entities.Add(rotatingEnemy);
            Entities.Add(_player);
            Entities.Add(_playerShield);

        }

        public override void Update(float dt)
        {
            _background.Update(dt);
            foreach (var entity in Entities.ToList())
            {
                entity.Update(dt);
            }
            Bullets.Update(dt);
            HandleBulletHits(dt);

            // Pause logic
            if (_input.ConsumePausePressed())
            {
                _gameStateManager.ChangeState(new PauseState(_gameStateManager, _window, _input, this, _gameManager));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Entity entity in Entities)
            {
                entity.Draw(spriteBatch);
            }

            Bullets.Draw(spriteBatch);
            Bullets.Draw(spriteBatch);
        }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
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
                        if (entity is RotatingEnemy || entity is StationaryEnemy)
                        {
                            if (bullet.Hitbox.IntersectsWith(entity.Hitbox))
                            {
                                entity.CurrentHealth -= bullet.Damage;
                                Bullets.Bullets.RemoveAt(i);

                                if (entity.CurrentHealth <= 0)
                                {
                                    Entities.Remove(entity);
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
                        _player.RegisterGraze(dt);
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
                        }
                        else
                        {
                            if (_player.CurrentHealth == 0)
                            {
                                _player.CurrentHealth = 0;
                            }
                            else
                            {
                                _player.CurrentHealth -= bullet.Damage;
                            }
                            
                            _player.OnDamaged();
                            _gameManager.Shake(0.25f, 20f);
                        }

                        Bullets.Bullets.RemoveAt(i);
                    }
                }
            }
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
    }
}