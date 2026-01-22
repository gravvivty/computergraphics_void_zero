using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;
using System;
using VoidZero.Graphics;

namespace VoidZero.Game.Combat.Patterns
{
    public class ThreeWaySpreadPattern : IBulletPattern
    {
        private readonly Texture2D _bulletTex;
        private readonly Vector2 _baseDirection;
        private readonly float _angleDeg;
        private readonly float _bulletSpeed;

        public ThreeWaySpreadPattern(
            Texture2D bulletTex,
            Vector2 baseDirection,
            float angleDegrees = 40f,
            float bulletSpeed = 1500f
        )
        {
            _bulletTex = bulletTex;
            _baseDirection = baseDirection.Normalized();
            _angleDeg = angleDegrees;
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
                shooter.Position.Y + (owner == BulletOwner.Player ? 0 : shooter.Height)
            );

            // Center
            SpawnBullet(bullets, spawnPos, _baseDirection, damage, owner, energy);

            // Left / Right
            float rad = MathHelper.DegreesToRadians(_angleDeg);

            Vector2 leftDir = Rotate(_baseDirection, -rad);
            Vector2 rightDir = Rotate(_baseDirection, rad);

            SpawnBullet(bullets, spawnPos, leftDir, damage, owner, energy);
            SpawnBullet(bullets, spawnPos, rightDir, damage, owner, energy);
        }

        private void SpawnBullet(
            BulletManager bullets,
            Vector2 pos,
            Vector2 dir,
            float damage,
            BulletOwner owner,
            BulletEnergy energy
        )
        {
            bullets.Add(new Bullet(
                _bulletTex,
                pos,
                dir,
                _bulletSpeed,
                damage,
                energy,
                owner
            ));
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            return new Vector2(
                v.X * cos - v.Y * sin,
                v.X * sin + v.Y * cos
            ).Normalized();
        }
    }
}
