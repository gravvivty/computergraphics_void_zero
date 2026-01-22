using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace VoidZero.Game.Input
{
    public class InputManager
    {
        private KeyboardState _keyboard;
        public bool PausePressed => _keyboard.IsKeyPressed(Keys.Escape);

        public void Update(GameWindow window)
        {
            _keyboard = window.KeyboardState;
        }

        public bool MoveUp => _keyboard.IsKeyDown(Keys.W);
        public bool MoveDown => _keyboard.IsKeyDown(Keys.S);
        public bool MoveLeft => _keyboard.IsKeyDown(Keys.A);
        public bool MoveRight => _keyboard.IsKeyDown(Keys.D);

        public bool Shoot => _keyboard.IsKeyDown(Keys.J);
        public bool SwitchShield => _keyboard.IsKeyDown(Keys.K);
        public bool Dash => _keyboard.IsKeyDown(Keys.Space);

        // Debug pattern swap
        public bool SwitchPatternPressed => _keyboard.IsKeyPressed(Keys.L);

    }
}