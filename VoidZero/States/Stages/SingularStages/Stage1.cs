using VoidZero.Game.Combat;

namespace VoidZero.States.Stages
{
    public class Stage1 : IStageDefinition
    {
        public StageComposer Build() => new StageComposer(new()
        {
            EnemySpawn.At(0f) .From(4,0).MoveTo(4,2).TripleShot(BulletEnergy.Blue),
            EnemySpawn.At(5f) .From(8,0).MoveTo(8,2).TripleShot(BulletEnergy.Yellow),

            EnemySpawn.At(7f) .From(0,3).MoveTo(0,6).OmniShotRotate(BulletEnergy.Blue),

            EnemySpawn.At(13f).From(5,0).MoveTo(5,2).TripleShot(BulletEnergy.Purple),
            EnemySpawn.At(13f).From(7,0).MoveTo(7,2).TripleShot(BulletEnergy.Purple),

            EnemySpawn.At(18f).From(6,0).MoveTo(6,3).OmniShotRotate(BulletEnergy.Blue),

            EnemySpawn.At(22f).From(12,3).MoveTo(12,6).OmniShotRotate(BulletEnergy.Yellow),

            EnemySpawn.At(27f).From(4,0).MoveTo(4,2).WithRotation(MathF.PI/4f, 1f).TripleShot(BulletEnergy.Blue),
            EnemySpawn.At(30f).From(8,0).MoveTo(8,4).WithRotation(MathF.PI/4f, 1f).TripleShot(BulletEnergy.Blue),
            EnemySpawn.At(33f).From(5,0).MoveTo(5,6).WithRotation(MathF.PI/4f, 1f).TripleShot(BulletEnergy.Purple),
            EnemySpawn.At(36f).From(7,0).MoveTo(7,5).WithRotation(MathF.PI/4f, 1f).TripleShot(BulletEnergy.Purple),

            EnemySpawn.At(40f).From(6,0).MoveTo(6,3).DummyBoss(BulletEnergy.Purple),
        });
    }
}