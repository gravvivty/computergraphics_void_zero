using ImGuiNET;
using OpenTK.Windowing.Desktop;
using System.Numerics;
using VoidZero.Core;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.States;
using VoidZero.States.GameStates;
using VoidZero.Utils;
using VoidZero.States.Graph;

namespace VoidZero.UI
{
    // Menu code
    public static class MenuUI
    {
        public enum MenuPage { Main, StageSelect, Options, Credits, Controls }
        private static bool _focusNextFrame = true;
        private static float _rainbowTimer = 0f;
        public static void TickRainbow(float dt) => _rainbowTimer += dt;

        public static void DrawMenu(GameStateManager gsm, GameWindow window, InputManager input, ref MenuPage currentPage, Background bg, GameManager gm)
        {
            switch (currentPage)
            {
                case MenuPage.Main:
                    DrawMainMenu(ref currentPage, gsm, window, input, bg, gm);
                    break;
                case MenuPage.StageSelect:
                    DrawStageSelectMenu(ref currentPage, gsm, window, input, bg, gm);
                    break;
                case MenuPage.Options:
                    DrawOptionsMenu(ref currentPage, gsm, window, input, gm);
                    break;
                case MenuPage.Credits:
                    DrawCreditsMenu(ref currentPage, gsm, window, input, gm);
                    break;
                case MenuPage.Controls:
                    DrawControlsMenu(ref currentPage, gsm, window, input, gm);
                    break;
            }
        }

        public static void DrawMainMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Main Menu",
                ImGuiWindowFlags.NoDecoration |   // No title bar, resize, move
                ImGuiWindowFlags.NoMove |         // Cannot drag
                ImGuiWindowFlags.NoResize |       // Cannot resize
                ImGuiWindowFlags.NoSavedSettings |// Do not save position/size
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            float buttonWidth = 200f * scale;
            float buttonHeight = 50f * scale;
            float spacing = 15f * scale;

            string[] labels = { "Play", "Options", "Credits", "Controls", "Exit" };
            float totalHeight = labels.Length * buttonHeight + (labels.Length - 1) * spacing + 80 * scale; // 80 for title space

            float windowHeight = ImGui.GetWindowHeight();
            ImGui.SetCursorPosY((windowHeight - totalHeight) / 2f);

            float available = ImGui.GetWindowWidth();

            // Draw title
            string title = "void_zero";
            float textWidth = ImGui.CalcTextSize(title).X;
            ImGui.SetCursorPosX((available - textWidth) / 2f);
            ImGui.Text(title);
            ImGui.Dummy(new Vector2(0, spacing * 2));

            foreach (string label in labels)
            {
                float centerX = (available - buttonWidth) / 2f;
                ImGui.SetCursorPosX(centerX);

                if (ImGui.Button(label, new Vector2(buttonWidth, buttonHeight)))
                {
                    switch (label)
                    {
                        case "Play":
                            var graph = WorldGraphBuilder.Generate();
                            gsm.ChangeState(
                                new MapState(
                                    gsm,
                                    window,
                                    input,
                                    bg,
                                    gm,
                                    graph,
                                    justCompleted: null
                                )
                            );
                            break;
                        case "Options":
                            currentPage = MenuPage.Options;
                            _focusNextFrame = true;
                            break;
                        case "Credits":
                            currentPage = MenuPage.Credits;
                            _focusNextFrame = true;
                            break;
                        case "Controls":
                            currentPage = MenuPage.Controls;
                            _focusNextFrame = true;
                            break;
                        case "Exit":
                            Environment.Exit(0);
                            break;
                    }
                }
                ImGui.Dummy(new Vector2(0, spacing));
            }

            ImGui.End();
        }

        public static void DrawOptionsMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input, GameManager gm)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Options",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            float buttonWidth = 250f * scale;
            float buttonHeight = 50f * scale;
            float spacing = 15f * scale;

            float totalHeight = 3 * buttonHeight + 2 * spacing + 60 * scale;
            ImGuiHelpers.BeginCenteredBlock(totalHeight);

            GameSettings settings = GameServices.Instance.Settings;

            ImGuiHelpers.CenterNextItem(buttonWidth);
            bool fullscreen = settings.Fullscreen;
            if (ImGui.Checkbox("Fullscreen", ref fullscreen))
            {
                settings.Fullscreen = fullscreen;
                ApplyGraphicsSettings(window);
            }

            ImGui.Dummy(new Vector2(0, spacing));

            string[] resolutions = { "1280x720", "1600x900", "1920x1080", "2560x1440" };
            int currentIndex = Array.FindIndex(resolutions, r => r == $"{settings.Width}x{settings.Height}");
            if (currentIndex < 0) currentIndex = 0;

            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            if (ImGui.Combo("Resolution", ref currentIndex, resolutions, resolutions.Length))
            {
                string[] parts = resolutions[currentIndex].Split('x');
                settings.Width = int.Parse(parts[0]);
                settings.Height = int.Parse(parts[1]);
                ApplyGraphicsSettings(window);
            }

            ImGui.Dummy(new Vector2(0, spacing));

            // Audio sliders
            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.Text("Audio Settings");
            // Master Volume
            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float master = settings.MasterVolume * 100f; // Convert 0-1 to 0-100
            if (ImGui.SliderFloat("Master", ref master, 0f, 100f, "%.1f%%"))
            {
                settings.MasterVolume = master / 100f; // Store back as 0-1
            }
            // SFX Volume
            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float sfx = settings.SfxVolume * 100f;
            if (ImGui.SliderFloat("SFX", ref sfx, 0f, 100f, "%.1f%%"))
            {
                settings.SfxVolume = sfx / 100f;
            }
            // Music Volume
            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float music = settings.MusicVolume * 100f;
            if (ImGui.SliderFloat("Music", ref music, 0f, 100f, "%.1f%%"))
            {
                settings.MusicVolume = music / 100f;
            }
            // UI Volume
            ImGuiHelpers.CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float ui = settings.UiVolume * 100f;
            if (ImGui.SliderFloat("UI", ref ui, 0f, 100f, "%.1f%%"))
            {
                settings.UiVolume = ui / 100f;
            }

            ImGui.Dummy(new Vector2(0, spacing));

            ImGuiHelpers.CenterNextItem(buttonWidth);
            if (ImGui.Button("Back", new Vector2(buttonWidth, buttonHeight)))
            {
                currentPage = MenuPage.Main;
                _focusNextFrame = true;
            }

            ImGui.End();
        }

        public static void DrawCreditsMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input, GameManager gm)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Credits",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            string[] lines =
            {
                "Game Design by Steven Gayer & Finley Hogan",
                "Programming by Steven Gayer & Finley Hogan",
                "UI/Shield/Player Art by Steven Gayer",
                "Font/Background/Enemy/Bullet Ressources by Content/Credits.txt",
                "Music will be by Steven Gayer (no sound yet)"
            };

            float spacing = 10f * scale;
            float totalHeight = lines.Length * ImGui.GetTextLineHeight() + spacing * 4 + 60 * scale;
            ImGuiHelpers.BeginCenteredBlock(totalHeight);

            foreach (string line in lines)
            {
                float textWidth = ImGui.CalcTextSize(line).X;
                ImGuiHelpers.CenterNextItem(textWidth);
                ImGui.Text(line);
                ImGui.Dummy(new Vector2(0, spacing));
            }

            ImGuiHelpers.CenterNextItem(200f);
            if (ImGui.Button("Back", new Vector2(200, 50)))
            {
                currentPage = MenuPage.Main;
                _focusNextFrame = true;
            }

            ImGui.End();
        }

        public static void DrawControlsMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input, GameManager gm)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Controls",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            string[] linesKeyboard =
            {
                "Keyboard:",
                "WASD - Movement",
                "J - Shoot",
                "K - Switch shield",
                "L - Activate ability",
                "ESC - Pause/Resume",
                "F3 - Debug Graze Registration Boxes",
                "F4 - Debug Hitboxes"
            };

            string[] linesController =
            {
                "Controller (XBOX/PS):",
                "Left Joystick - Movement",
                "RT/R2 - Shoot",
                "X/Rectangle - Switch shield",
                "Y/Triangle - Activate ability",
                "Start - Pause/Resume",
            };

            float lineH = ImGui.GetTextLineHeight();
            float spacing = 10f * scale;
            float colW = 400f * scale;
            float gapW = 60f * scale;
            float totalW = colW * 2 + gapW;
            float totalH = linesKeyboard.Length * (lineH + spacing) + 120f;

            // Center the whole block
            float startX = (vpW - totalW) / 2;
            float startY = (vpH - totalH) / 2;

            // Left column: Keyboard
            ImGui.SetCursorPos(new Vector2(startX, startY));
            // Child window so lines stack naturally left-aligned
            ImGui.BeginChild("col_keyboard", new Vector2(colW, totalH - 60f));
            foreach (string line in linesKeyboard)
            {
                ImGui.Text(line);
                ImGui.Dummy(new Vector2(0, spacing));
            }
            ImGui.EndChild();

            // Right column: Controller
            ImGui.SetCursorPos(new Vector2(startX + colW + gapW, startY));
            ImGui.BeginChild("col_controller", new Vector2(colW, totalH - 60f));
            foreach (string line in linesController)
            {
                ImGui.Text(line);
                ImGui.Dummy(new Vector2(0, spacing));
            }
            ImGui.EndChild();

            ImGui.SetCursorPos(new Vector2((vpW - 200f) / 2f, startY + totalH - 50f));
            if (ImGui.Button("Back", new Vector2(200, 50)))
            {
                currentPage = MenuPage.Main;
                _focusNextFrame = true;
            }

            ImGui.End();
        }

        public static void DrawPauseMenu(
            GameStateManager gsm,
            GameWindow window,
            InputManager input,
            ref MenuPage currentPage,
            PlayState pausedState,
            GameManager gm)
        {
            if (currentPage == MenuPage.Options)
            {
                DrawOptionsMenu(ref currentPage, gsm, window, input, gm);
                return;
            }

            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Pause Menu",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            float buttonWidth = 200f * scale;
            float buttonHeight = 50f * scale;
            float spacing = 15f * scale;

            float totalHeight = 3 * buttonHeight + 2 * spacing;
            ImGuiHelpers.BeginCenteredBlock(totalHeight);

            ImGuiHelpers.CenterNextItem(buttonWidth);
            if (ImGui.Button("Resume", new Vector2(buttonWidth, buttonHeight)))
            {
                gm.ExitPause();
                gsm.ChangeState(pausedState);
            }

            ImGui.Dummy(new Vector2(0, spacing));

            ImGuiHelpers.CenterNextItem(buttonWidth);
            if (ImGui.Button("Options", new Vector2(buttonWidth, buttonHeight)))
            {
                currentPage = MenuPage.Options;
            }

            ImGui.Dummy(new Vector2(0, spacing));

            ImGuiHelpers.CenterNextItem(buttonWidth);
            if (ImGui.Button("Main Menu", new Vector2(buttonWidth, buttonHeight)))
            {
                gm.ExitPause();
                gsm.ChangeState(new MenuState(gsm, window, input, pausedState._background, gm));
            }

            ImGui.End();
        }

        public static void DrawStageSelectMenu(
            ref MenuPage currentPage,
            GameStateManager gsm,
            GameWindow window,
            InputManager input,
            Background bg,
            GameManager gm)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = gm.GetViewportRect();
            float scaleX = vpW / 1920;
            float scaleY = vpH / 1080;
            float scale = MathF.Min(scaleX, scaleY);

            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));

            ImGui.Begin("Stage Select",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            if (_focusNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusNextFrame = false;
            }

            float buttonWidth = 180f * scale;
            float buttonHeight = 60f * scale;
            float cardWidth = 180f * scale;
            float cardHeight = 120f * scale; // record info above the button
            float spacing = 20f * scale;

            int stageCount = 3;
            float totalWidth =
                stageCount * buttonWidth +
                (stageCount - 1) * spacing;

            float startX = (ImGui.GetWindowWidth() - totalWidth) / 2f;
            float centerY = ImGui.GetWindowHeight() * 0.5f;

            ImGui.SetCursorPosY(centerY - buttonHeight * 0.5f);
            ImGui.SetCursorPosX(startX);

            for (int stage = 1; stage <= stageCount; stage++)
            {
                float colX = startX + (stage - 1) * (cardWidth + spacing);
                StageRecord rec = StageHighScores.Instance.GetRecord(stage);
                // --- Record card ---
                ImGui.SetCursorPos(new Vector2(colX, centerY));
                ImGui.BeginChild($"record_{stage}", new Vector2(cardWidth, cardHeight));

                if (rec == null)
                {
                    ImGuiHelpers.CenterText("No record yet");
                }
                else
                {
                    ImGuiHelpers.CenterColoredText(
                        $"Score: {rec.FinalScore:N0}",
                        new OpenTK.Mathematics.Vector4(0.2f, 1f, 0.2f, 1f));

                    ImGui.Spacing();

                    ImGuiHelpers.CenterText(
                        $"Time: {TimeSpan.FromSeconds(rec.CompletionTime):mm\\:ss}");

                    ImGuiHelpers.CenterText(
                        $"Kills: {rec.EnemiesKilled}");

                    if (rec.HitsTaken == 0)
                    {
                        ImGuiHelpers.CenterRainbowText("Hits: 0", _rainbowTimer);
                    }
                    else
                    {
                        ImGuiHelpers.CenterText($"Hits: {rec.HitsTaken}");
                    }
                }

                ImGui.EndChild();

                ImGui.SetCursorPos(new Vector2(colX, centerY + cardHeight));

                if (ImGui.Button($"Stage {stage}", new Vector2(buttonWidth, buttonHeight)))
                {
                    gm.EnterPlay();
                    gm.ResetTime();
                    input.Reset();
                    gsm.ChangeState(
                        new PlayState(gsm, window, input, bg, gm, stage)
                    );
                }

                if (stage < stageCount)
                {
                    ImGui.SameLine(0f, spacing);
                }
            }

            // Back button
            ImGui.SetCursorPos(new Vector2((vpW - 200f) / 2f, centerY + cardHeight + buttonHeight + 30f));

            ImGuiHelpers.CenterNextItem(200f);
            if (ImGui.Button("Back", new Vector2(200, 50)))
            {
                currentPage = MenuPage.Main;
                _focusNextFrame = true;
            }

            ImGui.End();
        }


        private static void ApplyGraphicsSettings(GameWindow window)
        {
            GameSettings settings = GameServices.Instance.Settings;
            if (window != null)
            {
                window.WindowState = settings.Fullscreen
                    ? OpenTK.Windowing.Common.WindowState.Fullscreen
                    : OpenTK.Windowing.Common.WindowState.Normal;

                window.Size = new OpenTK.Mathematics.Vector2i(settings.Width, settings.Height);
            }
        }
    }
}
