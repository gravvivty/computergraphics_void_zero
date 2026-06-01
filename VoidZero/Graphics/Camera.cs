using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Game;

namespace VoidZero.Graphics
{
    public class Camera
    {
        public Vector2 Position;

        private readonly Random _rng = new();

        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeStrength;

        private Vector2 _shakeOffset;

        public void Update(float dt)
        {
            if (_shakeTimer <= 0f)
            {
                _shakeOffset = Vector2.Zero;
                return;
            }

            _shakeTimer -= dt;

            float t = _shakeTimer / _shakeDuration;
            float strength = _shakeStrength * t * t;

            _shakeOffset = new Vector2(
                ((float)_rng.NextDouble() * 2f - 1f) * strength,
                ((float)_rng.NextDouble() * 2f - 1f) * strength
            );
        }

        public void Shake(float duration, float strength)
        {
            _shakeDuration = duration;
            _shakeTimer = duration;
            _shakeStrength = strength;
        }

        public Matrix4 GetTransform()
        {
            float worldWidth = GameServices.Instance.Settings.WorldWidth;
            float worldHeight = GameServices.Instance.Settings.WorldHeight;

            Matrix4 projection =
                Matrix4.CreateOrthographicOffCenter(
                    0,
                    worldWidth,
                    worldHeight,
                    0,
                    -1,
                    1);

            Matrix4 view =
                Matrix4.CreateTranslation(
                    -Position.X + _shakeOffset.X,
                    -Position.Y + _shakeOffset.Y,
                    0f);

            return view * projection;
        }
    }
}
