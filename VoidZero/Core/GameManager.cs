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
            GameServices.Instance.Content.LoadTexture("witch", "Content/Enemies/Witch.png");
            GameServices.Instance.Content.LoadTexture("shield", "Content/Shield/shield.png");

            _input = new InputManager();
            _spriteBatch = new SpriteBatch();
            _stateManager = new GameStateManager();
            _imGui = new ImGuiController(_window.Size.X, _window.Size.Y);
            LoadImGuiFont("Content/Fonts/lowrespixel.otf", 20f); // adjust path & size

            // Start with MenuState
            _stateManager.ChangeState(new MenuState(_stateManager, _window, _input, _background, this));

            // Gloval ImGui Style
            var style = ImGui.GetStyle();

            // Button shape
            style.FrameBorderSize = 1.5f;

            // Transparent buttons
            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0f, 0f, 0f, 0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(1f, 1f, 1f, 0.2f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(1f, 1f, 1f, 0.4f);

            // White borders
            style.Colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(1f, 1f, 1f, 1f);

            // Text color
            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1f, 1f, 1f, 1f);
        }

        public void Update(float dt)
        {
            _imGui.Update(_window, dt);
            // Update current state
            _stateManager.Update(dt);
            _input.Update(_window);
            if (!_gameStarted)
            {
                _background.Update(dt);
            }
        }

        public void Draw()
        {
            ImGui.PushFont(_defaultFont);

            // Draw game objects
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
                0, _window.Size.X,
                _window.Size.Y, 0,
                -1, 1);

            _spriteBatch.Begin(projection);
            _background.Draw(_spriteBatch);
            _stateManager.Draw(_spriteBatch); // only draws state-specific sprites
            _stateManager.DrawUI();
            _spriteBatch.End();
            ImGui.PopFont();
            _imGui.Render();
        }

        public void OnResize(int width, int height)
        {
            GameServices.Instance.Settings.Width = width;
            GameServices.Instance.Settings.Height = height;
            _imGui.WindowResized(width, height);
            _background?.Resize(width, height);

            // Resize all entities in current state
            if (_stateManager._current is IResizableState resizable)
            {
                resizable.OnResize(width, height);
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
