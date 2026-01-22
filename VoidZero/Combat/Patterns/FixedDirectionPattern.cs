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

        public FixedDirectionPattern(Texture2D bulletTex, Vector2 direction)
        {
            _bulletTex = bulletTex;
            _direction = direction.Normalized();
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

            bullets.Add(new Bullet(
                _bulletTex,
                spawnPos,
                _direction,
                1500f,
                damage,
                energy,
                owner
            ));
        }
    }
}
