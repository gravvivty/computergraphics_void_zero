using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.States.Stages.VoidZero.States.Stages;

namespace VoidZero.States.Stages
{
    // Each Stage has a build which defines which enemies, spawn where and when, what their bullet color is
    // and when they would leave the screen if they are not killed
    public class Stage1 : IStageDefinition
    {
        private Vector2 RelativeCenter(float xPercent, float yPercent, Entity entity)
        {
            return new Vector2(
                xPercent * GameServices.Instance.Settings.WorldWidth - entity.Width / 2f,
                yPercent * GameServices.Instance.Settings.WorldHeight - entity.Height / 2f
            );
        }

        public StageComposer Build()
        {
            return new StageComposer(new()
            {
                new SpawnEnemyEvent(0f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.33f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.3f),
                        waitTime: 15f,
                        exitDirection: -Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(5f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Yellow
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.66f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.3f),
                        waitTime: 15f,
                        exitDirection: -Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(7f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new OmniShotRotate(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    enemy.SetPosition(
                        RelativeCenter(-0.2f, 0.5f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitX, 0f, 0f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(13f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Purple
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.33f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.25f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(13f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Purple
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.66f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.25f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(18f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new OmniShotRotate(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.5f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.25f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(22f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new OmniShotRotate(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Yellow
                    );
                    enemy.SetPosition(
                        RelativeCenter(1.2f, 0.5f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 0f, 0f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    return enemy;
                }),
                new SpawnEnemyEvent(27f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.33f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.4f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    enemy.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 4f, 0f, 1f)
                        ));
                    return enemy;
                }),
                new SpawnEnemyEvent(30f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Blue
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.66f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.2f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    enemy.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 4f, 0f, 1f)
                        ));
                    return enemy;
                }),
                new SpawnEnemyEvent(33f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Purple
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.25f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.2f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    enemy.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 4f, 0f, 1f)
                        ));
                    return enemy;
                }),
                new SpawnEnemyEvent(36f, state =>
                {
                    var enemyTex = GameServices.Instance.Content.GetTexture("witch");
                    var enemy = new TripleShot(
                        enemyTex,
                        position: Vector2.Zero,
                        bulletManager: state.Bullets,
                        energy: BulletEnergy.Purple
                    );
                    enemy.SetPosition(
                        RelativeCenter(0.75f, -0.2f, enemy)
                    );
                    enemy.AddComponent(new MovementLifecycleComponent(
                        entryMovement: new MovementTool(Vector2.UnitY, 1500f, 0.5f),
                        waitTime: 15f,
                        exitDirection: Vector2.UnitY,
                        exitSpeed: 1500f,
                        exitDuration: 0.5f
                    ));
                    enemy.AddComponent(new RotationSequenceComponent(
                        loop: true,
                        new RotationComponent(MathF.PI / 4f, 0f, 1f)
                        ));
                    return enemy;
                }),
            });
        }
    }
}
