using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using System.Collections.Generic;

namespace VoidZero.Game.Entities
{
    public class Bullet : Entity
    {
        public Vector2 Direction { get; set; }
        public float Lifetime { get; set; } = 10f;

        public BulletEnergy Energy { get; set; }
        public BulletOwner Owner { get; set; }

        public float Damage { get; set; }

        // For later collision logic
        public HashSet<Entity> HitEntities { get; } = new();

        public Bullet(
            Texture2D texture,
            Vector2 position,
            Vector2 direction,
            float speed,
            float damage,
            BulletEnergy energy,
            BulletOwner owner)
            : base(texture, position, 8, 8)
        {
            Direction = direction.Normalized();
            Speed = speed;
            Energy = energy;
            Owner = owner;
            Damage = damage;

            Scale = 4f;
            Width = 16 * Scale;
            Height = 16 * Scale;

            Animations.Add("Travel", new Animation(texture, 16, 16, 4, 0.08f));
        }

        public override void Update(float dt)
        {
            Position += Direction * Speed * dt;
            Lifetime -= dt;

            UpdateAnimation("Travel", dt);
        }

        public bool IsExpired => Lifetime <= 0f;
    }
}
