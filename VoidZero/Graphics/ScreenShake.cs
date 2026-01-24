using OpenTK.Mathematics;
using System;

namespace VoidZero.Graphics
{
    public class ScreenShake
    {
        private float _timer;
        private float _duration;
        private float _strength;

        private readonly Random _rng = new();

        public Vector2 Offset { get; private set; } = Vector2.Zero;

        public void Start(float duration, float strength)
        {
            _duration = duration;
            _timer = duration;
            _strength = strength;
        }

        public void Update(float dt)
        {
            if (_timer <= 0f)
            {
                Offset = Vector2.Zero;
                return;
            }

            _timer -= dt;

            float t = _timer / _duration; // 1 -> 0
            float currentStrength = _strength * t * t; // smooth falloff, squared

            Offset = new Vector2(
                ((float)_rng.NextDouble() * 2f - 1f) * currentStrength,
                ((float)_rng.NextDouble() * 2f - 1f) * currentStrength
            );
        }
    }
}
