using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using System.Collections.Generic;
using System.Drawing;

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

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            // Debug grazing
            batch.DrawRectangle(GrazeHitbox, Color.White);
        }

        public bool IsExpired => Lifetime <= 0f;

        public override RectangleF Hitbox
        {
            get
            {
                float shrinkFactor = 0.3f; // shrink 30% from each side
                float offsetX = Width * shrinkFactor;
                float offsetY = Height * shrinkFactor;
                float hitboxWidth = Width * (1f - 2f * shrinkFactor);
                float hitboxHeight = Height * (1f - 2f * shrinkFactor);

                return new RectangleF(Position.X + offsetX, Position.Y + offsetY, hitboxWidth, hitboxHeight);
            }
        }

        public RectangleF GrazeHitbox
        {
            get
            {
                float expandFactor = 0.3f; // 30% outward
                float expandX = Width * expandFactor;
                float expandY = Height * expandFactor;

                return new RectangleF(
                    Position.X - expandX,
                    Position.Y - expandY,
                    Width + expandX * 2f,
                    Height + expandY * 2f
                );
            }
        }
    }
}
