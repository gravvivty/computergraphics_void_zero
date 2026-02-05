using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities;
using System;
using VoidZero.Game.Entities.Tools;

namespace VoidZero.Game.Entities.Enemies
{
    public class Spinner : Enemy
    {
        private ShooterComponent _shooter;
        public ShooterComponent Shooter => _shooter;

        public Spinner(Texture2D texture, Vector2 position, BulletManager bulletManager)
            : base(texture, position, 24, 24)
        {
            MaxHealth = 300f;
            CurrentHealth = MaxHealth;
            Scale = 6f;
            Width = 24 * Scale;
            Height = 24 * Scale;

            Animations.Add("Idle", new Animation(texture, 24, 24, 3, 1f, 0));
            Animations.Play("Idle");

            // Setup shooter with CardinalPattern
            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new CardinalPattern(bulletTex, 500f),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 0.1f,
                damage: 1f
            );
            _shooter.BulletEnergy = BulletEnergy.Red;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Movement?.Update(this, dt);

            _shooter.TryShoot(this, dt, trigger: true);

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
