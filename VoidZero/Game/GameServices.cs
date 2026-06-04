using VoidZero.Graphics.Particles;

namespace VoidZero.Game
{
    public class GameServices
    {
        public static GameServices Instance { get; } = new();
        public ContentManager Content { get; } = new();
        public GameSettings Settings { get; } = new();
        private GameServices() { }
        public ParticleSystem ParticleSystem { get; } = new();
    }
}
