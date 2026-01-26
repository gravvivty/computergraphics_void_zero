using OpenTK.Mathematics;
using System.Collections.Generic;

namespace VoidZero.Graphics
{
    public class Animation
    {
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        private readonly Texture2D _texture;
        private readonly List<Rectangle> _frames;
        private int _currentFrame;
        private float _frameTime;
        private float _timeLeft;
        private bool _playing = false;
        private bool _loop = true;

        public bool IsFinished { get; private set; } = false;

        public Animation(Texture2D texture, int frameWidth, int frameHeight, int frameCount, float frameTime, int column = 0, bool loop = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _timeLeft = frameTime;
            _frames = new List<Rectangle>();
            _loop = loop;
            FrameHeight = frameHeight;
            FrameWidth = frameWidth;

            int columns = _texture.Width / frameWidth;

            for (int i = 0; i < frameCount; i++)
            {
                int x = column * frameWidth;
                int y = i * frameHeight;
                _frames.Add(new Rectangle(x, y, frameWidth, frameHeight));
            }
        }

        public void Start() { _playing = true; IsFinished = false; }
        public void Stop() { _playing = false; }

        public void Reset()
        {
            _currentFrame = 0;
            _timeLeft = _frameTime;
            IsFinished = false;
        }

        public void Update(float dt)
        {
            if (!_playing || IsFinished) return;

            _timeLeft -= dt;
            if (_timeLeft <= 0f)
            {
                _timeLeft += _frameTime;
                _currentFrame++;
                if (_currentFrame >= _frames.Count)
                {
                    if (_loop)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = _frames.Count - 1;
                        IsFinished = true;
                    }
                }
            }
        }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, Vector4 tint, float rotation = 0f)
        {
            batch.DrawFrame(_texture, position, _frames[_currentFrame], tint, scale, rotation);
        }
    }

}
