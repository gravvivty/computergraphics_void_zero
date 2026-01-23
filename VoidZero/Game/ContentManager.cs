using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Graphics;

namespace VoidZero.Game
{
    public class ContentManager
    {
        public Texture2D GetTexture(string name) => _textures[name];
        private readonly Dictionary<string, Texture2D> _textures = new();

        public Texture2D LoadTexture(string name, string path)
        {
            if (_textures.TryGetValue(name, out var texture))
            {
                return texture;
            }

            texture = new Texture2D(path);
            _textures[name] = texture;
            return texture;
        }
    }
}