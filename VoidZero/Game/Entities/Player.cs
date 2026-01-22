using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;

namespace VoidZero.Game.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 10000f; // low -> on ice
        private readonly float _deceleration = 3000f;
        private readonly float _maxSpeed = 1000f;
        private ShooterComponent _shooter;

        public Player(Texture2D texture, Vector2 startPos, InputManager input, BulletManager bulletManager)
            : base(texture, startPos, 16, 16)
        {
            MaxHealth = 100f;
            CurrentHealth = MaxHealth;
            _input = input;

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new FixedDirectionPattern(bulletTex, -Vector2.UnitY),
                bulletManager,
                BulletOwner.Player,
                0.125f,
                10f
            );
            _shooter.BulletEnergy = BulletEnergy.Neutral;

            // Do this for every entity
            this.Scale = 4f;
            Width = 16 * Scale;
            Height = 16 * Scale;

            Animations.Add("Up", new Animation(texture, 16, 16, 3, 0.1f, 0));
            Animations.Add("Right", new Animation(texture, 16, 16, 3, 0.1f, 2));
            Animations.Add("Down", new Animation(texture, 16, 16, 3, 0.1f, 4));
            Animations.Add("Left", new Animation(texture, 16, 16, 3, 0.1f, 6));

            Animations.Add("Idle", new Animation(texture, 16, 16, 1, 1f, 0));
        }

        public override void Update(float dt)
        {
            Vector2 inputDir = Vector2.Zero;

            if (_input.MoveUp) inputDir.Y -= 1;
            if (_input.MoveDown) inputDir.Y += 1;
            if (_input.MoveLeft) inputDir.X -= 1;
            if (_input.MoveRight) inputDir.X += 1;

            if (inputDir.LengthSquared > 0)
            {
                inputDir = inputDir.Normalized();
                Velocity += inputDir * _acceleration * dt;
            }
            else if (Velocity.LengthSquared > 0)
            {
                Vector2 decel = Velocity.Normalized() * _deceleration * dt;
                Velocity = decel.LengthSquared > Velocity.LengthSquared ? Vector2.Zero : Velocity - decel;
            }

            if (Velocity.Length > _maxSpeed)
            {
                Velocity = Velocity.Normalized() * _maxSpeed;
            }

            Position += Velocity * dt;


            // Hooks for later
            if (_input.Shoot)
            {
                // TODO: Shoot
            }

            if (_input.SwitchShield)
            {
                // TODO: Switch shield
            }

            _shooter.TryShoot(this, dt, _input.Shoot);
            UpdateAnimation(GetAnimationKey(inputDir), dt);

            // Debug pattern
            if (_input.SwitchPatternPressed)
            {
                _shooter.SetPattern(
                    new ThreeWaySpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        40f
                    )
                );
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
        }

        private string GetAnimationKey(Vector2 dir)
        {
            if (dir.LengthSquared == 0)
                return "Idle";

            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                return dir.X > 0 ? "Right" : "Left";
            else
                return dir.Y > 0 ? "Down" : "Up";
        }
    }
}