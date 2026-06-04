using OpenTK.Mathematics;
using VoidZero.Game.Combat;

namespace VoidZero.Utils
{
    public static class BulletColorHelper
    {
        public static Vector4 GetTint(BulletEnergy energy)
        {
            return energy switch
            {
                BulletEnergy.Purple => new Vector4(0.6f, 0f, 1f, 1f),    // Purple
                BulletEnergy.Yellow => new Vector4(1f, 1f, 0f, 1f),    // Yellow
                BulletEnergy.Blue => new Vector4(0.0f, 0.6f, 1f, 1f),   // Blue
                BulletEnergy.Neutral => Vector4.One,    // No tint
                _ => Vector4.One
            };
        }
    }
}
