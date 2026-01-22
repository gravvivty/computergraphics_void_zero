using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States;

namespace VoidZero.Core
{
    public class GameManager
    {
        private readonly GameWindow _window;
        private SpriteBatch _spriteBatch;
        private GameStateManager _stateManager;
        private ImGuiController _imGui;
        private Background _background;
        private InputManager _input;
        private ImFontPtr _defaultFont;
        private bool _gameStarted = false;

        public GameManager(GameWindow window)
        {
            _window = window;
        }

        public void Initialize()
        {
            GameServices.Instance.Settings.Width = _window.Size.X;
            GameServices.Instance.Settings.Height = _window.Size.Y;

            // Load background layers
            _background = new Background(
                _window.Size.X,
                _window.Size.Y,
                GameServices.Instance.Content.LoadTexture("space", "Content/Background/space.png"),
                GameServices.Instance.Content.LoadTexture("stars", "Content/Background/star.png"),
                GameServices.Instance.Content.LoadTexture("planets", "Content/Background/planet.png")
            );

            GameServices.Instance.Content.LoadTexture("player", "Content/player.png");
            GameServices.Instance.Content.LoadTexture("VanillaBullet", "Content/Bullets/VanillaBullet.png");

            _input = new InputManager();
            _spriteBatch = new SpriteBatch();
            _stateManager = new GameStateManager();
            _imGui = new ImGuiController(_window.Size.X, _window.Size.Y);
            LoadImGuiFont("Content/Fonts/lowrespixel.otf", 20f); // adjust path & size

            // Start with MenuState
            _stateManager.ChangeState(new MenuState(_stateManager, _window, _input, _background, this));

            // GLOBAL IMGUI STYLE (one-time)
            var style = ImGui.GetStyle();

            // Button shape
            style.FrameBorderSize = 1.5f;

            // Transparent buttons
            style.Colors[(int)ImGuiNET.ImGuiCol.Button] = new System.Numerics.Vector4(0f, 0f, 0f, 0f);
            style.Colors[(int)ImGuiNET.ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(1f, 1f, 1f, 0.2f);
            style.Colors[(int)ImGuiNET.ImGuiCol.ButtonActive] = new System.Numerics.Vector4(1f, 1f, 1f, 0.4f);

            // White borders
            style.Colors[(int)ImGuiNET.ImGuiCol.Border] = new System.Numerics.Vector4(1f, 1f, 1f, 1f);

            // Text color (optional)
            style.Colors[(int)ImGuiNET.ImGuiCol.Text] = new System.Numerics.Vector4(1f, 1f, 1f, 1f);
        }

        public void Update(float dt)
        {
            _imGui.Update(_window, dt);
            // Update current state
            _stateManager.Update(dt);
            _input.Update(_window);
            if (!_gameStarted)
                _background.Update(dt);
        }

        public void Draw()
        {
            ImGui.PushFont(_defaultFont);

            // 2️⃣ Draw game objects
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
                0, _window.Size.X,
                _window.Size.Y, 0,
                -1, 1);

            _spriteBatch.Begin(projection);
            _background.Draw(_spriteBatch); // draw background first
            _stateManager.Draw(_spriteBatch); // only draws state-specific sprites
            _stateManager.DrawUI();
            _spriteBatch.End();
            ImGui.PopFont();
            _imGui.Render();
        }

        public void OnResize(int w, int h)
        {
            GameServices.Instance.Settings.Width = w;
            GameServices.Instance.Settings.Height = h;
            _imGui.WindowResized(w, h);
            _background?.Resize(w, h);

            // Resize all entities in current state
            if (_stateManager._current is IResizableState resizable)
            {
                resizable.OnResize(w, h);
            }

        }

        public void StartGame()
        {
            _gameStarted = true;
        }

        public void ExitGame()
        {
            _gameStarted = false;
        }

        private void LoadImGuiFont(string fontPath, float fontSize)
        {
            _imGui.LoadFont(fontPath, fontSize, out _defaultFont);
        }
    }
}
