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
        private readonly GameStateManager _gsm;
        private readonly GameManager _gm;
        public BulletManager Bullets { get; } = new();

        public Background _background { get; }
        public List<Entity> Entities { get; } = new();

        private Player _player;
        private Shield _playerShield;


        public PlayState(GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm)
        {
            _gsm = gsm;
            _window = window;
            _input = input;
            _background = bg;
            _gm = gm;

            var tex = GameServices.Instance.Content.GetTexture("player");
            _player = new Player(tex, new Vector2(500, 500), _input, Bullets);
            _player.SetPositionRelative(_player.Position,window.Size.X, window.Size.Y);

            var shieldTex = GameServices.Instance.Content.GetTexture("shield");
            _playerShield = new Shield(shieldTex, _player);

            var stationaryEnemyTexture = GameServices.Instance.Content.GetTexture("witch");
            var stationaryEnemy = new StationaryEnemy(stationaryEnemyTexture, new Vector2(750, 0), Bullets);
            stationaryEnemy.SetPositionRelative(stationaryEnemy.Position, _window.Size.X, _window.Size.Y);

            var rotatingEnemyTexture = GameServices.Instance.Content.GetTexture("witch");
            var rotatingEnemy = new RotatingEnemy(rotatingEnemyTexture, new Vector2(350, 0), Bullets, 1);
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
                entity.Update(dt);
            Bullets.Update(dt);
            HandleBulletHits(dt);


            // Pause logic
            if (_input.ConsumePausePressed())
            {
                _gsm.ChangeState(new PauseState(_gsm, _window, _input, this, _gm));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in Entities)
            {
                entity.Draw(spriteBatch);
            }

            Bullets.Draw(spriteBatch);
            Bullets.Draw(spriteBatch);
        }

        public override void DrawUI()
        {
            // HUD etc
        }

        public void OnResize(int newWidth, int newHeight)
        {
            foreach (var entity in Entities)
                entity.OnResize(newWidth, newHeight);
        }

        private void HandleBulletHits(float dt)
        {
            for (int i = Bullets.Bullets.Count - 1; i >= 0; i--)
            {
                var bullet = Bullets.Bullets[i];

                // Player bullets hitting enemies
                if (bullet.Owner == BulletOwner.Player)
                {
                    foreach (var entity in Entities)
                    {
                        // Check if entity is any type of enemy
                        if (entity is RotatingEnemy || entity is StationaryEnemy)
                        {
                            if (bullet.Hitbox.IntersectsWith(entity.Hitbox))
                            {
                                entity.CurrentHealth -= bullet.Damage;
                                Bullets.Bullets.RemoveAt(i);

                                if (entity.CurrentHealth <= 0)
                                    Entities.Remove(entity);

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
                        if (shieldAbsorbs)
                        {
                            _playerShield.Flash();
                        }
                        else
                        {
                            _player.CurrentHealth -= bullet.Damage;
                        }

                        Bullets.Bullets.RemoveAt(i);
                    }
                }
            }
        }

    }
}