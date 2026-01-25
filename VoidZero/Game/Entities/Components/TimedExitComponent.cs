using OpenTK.Mathematics;
using VoidZero.Game.Entities;

namespace VoidZero.Game.Entities.Components
{
    public class TimedExitComponent : IEntityComponent
    {
        private enum ExitState
        {
            Waiting,
            Moving,
            Done
        }

        private readonly float _waitTime;
        private readonly Vector2 _exitDirection;
        private readonly float _exitSpeed;
        private readonly float _moveDuration;

        private float _timer;
        private float _moveTimer;
        private ExitState _state;

        public bool IsExpired { get; private set; }

        public TimedExitComponent(
            float waitTime,
            Vector2 exitDirection,
            float exitSpeed,
            float moveDuration)
        {
            _waitTime = waitTime;
            _exitDirection = exitDirection.Normalized();
            _exitSpeed = exitSpeed;
            _moveDuration = moveDuration;
        }

        public void Attach(Entity entity)
        {
            _timer = 0f;
            _moveTimer = 0f;
            _state = ExitState.Waiting;
            IsExpired = false;
        }

        public void Update(Entity entity, float dt)
        {
            switch (_state)
            {
                case ExitState.Waiting:
                    _timer += dt;
                    if (_timer >= _waitTime)
                    {
                        entity.Movement = new MovementTool(
                            _exitDirection,
                            _exitSpeed,
                            duration: _moveDuration
                        );

                        _state = ExitState.Moving;
                    }
                    break;

                case ExitState.Moving:
                    _moveTimer += dt;
                    if (_moveTimer >= _moveDuration)
                    {
                        IsExpired = true;
                        _state = ExitState.Done;
                    }
                    break;
            }
        }
    }
}
