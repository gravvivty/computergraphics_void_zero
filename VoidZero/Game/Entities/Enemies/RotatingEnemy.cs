using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities;
using System;

namespace VoidZero.Game.Entities.Enemies
{
    public class RotatingEnemy : Entity
    {
        private ShooterComponent _shooter;
        private float _rotationSpeed; // radians per second
        private float _currentAngle;  // current rotation angle

        public RotatingEnemy(Texture2D texture, Vector2 position, BulletManager bulletManager, float rotationSpeed = 1f)
            : base(texture, position, 24, 24)
        {
            MaxHealth = 60f;
            CurrentHealth = MaxHealth;
            Scale = 6f;
            Width = 24 * Scale;
            Height = 24 * Scale;

            _rotationSpeed = rotationSpeed;
            _currentAngle = 0f;

            Animations.Add("Idle", new Animation(texture, 24, 24, 3, 1f, 0));

            // Setup shooter with CardinalPattern
            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new CardinalPattern(bulletTex, 500f),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 0.25f,
                damage: 1f
            );
            _shooter.BulletEnergy = BulletEnergy.Green;
        }

        public override void Update(float dt)
        {
            Movement?.Update(this, dt);
            // Rotate enemy
            _currentAngle += _rotationSpeed * dt;
            if (_currentAngle > MathF.PI * 2f)
            {
                _currentAngle -= MathF.PI * 2f;
            }

            // Apply rotation to pattern
            if (_shooter.Pattern is CardinalPattern cardinal)
            {
                cardinal.SetRotation(_currentAngle);
            }

            _shooter.TryShoot(this, dt, trigger: true);

            UpdateAnimation("Idle", dt);
        }
    }
}
