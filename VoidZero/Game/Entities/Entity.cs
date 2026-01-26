using OpenTK.Mathematics;
using System.Drawing;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities.Components;
using VoidZero.Graphics;
using VoidZero.Utils;

namespace VoidZero.Game.Entities
{
    public abstract class Entity
    {
        public Texture2D Texture { get; protected set; }
        public List<IEntityComponent> Components { get; } = new();
        public float Rotation { get; set; }
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; }
        public float Scale { get; set; } = 6f;
        public MovementTool Movement { get; set; }
        public Vector2 RelativePosition { get; protected set; } // Relative to screen
        public Vector2 Velocity;
        public float Speed { get; protected set; }
        public float Width { get; protected set; }
        public float Height { get; protected set; }
        protected AnimationManager Animations { get; }
        private Vector2 _position;
        // Death
        public bool IsDead { get; protected set; } = false;
        public bool IsDying { get; private set; } = false;
        protected virtual string DeathAnimationKey => "Death";
        // How long before cleanup (fallback if animation length unknown)
        protected virtual float DeathDuration => 0.4f;

        protected float _deathTimer = 0f;
        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                // Always keep relative position up to date
                RelativePosition = new Vector2(
                    _position.X / GameServices.Instance.Settings.Width,
                    _position.Y / GameServices.Instance.Settings.Height
                );
            }
        }

        protected Entity(Texture2D texture, Vector2 position, float width, float height)
        {
            Texture = texture;
            Position = position;
            Width = width;
            Height = height;

            CurrentHealth = MaxHealth;

            RelativePosition = new Vector2(
                position.X / GameServices.Instance.Settings.Width,
                position.Y / GameServices.Instance.Settings.Height
            );

            Animations = new AnimationManager();
            AddDefaultDeathAnimation();
        }
        public virtual RectangleF Hitbox
        {
            get
            {
                if (IsDying)
                    return RectangleF.Empty;

                float shrinkFactor = 0.25f; // 25%
                float offsetX = Width * (shrinkFactor);
                float offsetY = Height * (shrinkFactor);
                float hitboxWidth = Width * (1f - 2f * shrinkFactor);
                float hitboxHeight = Height * (1f - 2f * shrinkFactor);

                return new RectangleF(Position.X + offsetX, Position.Y + offsetY, hitboxWidth, hitboxHeight);
            }
        }
        public virtual void Update(float dt)
        {
            if (IsDying)
            {
                Animations.Update(dt);
                UpdateDeath(dt);
                return;
            }

            foreach (var component in Components)
                component.Update(this, dt);
        }

        public virtual void Draw(SpriteBatch batch)
        {
            // Default tint
            Vector4 tint = (this is Bullet b) ? BulletColorHelper.GetTint(b.Energy) : Vector4.One;

            // Calculate draw position (center death animation if dying)
            Vector2 drawPos = Position;

            if (IsDying && Animations.CurrentAnimationKey == DeathAnimationKey)
            {
                var anim = Animations.CurrentAnimation;
                if (anim != null)
                {
                    // Center the animation over the entity
                    drawPos += new Vector2(Width, Height) / 2f - new Vector2(anim.FrameWidth, anim.FrameHeight) * 0.5f * Scale;
                }
            }

            // Draw animation at calculated position
            Animations.Draw(batch, drawPos, Scale, tint, Rotation);

            // Debug hitbox
            batch.DrawRectangle(Hitbox, Color.Red);
        }

        public void SetPositionRelative(Vector2 absolutePos, int screenWidth, int screenHeight)
        {
            _position = absolutePos;
            RelativePosition = new Vector2(
                absolutePos.X / screenWidth,
                absolutePos.Y / screenHeight
            );
        }

        public void UpdateAbsolutePosition(int screenWidth, int screenHeight)
        {
            Position = new Vector2(RelativePosition.X * screenWidth, RelativePosition.Y * screenHeight);
        }

        public void OnResize(int newWidth, int newHeight)
        {
            // Only recalc absolute position from relative
            _position = new Vector2(RelativePosition.X * newWidth, RelativePosition.Y * newHeight);
        }

        protected void AddDefaultDeathAnimation()
        {
            Texture2D deathTexture = GameServices.Instance.Content.GetTexture("death");

            Animations.Add(
                "Death",
                new Animation(deathTexture, 32, 32, 10, 0.08f, loop: false)
            );
        }

        public void AddComponent(IEntityComponent component)
        {
            component.Attach(this);
            Components.Add(component);
        }

        public virtual void Kill()
        {
            if (IsDying) return;

            IsDying = true;
            Velocity = Vector2.Zero;
            Components.Clear(); // optional, prevents logic during death
            Animations.Play(DeathAnimationKey);

            _deathTimer = DeathDuration;
        }

        protected void UpdateDeath(float dt)
        {
            if (!IsDying)
                return;

            _deathTimer -= dt;

            if (_deathTimer <= 0f)
            {
                IsDead = true;
            }
        }
    }
}