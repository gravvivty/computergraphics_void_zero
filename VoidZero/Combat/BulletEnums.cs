using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game.Combat
{
    // Simple enums needed for collision checking
    public enum BulletEnergy
    {
        Blue,
        Red,
        Green,
        Neutral
    }

    public enum BulletOwner
    {
        Player,
        Enemy,
        None
    }
}
