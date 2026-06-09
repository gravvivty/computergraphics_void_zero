using VoidZero.States.GameStates;

namespace VoidZero.States.Stages
{
    // With this we can design levels 
    public class StageComposer
    {
        private readonly List<StageEvent> _events;
        private float _time;
        private int _currentIndex;

        public StageComposer(List<StageEvent> events)
        {
            _events = events.OrderBy(e => e.TriggerTime).ToList();
            _time = 0f;
            _currentIndex = 0;
        }

        public void Update(float dt, PlayState state)
        {
            _time += dt;

            while (_currentIndex < _events.Count && _events[_currentIndex].TriggerTime <= _time)
            {
                _events[_currentIndex].Execute(state);
                _currentIndex++;
            }
        }
    }
}
