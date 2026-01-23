using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Game.Combat;

namespace VoidZero.Utils
{
    public static class BulletColorHelper
    {
        public static Vector4 GetTint(BulletEnergy energy)
        {
            return energy switch
            {
                BulletEnergy.Green => new Vector4(0f, 1f, 0f, 1f),   // green
                BulletEnergy.Yellow => new Vector4(1f, 1f, 0f, 1f),   // yellow
                BulletEnergy.Blue => new Vector4(0.1f, 0.8f, 1f, 1f), // blue
                BulletEnergy.Neutral => Vector4.One,                  // no tint
                _ => Vector4.One
            };
        }
    }
}
