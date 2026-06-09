using VoidZero.Game.Entities;
using VoidZero.States.GameStates;

namespace VoidZero.States.Stages
{
    public class SpawnEnemyEvent : StageEvent
    {
        private readonly Func<PlayState, Entity> _factory;

        public SpawnEnemyEvent(float triggerTime, Func<PlayState, Entity> factory)
            : base(triggerTime)
        {
            _factory = factory;
        }

        public override void Execute(PlayState state)
        {
            Entity enemy = _factory(state);
            state.Entities.Add(enemy);
        }
    }

}
