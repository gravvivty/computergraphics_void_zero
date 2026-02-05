using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;
using System;
using VoidZero.Graphics;
using VoidZero.Game.Entities.Components;

namespace VoidZero.Game.Combat.Patterns
{
    public class SpreadPattern : IBulletPattern
    {
        private readonly Texture2D _bulletTexture;
        private readonly Vector2 _baseDirection;
        private readonly float _bulletSpeed;
        private readonly int _bulletCount;
        private readonly float _angleBetweenDegrees;
        private readonly bool _omnidirectional;

        public SpreadPattern(
            Texture2D bulletTexture,
            Vector2 baseDirection,
            int bulletCount = 3,
            float angleBetweenDegrees = 20f,
            float bulletSpeed = 1500f,
            bool omnidirectional = false
        )
        {
            _bulletTexture = bulletTexture;
            _baseDirection = baseDirection.Normalized();
            _bulletCount = bulletCount;
            _angleBetweenDegrees = angleBetweenDegrees;
            _bulletSpeed = bulletSpeed;
            _omnidirectional = omnidirectional;
        }

        public void Shoot(Entity shooter, BulletManager bullets, BulletOwner owner, float damage, BulletEnergy energy)
        {
            Vector2 spawnPos = new(
                shooter.Position.X + shooter.Width / 4f,
                shooter.Position.Y + (owner == BulletOwner.Player ? 0 : shooter.Height / 2f)
            );

            float rotation = shooter.Rotation;
            RotationComponent rotComp = shooter.Components.OfType<RotationComponent>().FirstOrDefault();
            if (rotComp != null) rotation += rotComp.CurrentRotation;

            if (_omnidirectional)
            {
                float angleStep = 360f / _bulletCount;
                for (int i = 0; i < _bulletCount; i++)
                {
                    float radians = MathHelper.DegreesToRadians(i * angleStep);
                    Vector2 dir = Rotate(Vector2.UnitY, radians + rotation);
                    SpawnBullet(bullets, spawnPos, dir, damage, owner, energy);
                }
            }
            else
            {
                float totalSpread = (_bulletCount - 1) * _angleBetweenDegrees;
                if (totalSpread > 180f) totalSpread = 180f;
                float startAngle = -totalSpread / 2f;

                for (int i = 0; i < _bulletCount; i++)
                {
                    float angle = startAngle + i * _angleBetweenDegrees;
                    float radians = MathHelper.DegreesToRadians(angle);
                    Vector2 dir = Rotate(_baseDirection, radians + rotation);
                    SpawnBullet(bullets, spawnPos, dir, damage, owner, energy);
                }
            }
        }


        private void SpawnBullet(
            BulletManager bullets,
            Vector2 position,
            Vector2 direction,
            float damage,
            BulletOwner owner,
            BulletEnergy energy
        )
        {
            bullets.Add(new Bullet(
                _bulletTexture,
                position,
                direction,
                _bulletSpeed,
                damage,
                energy,
                owner
            ));
        }

        private static Vector2 Rotate(Vector2 vector, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            ).Normalized();
        }
    }
}
