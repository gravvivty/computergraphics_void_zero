using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;

namespace VoidZero.Game.Entities.Enemies
{
    public class StationaryEnemy : Entity
    {
        private ShooterComponent _shooter;

        public StationaryEnemy(Texture2D texture, Vector2 position, BulletManager bulletManager)
            : base(texture, position, 24, 24)
        {
            MaxHealth = 50f;
            CurrentHealth = MaxHealth;
            Scale = 6f;
            Width = 24 * Scale;
            Height = 24 * Scale;

            Animations.Add("Idle", new Animation(texture, 24, 24, 3, 1f, 0));

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new ThreeWaySpreadPattern(bulletTex, Vector2.UnitY, 40f, 500f),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 1.0f,
                damage: 5f
            );
            _shooter.BulletEnergy = BulletEnergy.Yellow;
        }

        public override void Update(float dt)
        {
            Movement?.Update(this, dt);

            _shooter.TryShoot(this, dt, trigger: true);

            // Animate idle
            UpdateAnimation("Idle", dt);
        }
    }
}
