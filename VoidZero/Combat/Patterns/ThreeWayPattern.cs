using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;
using System;
using VoidZero.Graphics;

namespace VoidZero.Game.Combat.Patterns
{
    public class ThreeWaySpreadPattern : IBulletPattern
    {
        private readonly Texture2D _bulletTexture;
        private readonly Vector2 _baseDirection;
        private readonly float _angleDegrees;
        private readonly float _bulletSpeed;

        public ThreeWaySpreadPattern(
            Texture2D bulletTexture,
            Vector2 baseDirection,
            float angleDegrees = 40f,
            float bulletSpeed = 1500f
        )
        {
            _bulletTexture = bulletTexture;
            _baseDirection = baseDirection.Normalized();
            _angleDegrees = angleDegrees;
            _bulletSpeed = bulletSpeed;
        }

        public void Shoot(
            Entity shooter,
            BulletManager bullets,
            BulletOwner owner,
            float damage,
            BulletEnergy energy
        )
        {
            Vector2 spawnPos = new(
                shooter.Position.X + shooter.Width / 4f,
                shooter.Position.Y + (owner == BulletOwner.Player ? 0 : shooter.Height/2f)
            );

            // Center
            SpawnBullet(bullets, spawnPos, _baseDirection, damage, owner, energy);

            // Left / Right
            float radian = MathHelper.DegreesToRadians(_angleDegrees);

            Vector2 leftDir = Rotate(_baseDirection, -radian);
            Vector2 rightDir = Rotate(_baseDirection, radian);

            SpawnBullet(bullets, spawnPos, leftDir, damage, owner, energy);
            SpawnBullet(bullets, spawnPos, rightDir, damage, owner, energy);
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
