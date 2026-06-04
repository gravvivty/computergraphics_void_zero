using VoidZero.Graphics.Particles;
using VoidZero.UI;

namespace VoidZero.Game
{
    public class GameServices
    {
        public static GameServices Instance { get; } = new();
        public ContentManager Content { get; } = new();
        public GameSettings Settings { get; } = new();
        private GameServices() { }
        public ParticleSystem ParticleSystem { get; } = new();
        public DamageNumberManager DamageNumbers { get; } = new();
    }
}
