using OpenTK.Mathematics;
using System.Collections.Generic;

namespace VoidZero.Graphics
{
    public class AnimationManager
    {
        private readonly Dictionary<string, Animation> _animations = new();
        private string _currentKey;
        private bool _currentPlaying = false;
        public string CurrentAnimationKey { get { return _currentKey; } }
        public Animation CurrentAnimation
        {
            get
            {
                if (_currentKey == null)
                {
                    return null;
                }
                return _animations[_currentKey];
            }
        }

        public void Add(string key, Animation animation)
        {
            _animations[key] = animation;
            if (_currentKey == null)
            {
                _currentKey = key;
            }
        }

        public void Play(string key)
        {
            if (_currentKey == key && _currentPlaying)
            {
                return;
            }

            if (_currentKey != null)
            {
                _animations[_currentKey].Stop();
            }

            _currentKey = key;
            _animations[_currentKey].Reset();
            _animations[_currentKey].Start();
            _currentPlaying = true;
        }

        public void Stop()
        {
            if (_currentKey == null)
            {
                return;
            }
            _animations[_currentKey].Stop();
            _currentPlaying = false;
        }

        public void Update(float dt)
        {
            if (_currentKey == null || !_animations.ContainsKey(_currentKey))
            {
                return;
            }
            _animations[_currentKey].Update(dt);
        }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, Vector4 tint, float rotation = 0f)
        {
            if (_currentKey != null)
            {
                _animations[_currentKey].Draw(batch, position, scale, tint, rotation);
            } 
        }

        public bool IsFinished => _currentKey != null && _animations[_currentKey].IsFinished;
    }

}
