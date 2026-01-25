using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Tools;

namespace VoidZero.Game.Entities.Enemies
{
    public class OmniShotRotate : Enemy
    {
        private ShooterComponent _shooter;
        public ShooterComponent Shooter => _shooter;

        public OmniShotRotate(Texture2D texture, Vector2 position, BulletManager bulletManager)
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
                new SpreadPattern(bulletTex, Vector2.UnitY, 16, 20f, 500f, true),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 0.3f,
                damage: 1f
            );
            _shooter.BulletEnergy = BulletEnergy.Red;
            RotationComponent rotationComponent = new RotationComponent(MathF.Tau, 2f, 0f, loop: true);
            AddComponent(rotationComponent);
            rotationComponent.Update(this, 0f);
        }

        public override void Update(float dt)
        {
            foreach (var component in Components)
            {
                switch (component)
                {
                    case TimedExitComponent exit:
                        exit.Update(this, dt);
                        break;

                    case RotationComponent rotation:
                        rotation.Update(this, dt);
                        break;

                    case RotationSequenceComponent sequence:
                        sequence.Update(this, dt);
                        break;
                }
            }
            Movement?.Update(this, dt);
            _shooter.TryShoot(this, dt, trigger: true);
            UpdateAnimation("Idle", dt);
        }

        public void SetBulletEnergy(BulletEnergy bulletEnergy)
        {
            _shooter.BulletEnergy = bulletEnergy;
        }

        public void SetShooterPattern(
            IBulletPattern pattern,
            float cooldown,
            float damage)
        {
            _shooter.Configure(pattern, cooldown, damage);
        }
    }
}
