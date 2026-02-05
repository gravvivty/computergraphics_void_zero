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
        public bool Borderless { get; set; } = false;
        public int Width { get; set; } = 1600;
        public int Height { get; set; } = 900;
        public float BackgroundSpeedMultiplier { get; set; } = 6f;
        public float MasterVolume = 0.5f;
        public float SfxVolume = 0.5f;
        public float MusicVolume = 0.5f;
        public float UiVolume = 0.5f;
    }
}
