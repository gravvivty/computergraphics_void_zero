using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;
using VoidZero.Graphics;

namespace VoidZero.Game.Combat.Patterns
{
    public class FixedDirectionPattern : IBulletPattern
    {
        private readonly Texture2D _bulletTex;
        private readonly Vector2 _direction;
        private readonly float _bulletSpeed;

        public FixedDirectionPattern(Texture2D bulletTex, Vector2 direction, float bulletSpeed = 1500f)
        {
            _bulletTex = bulletTex;
            _direction = direction.Normalized();
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

            bullets.Add(new Bullet(
                _bulletTex,
                spawnPos,
                _direction,
                _bulletSpeed,
                damage,
                energy,
                owner
            ));
        }
    }
}
