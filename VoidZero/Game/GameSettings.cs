using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game
{
    public class GameSettings
    {
        public bool Fullscreen { get; set; } = false;
        public int Width { get; set; } = 1600;
        public int Height { get; set; } = 900;
        public float BackgroundSpeedMultiplier { get; set; } = 4f;

        // Add more settings here if needed (volume, difficulty, etc.)
    }
}
