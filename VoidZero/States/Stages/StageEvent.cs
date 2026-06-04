namespace VoidZero.States.Stages
{
    public abstract class StageEvent
    {
        public float TriggerTime { get; }

        protected StageEvent(float triggerTime)
        {
            TriggerTime = triggerTime;
        }

        public abstract void Execute(PlayState state);
    }

}
