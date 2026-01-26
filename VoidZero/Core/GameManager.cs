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
        private ScreenShake _screenShake;
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
                GameServices.Instance.Content.LoadTexture("space", "Content/Background/background.png"),
                GameServices.Instance.Content.LoadTexture("stars", "Content/Background/star.png"),
                GameServices.Instance.Content.LoadTexture("planets", "Content/Background/planet.png")
            );

            GameServices.Instance.Content.LoadTexture("player", "Content/player.png");
            GameServices.Instance.Content.LoadTexture("VanillaBullet", "Content/Bullets/VanillaBullet.png");
            GameServices.Instance.Content.LoadTexture("witch", "Content/Enemies/Witch.png");
            GameServices.Instance.Content.LoadTexture("shield", "Content/Shield/shield.png");
            GameServices.Instance.Content.LoadTexture("death", "Content/Effects/death.png");

            _input = new InputManager();
            _spriteBatch = new SpriteBatch();
            _stateManager = new GameStateManager();
            _screenShake = new ScreenShake();
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
            _screenShake.Update(dt);
            // Update current state
            _stateManager.Update(dt);
            _input.Update(_window);
            if (!_gameStarted)
            {
                _background.Update(dt);
            }
        }

        public void Draw(float dt)
        {
            ImGui.PushFont(_defaultFont);

            float targetGray = calculateGrayScale();

            // Smooth transition
            _spriteBatch.Grayscale = MathHelper.Lerp(
                _spriteBatch.Grayscale,
                targetGray,
                dt * 6f
            );

            // Draw game objects
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
                0 + _screenShake.Offset.X,
                _window.Size.X + _screenShake.Offset.X,
                _window.Size.Y + _screenShake.Offset.Y,
                0 + _screenShake.Offset.Y,
                -1, 1
            );

            _spriteBatch.Begin(projection, targetGray);
            _background.Draw(_spriteBatch);
            _stateManager.Draw(_spriteBatch); // only draws state-specific sprites
            _stateManager.DrawUI(_spriteBatch, dt);
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

        public void Shake(float duration, float strength)
        {
            _screenShake.Start(duration, strength);
        }

        private float calculateGrayScale()
        {
            float targetGray = 0f;

            if (_stateManager._current is PlayState playState)
            {
                var player = playState._player;

                if (player.IsCriticalHealth)
                {
                    const float totalRegen = 3f;
                    const float fadeDuration = 1f;
                    const float holdDuration = totalRegen - fadeDuration; // 2s

                    float t = player.HealthRegenTimer;

                    if (t < holdDuration)
                    {
                        // Stay fully gray
                        targetGray = 1f;
                    }
                    else
                    {
                        // Fade back to color in last 0.5s
                        float fadeT = Math.Clamp((t - holdDuration) / fadeDuration, 0f, 1f);
                        fadeT = fadeT * fadeT * (3f - 2f * fadeT); // smoothstep
                        targetGray = MathHelper.Lerp(1f, 0f, fadeT);
                    }
                }
            }

            return targetGray;
        }
    }
}
