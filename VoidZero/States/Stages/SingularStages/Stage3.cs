using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;

namespace VoidZero.States.Stages
{
    public class Stage3 : IStageDefinition
    {
        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                EnemySpawn.At(10f).From(6,0).MoveTo(6,3).DummyBoss(BulletEnergy.Purple),
            });
        }
    }
}
