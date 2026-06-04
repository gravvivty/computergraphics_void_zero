using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.States.Stages.VoidZero.States.Stages;

namespace VoidZero.States.Stages
{
    public class Stage3 : IStageDefinition
    {
        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                new SpawnEnemyEvent(0f, state =>
                {
                    var threeForkTexture = GameServices.Instance.Content.GetTexture("witch");
                    var threeFork = new CardinalSpinner(
                        threeForkTexture,
                        position: new Vector2(600, -50),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Yellow
                    );
                    threeFork.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1000f, 0.5f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 250f,
                        exitDuration : 3f));
                    return threeFork;
                }),

                new SpawnEnemyEvent(4f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new CardinalSpinner(
                        spinnerTexture,
                        position: new Vector2(200, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Purple
                    );
                    spinner.Components.Add(new MovementLifecycleComponent(
                        entryMovement : new MovementTool(Vector2.UnitY, 100f, 2f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 250f,
                        exitDuration : 3f));
                    spinner.AddComponent(new RotationComponent(MathF.Tau, 2f, 0f, loop: true));
                    spinner.SetBulletEnergy(BulletEnergy.Blue);
                    return spinner;
                }),

                new SpawnEnemyEvent(4f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new SpreadShotFast(
                        texture: spinnerTexture,
                        position: new Vector2(800, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    spinner.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 100f, 2f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 250f,
                        exitDuration : 3f));
                    spinner.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 2f, 0f, 1f)
                    ));
                    return spinner;
                }),
                new SpawnEnemyEvent(4f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new OmniShotRotate(
                        texture: spinnerTexture,
                        position: new Vector2(1400, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    spinner.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 100f, 2f),
                        waitTime : 8f,
                        exitDirection : -Vector2.UnitY,
                        exitSpeed : 250f,
                        exitDuration : 3f));
                    spinner.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 2f, 0f, 1f)
                    ));
                    return spinner;
                })
            });
        }
    }
}
