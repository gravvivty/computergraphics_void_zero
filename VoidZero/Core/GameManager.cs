using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States;

namespace VoidZero.Core
{
    // Core manager when starting the game
    public class GameManager
    {
        public enum GameMode
        {
            Menu,
            Playing,
            Paused,
            Dying
        }
        public GameMode CurrentMode { get; private set; } = GameMode.Menu;

        private readonly GameWindow _window;
        private SpriteBatch _spriteBatch;
        private GameStateManager _stateManager;
        private ImGuiController _imGui;
        public Background _background { get; private set; }
        public InputManager _input { get; private set; }
        private Camera _camera;
        private ImFontPtr _defaultFont;

        private float _deathTimer = 0f;
        private const float DeathSlowDuration = 1f;
        private const float DeathTargetScale = 0.3f;

        private float _fpsTimer = 0f;
        private int _frameCount = 0;
        private float _currentFPS = 0f;

        public Vector2 ViewportOffset { get; private set; }
        public Vector2 ViewportSize { get; private set; }
        public RectangleF LetterboxRect { get; private set; }
        public (float x, float y, float w, float h) GetViewportRect() =>
            ((float)ViewportOffset.X, (float)ViewportOffset.Y,
            (float)ViewportSize.X, (float)ViewportSize.Y);

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
                GameServices.Instance.Settings.WorldWidth,
                GameServices.Instance.Settings.WorldHeight,
                GameServices.Instance.Content.LoadTexture("space", "Content/Background/background_generated_dark.png"),
                GameServices.Instance.Content.LoadTexture("stars", "Content/Background/star.png"),
                GameServices.Instance.Content.LoadTexture("planets", "Content/Background/planet.png"),
                GameServices.Instance.Content.LoadTexture("galaxies", "Content/Background/galaxy.png")
            );

            // Enemies, Player, Bullets
            GameServices.Instance.Content.LoadTexture("player", "Content/Player/player.png");
            GameServices.Instance.Content.LoadTexture("VanillaBullet", "Content/Bullets/VanillaBullet.png");
            GameServices.Instance.Content.LoadTexture("witch", "Content/Enemies/Witch.png");
            GameServices.Instance.Content.LoadTexture("shield", "Content/Shield/shield.png");
            GameServices.Instance.Content.LoadTexture("death", "Content/Effects/death.png");

            // UI
            GameServices.Instance.Content.LoadTexture("healthbar", "Content/UI/health/healthbar.png");
            GameServices.Instance.Content.LoadTexture("life1", "Content/UI/health/life1.png");
            GameServices.Instance.Content.LoadTexture("life2", "Content/UI/health/life2.png");
            GameServices.Instance.Content.LoadTexture("life3", "Content/UI/health/life3.png");
            GameServices.Instance.Content.LoadTexture("graze", "Content/UI/graze/graze.png");
            GameServices.Instance.Content.LoadTexture("graze_fill", "Content/UI/graze/graze_fill.png");
            GameServices.Instance.Content.LoadTexture("ability", "Content/UI/ability/ability.png");
            GameServices.Instance.Content.LoadTexture("ability_fill", "Content/UI/ability/ability_fill.png");

            _input = new InputManager();
            _spriteBatch = new SpriteBatch();
            _stateManager = new GameStateManager();
            _camera = new Camera();
            _imGui = new ImGuiController(_window.FramebufferSize.X, _window.FramebufferSize.Y);
            LoadImGuiFont("Content/Fonts/lowrespixel.otf", 20f);

            // Start with MenuState
            _stateManager.ChangeState(new MenuState(_stateManager, _window, _input, _background, this));

            // Global ImGui Style
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
            _input.Update(_window);
            _imGui.Update(_window, dt, _input.GamepadConnected, _input.GamepadState);

            float timeScale = 1f;

            if (CurrentMode == GameMode.Paused)
            {
                timeScale = 0f;
            }
            else if (CurrentMode == GameMode.Dying)
            {
                _deathTimer += dt;

                // Slow down logic during death
                float t = Math.Clamp(_deathTimer / DeathSlowDuration, 0f, 1f);
                t = t * t * (3f - 2f * t);

                timeScale = MathHelper.Lerp(1f, 0.3f, t);
            }

            _camera.Update(dt);
            _background.Update(dt * timeScale);

            _stateManager.Update(dt * timeScale);
            GameServices.Instance.DamageNumbers.Update(dt);

            _fpsTimer += dt;
            _frameCount++;

            if (_fpsTimer >= 1f)
            {
                _currentFPS = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;
            }
        }

        public void Draw(float dt)
        {

            ImGui.PushFont(_defaultFont);
            float targetGray = calculateGrayScale();

            _spriteBatch.Grayscale = MathHelper.Lerp(
                _spriteBatch.Grayscale,
                targetGray,
                dt * 6f
            );

            // Draw world objects
            _spriteBatch.Begin(_camera.GetTransform(), targetGray);
            _background.Draw(_spriteBatch);
            _stateManager.Draw(_spriteBatch); // only draws state-specific sprites
            GameServices.Instance.ParticleSystem.Draw(_spriteBatch);
            _spriteBatch.End();

            // Draw UI
            _spriteBatch.Begin(Matrix4.CreateOrthographicOffCenter(
                0,
                GameServices.Instance.Settings.WorldWidth,
                GameServices.Instance.Settings.WorldHeight,
                0,
                -1,
                1));
            _stateManager.DrawUI(_spriteBatch, dt);
            GameServices.Instance.DamageNumbers.Draw(WorldToScreen);
            _spriteBatch.End();

            DrawFPS();
        }
        public void DrawMenu()
        {
            ImGui.PopFont();
            _imGui.Render();
        }

        public void ResetTime()
        {
            _deathTimer = 0f;
        }

        public void OnResize(int width, int height, int fbWidth, int fbHeight)
        {
            GameServices.Instance.Settings.Width = width;
            GameServices.Instance.Settings.Height = height;
            _imGui.WindowResized(fbWidth, fbHeight);
        }

        public void EnterPlay()
        {
            ResetTime();
            CurrentMode = GameMode.Playing;
        }

        public void EnterPause()
        {
            CurrentMode = GameMode.Paused;
        }

        public void ExitPause()
        {
            CurrentMode = GameMode.Playing;
        }

        public void EnterDeath()
        {
            _deathTimer = 0f;
            CurrentMode = GameMode.Dying;
        }

        public void EnterMenu()
        {
            _deathTimer = 0f;
            CurrentMode = GameMode.Menu;
        }

        private void LoadImGuiFont(string fontPath, float fontSize)
        {
            _imGui.LoadFont(fontPath, fontSize, out _defaultFont);
        }

        public void Shake(float duration, float strength)
        {
            _camera.Shake(duration, strength);
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
                    const float holdDuration = totalRegen - fadeDuration;

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
                        fadeT = fadeT * fadeT * (3f - 2f * fadeT);
                        targetGray = MathHelper.Lerp(1f, 0f, fadeT);
                    }
                }
            }

            return targetGray;
        }

        private void DrawFPS()
        {
            var io = ImGui.GetIO();
            float padding = 10f;
            float windowWidth = 80f;
            float windowHeight = 20f;

            ImGui.SetNextWindowPos(
                new System.Numerics.Vector2(
                    io.DisplaySize.X - windowWidth - padding,
                    io.DisplaySize.Y - windowHeight - padding),
                ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(windowWidth, windowHeight));

            ImGui.Begin("FPS",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            ImGui.Text($"FPS: {MathF.Round(_currentFPS)}");
            ImGui.End();
        }

        public void ApplyViewport(int width, int height)
        {
            float worldWidth = GameServices.Instance.Settings.WorldWidth;
            float worldHeight = GameServices.Instance.Settings.WorldHeight;

            float scale = MathF.Min(width / worldWidth, height / worldHeight);

            float renderWidth = worldWidth * scale;
            float renderHeight = worldHeight * scale;

            Vector2 offset = new Vector2(
                (width - renderWidth) * 0.5f,
                (height - renderHeight) * 0.5f
            );

            ViewportOffset = offset;
            ViewportSize = new Vector2(renderWidth, renderHeight);


            GL.Viewport(
                (int)offset.X,
                (int)offset.Y,
                (int)renderWidth,
                (int)renderHeight
            );
        }

        public System.Numerics.Vector2 WorldToScreen(Vector2 worldPos)
        {
            float worldWidth = GameServices.Instance.Settings.WorldWidth;
            float worldHeight = GameServices.Instance.Settings.WorldHeight;

            float nx = worldPos.X / worldWidth;
            float ny = worldPos.Y / worldHeight;

            return new System.Numerics.Vector2(
                ViewportOffset.X + nx * ViewportSize.X,
                ViewportOffset.Y + ny * ViewportSize.Y
            );
        }
    }
}
