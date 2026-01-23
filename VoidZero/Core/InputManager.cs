using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace VoidZero.Game.Input
{
    public class InputManager
    {
        private KeyboardState _keyboard;

        private bool _pausePressed;
        private bool _pauseConsumed;

        public Vector2 MoveAxis { get; private set; }
        public bool ShootHeld { get; private set; }
        public bool SwitchShieldPressed { get; private set; }

        public bool SwitchPatternPressed { get; private set; }

        public void Update(GameWindow window)
        {
            _keyboard = window.KeyboardState;

            Vector2 axis = Vector2.Zero;
            if (_keyboard.IsKeyDown(Keys.W)) axis.Y -= 1;
            if (_keyboard.IsKeyDown(Keys.S)) axis.Y += 1;
            if (_keyboard.IsKeyDown(Keys.A)) axis.X -= 1;
            if (_keyboard.IsKeyDown(Keys.D)) axis.X += 1;

            if (axis.LengthSquared > 1)
            {
                axis = axis.Normalized();
            }
            MoveAxis = axis;

            ShootHeld = _keyboard.IsKeyDown(Keys.J);

            if (_keyboard.IsKeyPressed(Keys.Escape))
            {
                _pausePressed = true;
                _pauseConsumed = false;
            }

            SwitchPatternPressed = _keyboard.IsKeyPressed(Keys.L);
            SwitchShieldPressed = _keyboard.IsKeyPressed(Keys.K);
        }

        public bool ConsumePausePressed()
        {
            if (_pausePressed && !_pauseConsumed)
            {
                _pauseConsumed = true;
                return true;
            }
            return false;
        }
    }
}