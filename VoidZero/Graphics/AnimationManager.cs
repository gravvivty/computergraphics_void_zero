using OpenTK.Mathematics;
using System.Collections.Generic;

namespace VoidZero.Graphics
{
    public class AnimationManager
    {
        private readonly Dictionary<string, Animation> _animations = new();
        private string _currentKey;

        public void Add(string key, Animation anim)
        {
            _animations[key] = anim;
            if (_currentKey == null) _currentKey = key;
        }

        public void Update(string key, float dt)
        {
            if (!_animations.ContainsKey(key)) return;

            if (_currentKey != key)
            {
                _animations[_currentKey].Reset();
                _currentKey = key;
            }

            _animations[_currentKey].Start();
            _animations[_currentKey].Update(dt);
        }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, Vector4 tint)
        {
            if (_currentKey != null)
            {
                _animations[_currentKey].Draw(batch, position, scale, tint);
            }
        }
    }
}
