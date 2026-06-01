using OpenTK.Mathematics;
using VoidZero.Game.Entities;
using VoidZero.Game.Combat;

namespace VoidZero.Game.Combat.Patterns
{
    // Simple Interface for BulletPatterns - that way we can just call Shoot in all IBulletPatterns
    // and depending on the logic in Shoot() each bullet is calculated differently
    public interface IBulletPattern
    {
        void Shoot(Entity shooter, BulletManager bullets, BulletOwner owner, float damage, BulletEnergy energy);
    }
}