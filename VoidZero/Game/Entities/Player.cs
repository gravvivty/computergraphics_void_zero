using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Input;

namespace VoidZero.Game.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 8000f;
        private readonly float _deceleration = 5000f;
        private readonly float _maxSpeed = 1000f;

        public Player(Texture2D texture, Vector2 startPos, InputManager input)
            : base(texture, startPos, 128, 128)
        {
            _input = input;
        }

        public override void Update(float dt)
        {
            Vector2 inputDir = Vector2.Zero;

            if (_input.MoveUp) inputDir.Y -= 1;
            if (_input.MoveDown) inputDir.Y += 1;
            if (_input.MoveLeft) inputDir.X -= 1;
            if (_input.MoveRight) inputDir.X += 1;

            if (inputDir.LengthSquared > 0)
            {
                inputDir = inputDir.Normalized();
                Velocity += inputDir * _acceleration * dt;
            }
            else if (Velocity.LengthSquared > 0)
            {
                Vector2 decel = Velocity.Normalized() * _deceleration * dt;
                Velocity = decel.LengthSquared > Velocity.LengthSquared ? Vector2.Zero : Velocity - decel;
            }

            if (Velocity.Length > _maxSpeed)
            {
                Velocity = Velocity.Normalized() * _maxSpeed;
            }

            // Move the player
            Position += Velocity * dt;


            // Hooks for later
            if (_input.Shoot)
            {
                // TODO: Shoot
            }

            if (_input.SwitchShield)
            {
                // TODO: Switch shield
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, new Vector2(Width, Height));
        }
    }
}