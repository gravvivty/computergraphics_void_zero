using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.States.Stages.VoidZero.States.Stages;

namespace VoidZero.States.Stages
{
    public class Stage1 : IStageDefinition
    {
        private Vector2 RelativeCenter(float xPercent, float yPercent, Entity entity)
        {
            return new Vector2(
                xPercent * GameServices.Instance.Settings.Width - entity.Width / 2f,
                yPercent * GameServices.Instance.Settings.Height - entity.Height / 2f
            );
        }

        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                new SpawnEnemyEvent(0f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new CardinalSeperatorWeave(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets
                    );
                    enemy.SetPositionRelative(
                        RelativeCenter(0.666f, -0.2f, enemy),
                        GameServices.Instance.Settings.Width,
                        GameServices.Instance.Settings.Height
                    );
                    enemy.Movement = new MovementTool(Vector2.UnitY, 1500f, 0.3f);
                    enemy.Components.Add(new TimedExitComponent(
                        waitTime: 15f,
                        exitDirection: -Vector2.UnitY,
                        exitSpeed: 1000f,
                        moveDuration: 0.5f
                    ));
                    enemy.SetBulletEnergy(BulletEnergy.Green);
                    return enemy;
                }),
                new SpawnEnemyEvent(0f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new CardinalSeperatorWeave(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets
                    );
                    enemy.SetPositionRelative(
                        RelativeCenter(0.333f, 1.2f, enemy),
                        GameServices.Instance.Settings.Width,
                        GameServices.Instance.Settings.Height
                    );
                    enemy.Movement = new MovementTool(-Vector2.UnitY, 1500f, 0.3f);
                    enemy.Components.Add(new TimedExitComponent(
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1000f,
                        moveDuration: 0.5f
                    ));
                    enemy.SetBulletEnergy(BulletEnergy.Green);
                    return enemy;
                }),
                new SpawnEnemyEvent(0f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new CardinalSpinner(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets
                    );
                    enemy.SetPositionRelative(
                        RelativeCenter(0.5f, 1.2f, enemy),
                        GameServices.Instance.Settings.Width,
                        GameServices.Instance.Settings.Height
                    );
                    enemy.Movement = new MovementTool(-Vector2.UnitY, 1500f, 0.45f);
                    enemy.Components.Add(new TimedExitComponent(
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1000f,
                        moveDuration: 0.5f
                    ));
                    enemy.SetBulletEnergy(BulletEnergy.Red);
                    return enemy;
                }),
            });
        }
    }
}
