using OpenTK.Mathematics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities.Components;
using VoidZero.Graphics;

namespace VoidZero.Game.Entities.Enemies.Bosses
{
    public class DummyBoss : Enemy
    {
        private ShooterComponent _shooter;
        public ShooterComponent Shooter => _shooter;

        public DummyBoss(Texture2D texture, Vector2 position, BulletManager bulletManager, BulletEnergy energy)
            : base(texture, position, 24, 24)
        {
            MaxHealth = 500f;
            CurrentHealth = MaxHealth;
            Scale = 10f;
            Width = 24 * Scale;
            Height = 24 * Scale;
            IsBoss = true;

            Animations.Add("Idle", new Animation(texture, 24, 24, 3, 1f, 0));
            Animations.Play("Idle");

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new SpreadPattern(bulletTex, Vector2.UnitY, 4, 90, 500f, true),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 0.1f,
                damage: 1f
            );
            _shooter.BulletEnergy = energy;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            _shooter.TryShoot(this, dt, trigger: CanShoot);

            Animations.Update(dt);
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
