using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;

namespace VoidZero.Game.Combat.Patterns
{
    public interface IBulletPattern
    {
        void Shoot(Entity shooter, BulletManager bullets, BulletOwner owner, float damage, BulletEnergy energy);
    }
}