using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;
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
                    var threeFork = new ThreeFork(
                        threeForkTexture,
                        position: new Vector2(600, -10),
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Red
                    );
                    threeFork.Movement = new MovementTool(Vector2.UnitY, 100f, 2f);
                    threeFork.AddComponent(new TimedExitComponent(waitTime : 8f, exitDirection : -Vector2.UnitY, exitSpeed : 250f, moveDuration : 3f));
                    return threeFork;
                }),

                new SpawnEnemyEvent(4f, state =>
                {
                    var spinnerTexture = GameServices.Instance.Content.GetTexture("witch");
                    var spinner = new Spinner(
                        spinnerTexture,
                        position: new Vector2(200, -10),
                        bulletManager: state.Bullets
                    );
                    spinner.Movement = new MovementTool(Vector2.UnitY, 100f, 2f);
                    spinner.Components.Add(new TimedExitComponent(waitTime : 8f, exitDirection : -Vector2.UnitY, exitSpeed : 250f, moveDuration : 3f));
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
                    spinner.Movement = new MovementTool(Vector2.UnitY, 100f, 2f);
                    spinner.AddComponent(new TimedExitComponent(waitTime : 8f, exitDirection : -Vector2.UnitY, exitSpeed : 250f, moveDuration : 3f));
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
                    spinner.Movement = new MovementTool(Vector2.UnitY, 100f, 2f);
                    spinner.AddComponent(new TimedExitComponent(waitTime : 8f, exitDirection : -Vector2.UnitY, exitSpeed : 250f, moveDuration : 3f));
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
