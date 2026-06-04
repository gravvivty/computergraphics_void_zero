namespace VoidZero.Game
{
    // General game settings
    public class GameSettings
    {
        public bool Fullscreen { get; set; } = false;
        public bool Borderless { get; set; } = false;
        public int Width { get; set; } = 1600;
        public int Height { get; set; } = 900;
        public int WorldWidth { get; } = 1920;
        public int WorldHeight { get; } = 1080;
        // Change this value to adjust the scrolling speed of the main background
        public float BackgroundSpeedMultiplier { get; set; } = 6f;
        // Toggle debug drawing of graze hitboxes (F3)
        public bool ShowGrazeHitboxes { get; set; } = false;
        // Toggle debug drawing of entity hitboxes (F4)
        public bool ShowHitboxes { get; set; } = false;
        public float MasterVolume = 0.5f;
        public float SfxVolume = 0.5f;
        public float MusicVolume = 0.5f;
        public float UiVolume = 0.5f;
    }
}
