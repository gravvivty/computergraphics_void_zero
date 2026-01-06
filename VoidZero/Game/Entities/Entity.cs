using OpenTK.Mathematics;
using System.Drawing;
using VoidZero.Graphics;

namespace VoidZero.Game.Entities
{
    public abstract class Entity
    {
        public Texture2D Texture { get; protected set; }

        public float MaxHealth { get; protected set; }
        public float CurrentHealth { get; protected set; }

        private Vector2 _position;
        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                // Always keep relative position up-to-date
                RelativePosition = new Vector2(
                    _position.X / GameServices.Instance.Settings.Width,
                    _position.Y / GameServices.Instance.Settings.Height
                );
            }
        }
        public Vector2 RelativePosition { get; protected set; } // relative to screen
        public Vector2 Velocity;

        public float Speed { get; protected set; }

        public float Width { get; protected set; }
        public float Height { get; protected set; }

        // Placeholder hitbox for later collision work
        public RectangleF Hitbox
        {
            get
            {
                float shrinkFactor = 0.25f; // 25%
                float offsetX = Width * (shrinkFactor / 2f);
                float offsetY = Height * (shrinkFactor / 2f);
                float hitboxWidth = Width * (1f - shrinkFactor);
                float hitboxHeight = Height * (1f - shrinkFactor);

                return new RectangleF(Position.X + offsetX, Position.Y + offsetY, hitboxWidth, hitboxHeight);
            }
        }

        protected Entity(Texture2D texture, Vector2 position, float width, float height)
        {
            Texture = texture;
            Position = position;
            Width = width;
            Height = height;

            MaxHealth = 100f;
            CurrentHealth = MaxHealth;

            RelativePosition = new Vector2(
                position.X / GameServices.Instance.Settings.Width,
                position.Y / GameServices.Instance.Settings.Height
            );
        }

        public abstract void Update(float dt);
        public abstract void Draw(SpriteBatch spriteBatch);

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
    }
}