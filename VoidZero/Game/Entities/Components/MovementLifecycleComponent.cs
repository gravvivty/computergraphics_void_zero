using OpenTK.Mathematics;
using VoidZero.Game.Entities;

namespace VoidZero.Game.Entities.Components
{
    public class MovementLifecycleComponent : IEntityComponent
    {
        private enum Phase { Entering, Waiting, Exiting, Done }

        private readonly MovementTool _entryMovement;
        private readonly float _waitTime;
        private readonly Vector2 _exitDirection;
        private readonly float _exitSpeed;
        private readonly float _exitDuration;

        private Phase _phase;
        private float _waitTimer;

        public bool IsActive { get; private set; } = false;

        public bool IsExpired { get; private set; }

        public MovementLifecycleComponent(
            MovementTool entryMovement,
            float waitTime,
            Vector2 exitDirection,
            float exitSpeed,
            float exitDuration)
        {
            _entryMovement = entryMovement;
            _waitTime = waitTime;
            _exitDirection = exitDirection.Normalized();
            _exitSpeed = exitSpeed;
            _exitDuration = exitDuration;
        }

        public void Attach(Entity entity)
        {
            _phase = _entryMovement != null ? Phase.Entering : Phase.Waiting;
            _waitTimer = 0f;
            IsExpired = false;

            if (_entryMovement != null)
                entity.Movement = _entryMovement;
        }

        public void Update(Entity entity, float dt)
        {
            switch (_phase)
            {
                case Phase.Entering:
                    entity.Movement?.Update(entity, dt);
                    if (entity.Movement == null || entity.Movement.Finished)
                    {
                        IsActive = true;
                        entity.Movement = null;
                        _phase = Phase.Waiting;
                    }
                    break;

                case Phase.Waiting:
                    entity.Movement?.Update(entity, dt);
                    _waitTimer += dt;
                    if (_waitTimer >= _waitTime)
                    {
                        IsActive = false;
                        entity.Movement = new MovementTool(_exitDirection, _exitSpeed, _exitDuration);

                        // flush any accumulated cooldown so the shooter doesn't
                        // fire one last shot on the first exiting frame
                        var shooter = entity.Components.OfType<ShooterComponent>().FirstOrDefault();
                        shooter?.ResetTimer();

                        _phase = Phase.Exiting;
                    }
                    break;

                case Phase.Exiting:
                    entity.Movement?.Update(entity, dt);
                    if (entity.Movement == null || entity.Movement.Finished)
                    {
                        IsExpired = true;
                        _phase = Phase.Done;
                    }
                    break;
            }
        }
    }
}