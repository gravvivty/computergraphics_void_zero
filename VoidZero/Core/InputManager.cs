using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace VoidZero.Game.Input
{
    public class InputManager
    {
        private KeyboardState _keyboard;
        private GamepadState _gamepad;
        private bool _gamepadConnected;

        private const int GamepadId = 0; // GLFW joystick 0
        private const float StickDeadZone = 0.2f;
        private bool _prevShieldButton;
        private bool _prevStartButton;

        private bool _pausePressed;
        private bool _pauseConsumed;

        public Vector2 MoveAxis { get; private set; }
        public bool ShootHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool SwitchShieldPressed { get; private set; }
        public bool SwitchPatternPressed { get; private set; }

        public unsafe void Update(GameWindow window)
        {
            _keyboard = window.KeyboardState;

            // Gamepad polling
            _gamepadConnected =
                GLFW.JoystickPresent(GamepadId) &&
                GLFW.JoystickIsGamepad(GamepadId);

            if (_gamepadConnected)
            {
                GLFW.GetGamepadState(GamepadId, out _gamepad);
            }
            Vector2 axis = Vector2.Zero;

            // Keyboard
            if (_keyboard.IsKeyDown(Keys.W)) axis.Y -= 1;
            if (_keyboard.IsKeyDown(Keys.S)) axis.Y += 1;
            if (_keyboard.IsKeyDown(Keys.A)) axis.X -= 1;
            if (_keyboard.IsKeyDown(Keys.D)) axis.X += 1;

            // Gamepad left stick
            if (_gamepadConnected)
            {
                float lx = _gamepad.Axes[0];
                float ly = _gamepad.Axes[1];

                Vector2 stick = new Vector2(lx, ly);

                if (stick.Length >= StickDeadZone)
                {
                    axis = stick;
                }
            }

            if (axis.LengthSquared > 1f)
            {
                axis = axis.Normalized();
            }

            MoveAxis = axis;

            // Actions
            ShootHeld =
                _keyboard.IsKeyDown(Keys.J) ||
                (_gamepadConnected && _gamepad.Axes[5] > 0.5f); // RT, J


            DashPressed =
                _keyboard.IsKeyPressed(Keys.Space) ||
                (_gamepadConnected && _gamepad.Buttons[0] == 1); // A, Space


            bool shieldHeld = _gamepadConnected && _gamepad.Buttons[2] == 1; // X, K
            SwitchShieldPressed =
                _keyboard.IsKeyPressed(Keys.K) ||
                (shieldHeld && !_prevShieldButton);
            _prevShieldButton = shieldHeld;


            SwitchPatternPressed =
                _keyboard.IsKeyPressed(Keys.L) ||
                (_gamepadConnected && _gamepad.Buttons[3] == 1); // Y, L


            bool startHeld = _gamepadConnected && _gamepad.Buttons[7] == 1; // Start, ESC
            if (_keyboard.IsKeyPressed(Keys.Escape) ||
                (startHeld && !_prevStartButton))
            {
                _pausePressed = true;
                _pauseConsumed = false;
            }

            _prevStartButton = startHeld;
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
