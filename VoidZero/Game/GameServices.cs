using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game
{
    public class GameServices
    {
        public static GameServices Instance { get; } = new();
        public ContentManager Content { get; } = new();
        public GameSettings Settings { get; } = new();
        private GameServices() { }
    }
}
