using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;

namespace VoidZero.States.Stages
{
    public class Stage2 : IStageDefinition
    {
        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                EnemySpawn.At(0f).From(5,0).MoveTo(5,3).WithRotation(MathF.PI/2f, 1f).CardinalSpinner(BulletEnergy.Yellow),
                EnemySpawn.At(0f).From(3,0).MoveTo(3,5).CardinalSequencer(BulletEnergy.Blue),
                EnemySpawn.At(10f).From(6,0).MoveTo(6,3).DummyBoss(BulletEnergy.Purple),
            });
        }
    }
}
