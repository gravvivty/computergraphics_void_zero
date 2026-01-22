using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Game.Entities;
using VoidZero.Graphics;

namespace VoidZero.Game.Combat
{
    public class BulletManager
    {
        public List<Bullet> Bullets { get; } = new();

        public void Add(Bullet bullet)
        {
            Bullets.Add(bullet);
        }

        public void Update(float dt)
        {
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(dt);

                if (Bullets[i].IsExpired)
                    Bullets.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (var bullet in Bullets)
                bullet.Draw(batch);
        }
    }
}
