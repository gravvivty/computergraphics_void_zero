using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game.Entities.Components
{
    public class RotationComponent : IEntityComponent
    {
        public float Delta => _targetDelta;
        public float Duration => _duration;
        public float PauseAfter => _pauseAfter;

        private readonly float _targetDelta;
        private readonly float _duration;
        private readonly float _pauseAfter;
        private readonly bool _loop;

        private float _elapsed;
        private float _startRotation;
        private bool _finished;

        public bool Finished => _finished && !_loop;
        public float CurrentRotation
        {
            get
            {
                if (_finished && !_loop)
                    return _startRotation + _targetDelta;

                float t = _duration > 0f ? MathF.Min(_elapsed / _duration, 1f) : 1f;
                return _startRotation + _targetDelta * t;
            }
        }

        public RotationComponent(float deltaRadians, float duration, float pauseAfter = 0f, bool loop = false)
        {
            _targetDelta = deltaRadians;
            _duration = duration;
            _pauseAfter = pauseAfter;
            _loop = loop;
        }

        public void Attach(Entity entity)
        {
            _startRotation = entity.Rotation;
            _elapsed = 0f;
            _finished = false;
        }

        public void Update(Entity entity, float dt)
        {
            if (_finished && !_loop)
                return;

            _elapsed += dt;

            if (_elapsed <= _duration)
            {
                float t = _elapsed / _duration;
                entity.Rotation = _startRotation + _targetDelta * t;
            }
            else if (_elapsed <= _duration + _pauseAfter)
            {
                entity.Rotation = _startRotation + _targetDelta;
            }
            else
            {
                if (_loop)
                {
                    _elapsed = 0f;
                    _startRotation = entity.Rotation;
                }
                else
                {
                    entity.Rotation = _startRotation + _targetDelta;
                    _finished = true;
                }
            }
        }
    }
}
