using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities;
using System;
using VoidZero.Game.Entities.Tools;
using System.Runtime.CompilerServices;
using VoidZero.Game.Entities.Components;

namespace VoidZero.Game.Entities.Enemies
{
    public class CardinalSeperatorWeave : Enemy
    {
        private ShooterComponent _shooter;
        public ShooterComponent Shooter => _shooter;

        public CardinalSeperatorWeave(Texture2D texture, Vector2 position, BulletManager bulletManager)
            : base(texture, position, 24, 24)
        {
            MaxHealth = 60f;
            CurrentHealth = MaxHealth;
            Scale = 6f;
            Width = 24 * Scale;
            Height = 24 * Scale;

            Animations.Add("Idle", new Animation(texture, 24, 24, 3, 1f, 0));

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new SpreadPattern(bulletTex, Vector2.UnitY, 4, 90, 500f, true),
                bulletManager,
                BulletOwner.Enemy,
                cooldown: 0.25f,
                damage: 1f
            );
            _shooter.BulletEnergy = BulletEnergy.Red;
        }

        public override void Update(float dt)
        {
            Movement?.Update(this, dt);

            foreach (var component in Components)
                component.Update(this, dt);

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
