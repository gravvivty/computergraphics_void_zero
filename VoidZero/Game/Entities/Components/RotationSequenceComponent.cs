using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Game.Entities.Components
{
    public class RotationSequenceComponent : IEntityComponent
    {
        private Queue<RotationComponent> _steps = new();
        private readonly List<RotationComponent> _originalSteps = new();
        private readonly bool _loop;
        private RotationComponent _current;

        public RotationSequenceComponent(bool loop = false, params RotationComponent[] steps)
        {
            _loop = loop;
            _originalSteps.AddRange(steps);
        }

        public void Attach(Entity entity)
        {
            ResetQueue(entity);
        }

        public void Update(Entity entity, float dt)
        {
            _current?.Update(entity, dt);

            if (_current != null && _current.Finished)
            {
                _steps.Dequeue();

                if (_steps.Count == 0)
                {
                    if (_loop)
                        ResetQueue(entity);
                    else
                        _current = null;

                    return;
                }

                _current = _steps.Peek();
                _current.Attach(entity);
            }
        }

        private void ResetQueue(Entity entity)
        {
            _steps.Clear();

            foreach (var step in _originalSteps)
            {
                var copy = new RotationComponent(step.Delta, step.Duration, step.PauseAfter);
                copy.Attach(entity);
                _steps.Enqueue(copy);
            }

            _current = _steps.Count > 0 ? _steps.Peek() : null;
        }
    }

}
