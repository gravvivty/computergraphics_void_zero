using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using VoidZero.Game;
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
                    var spinner = new Spinner(
                        texture: spinnerTexture,
                        position: new Vector2(800, -10),
                        bulletManager: state.Bullets
                    );
                    spinner.Movement = new MovementTool(Vector2.UnitY, 100f, 2f);
                    spinner.AddComponent(new TimedExitComponent(waitTime : 8f, exitDirection : -Vector2.UnitY, exitSpeed : 250f, moveDuration : 3f));
                    spinner.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 2f, 0f, 1f),
                        new RotationComponent(MathF.PI / 2f, 0f, 1f),
                        new RotationComponent(MathF.PI / 2f, 0f, 1f),
                        new RotationComponent(MathF.PI / 2f, 0f, 1f)
                    ));
                    return spinner;
                })
            });
        }
    }
}
