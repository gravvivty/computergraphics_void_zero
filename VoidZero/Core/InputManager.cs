using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoidZero.Game.Input
{
    // Handles all player input both keyboard and controller
    public class InputManager
    {
        private KeyboardState _keyboard;
        private GamepadState _gamepad;
        private bool _gamepadConnected;
        public bool GamepadConnected => _gamepadConnected;
        public GamepadState GamepadState => _gamepad;

        private const int GamepadId = 0; // GLFW joystick 0
        private const float StickDeadZone = 0.2f;
        private bool _prevShieldButton;
        private bool _prevStartButton;
        private bool _prevDashButton;

        private bool _pausePressed;
        private bool _pauseConsumed;
        // Gamepad
        private bool _confirmWasPressed = false;
        private bool _cancelWasPressed = false;

        public Vector2 MoveAxis { get; private set; }
        public bool ShootHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool SwitchShieldPressed { get; private set; }
        public bool ActivateAbilityPressed { get; private set; }

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
            ShootHeld = _keyboard.IsKeyDown(Keys.J) || (_gamepadConnected && _gamepad.Axes[5] > 0.5f); // RT, J
            DashPressed = _keyboard.IsKeyPressed(Keys.Space) || (_gamepadConnected && _gamepad.Buttons[0] == 1); // A, Space

            bool dashButtonHeld = _gamepadConnected && _gamepad.Buttons[0] == 1;
            DashPressed = _keyboard.IsKeyPressed(Keys.Space) || (dashButtonHeld && !_prevDashButton); // Space, A
            _prevDashButton = dashButtonHeld;

            bool shieldHeld = _gamepadConnected && _gamepad.Buttons[2] == 1;
            SwitchShieldPressed = _keyboard.IsKeyPressed(Keys.K) || (shieldHeld && !_prevShieldButton);  // X, K
            _prevShieldButton = shieldHeld;


            ActivateAbilityPressed = _keyboard.IsKeyPressed(Keys.L) || (_gamepadConnected && _gamepad.Buttons[3] == 1); // Y, L

            // Toggle debug graze hitbox drawing with F3
            if (_keyboard.IsKeyPressed(Keys.F3))
            {
                GameServices.Instance.Settings.ShowGrazeHitboxes = !GameServices.Instance.Settings.ShowGrazeHitboxes;
            }
            // Toggle debug entity hitbox drawing with F4
            if (_keyboard.IsKeyPressed(Keys.F4))
            {
                GameServices.Instance.Settings.ShowHitboxes = !GameServices.Instance.Settings.ShowHitboxes;
            }


            bool startHeld = _gamepadConnected && _gamepad.Buttons[7] == 1; // Start, ESC
            if (_keyboard.IsKeyPressed(Keys.Escape) || (startHeld && !_prevStartButton))
            {
                _pausePressed = true;
                _pauseConsumed = false;
            }

            _prevStartButton = startHeld;
            UpdateGamepadActions();
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

        public void Reset()
        {
            _pausePressed = false;
            _pauseConsumed = false;
            _confirmPressed = false;
            _cancelPressed = false;
            _prevDashButton = false;
            _prevShieldButton = false;
            _prevStartButton = false;
        }

        // Call this inside your existing Update() after reading GamepadState
        private unsafe void UpdateGamepadActions()
        {
            if (!GamepadConnected) return;

            bool confirmDown = _gamepad.Buttons[0] == 1; // A
            bool cancelDown = _gamepad.Buttons[1] == 1; // B

            _confirmPressed = confirmDown && !_confirmWasPressed;
            _cancelPressed = cancelDown && !_cancelWasPressed;

            _confirmWasPressed = confirmDown;
            _cancelWasPressed = cancelDown;
        }

        private bool _confirmPressed = false;
        private bool _cancelPressed = false;

        public unsafe System.Numerics.Vector2 GetLeftStick()
        {
            if (!GamepadConnected)
                return System.Numerics.Vector2.Zero;

            GamepadState state = _gamepad; // local copy — fixed buffers require local/field access
            float x = state.Axes[0];
            float y = state.Axes[1];

            const float deadZone = 0.2f;
            if (MathF.Abs(x) < deadZone) x = 0f;
            if (MathF.Abs(y) < deadZone) y = 0f;

            return new System.Numerics.Vector2(x, y);
        }

        public bool ConsumeConfirmPressed()
        {
            if (!_confirmPressed) return false;
            _confirmPressed = false;
            return true;
        }

        public bool ConsumeCancelPressed()
        {
            if (!_cancelPressed) return false;
            _cancelPressed = false;
            return true;
        }
    }
}
