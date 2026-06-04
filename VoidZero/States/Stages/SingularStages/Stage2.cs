using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.States.Stages.VoidZero.States.Stages;

namespace VoidZero.States.Stages
{
    public class Stage2 : IStageDefinition
    {
        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                new SpawnEnemyEvent(0f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new CardinalSpinner(
                        texture: spinnerTexture,
                        position: new Vector2(800, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Red
                    );
                    spinner.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 100f, 2f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 250f,
                        exitDuration : 3f));
                    spinner.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 2f, 1f)
                    ));
                    return spinner;
                }),
                new SpawnEnemyEvent(0f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new CardinalSequencer(
                        texture: spinnerTexture,
                        position: new Vector2(300, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    spinner.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.3f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 1500f,
                        exitDuration : 0.5f));
                    return spinner;
                })
            });
        }
    }
}
