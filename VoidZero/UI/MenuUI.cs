using ImGuiNET;
using OpenTK.Windowing.Desktop;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.States;
using System;
using VoidZero.Core;
using System.Numerics;

namespace VoidZero.UI
{
    public static class MenuUI
    {
        public enum MenuPage { Main, StageSelect, Options, Credits }

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
                    DrawOptionsMenu(ref currentPage, gsm, window, input);
                    break;
                case MenuPage.Credits:
                    DrawCreditsMenu(ref currentPage, gsm, window, input);
                    break;
            }
        }

        public static void DrawMainMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm)
        {
            var io = ImGui.GetIO();
            var windowSize = io.DisplaySize;

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSize(windowSize);

            ImGui.Begin("Main Menu",
                ImGuiWindowFlags.NoDecoration |   // no title bar, resize, move
                ImGuiWindowFlags.NoMove |         // cannot drag
                ImGuiWindowFlags.NoResize |       // cannot resize
                ImGuiWindowFlags.NoSavedSettings |// don't save position/size
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoBackground
            );

            float buttonWidth = 200f;
            float buttonHeight = 50f;
            float spacing = 15f;

            string[] labels = { "Play", "Options", "Credits", "Exit" };
            float totalHeight = labels.Length * buttonHeight + (labels.Length - 1) * spacing + 80; // 80 for title space

            float windowHeight = ImGui.GetWindowHeight();
            ImGui.SetCursorPosY((windowHeight - totalHeight) / 2f);

            Vector2 available = ImGui.GetContentRegionAvail();

            // Draw title
            string title = "void_zero";
            float textWidth = ImGui.CalcTextSize(title).X;
            ImGui.SetCursorPosX((available.X - textWidth) / 2f);
            ImGui.Text(title);
            ImGui.Dummy(new Vector2(0, spacing * 2));

            foreach (string label in labels)
            {
                float centerX = (available.X - buttonWidth) / 2f;
                ImGui.SetCursorPosX(centerX);

                if (ImGui.Button(label, new Vector2(buttonWidth, buttonHeight)))
                {
                    switch (label)
                    {
                        case "Play":
                            currentPage = MenuPage.StageSelect;
                            break;
                        case "Options":
                            currentPage = MenuPage.Options;
                            break;
                        case "Credits":
                            currentPage = MenuPage.Credits;
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

        public static void DrawOptionsMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input)
        {
            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);

            ImGui.Begin("Options",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            float buttonWidth = 250f;
            float buttonHeight = 50f;
            float spacing = 15f;

            float totalHeight = 3 * buttonHeight + 2 * spacing + 60;
            BeginCenteredBlock(totalHeight);

            GameSettings settings = GameServices.Instance.Settings;

            CenterNextItem(buttonWidth);
            bool fullscreen = settings.Fullscreen;
            if (ImGui.Checkbox("Fullscreen", ref fullscreen))
            {
                settings.Fullscreen = fullscreen;
                ApplyGraphicsSettings(window);
            }

            ImGui.Dummy(new Vector2(0, spacing));

            string[] resolutions = { "800x600", "1280x720", "1600x900", "1920x1080", "2560x1440" };
            int currentIndex = Array.FindIndex(resolutions, r => r == $"{settings.Width}x{settings.Height}");
            if (currentIndex < 0) currentIndex = 0;

            CenterNextItem(buttonWidth);
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
            CenterNextItem(buttonWidth);
            ImGui.Text("Audio Settings");
            // Master Volume
            CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float master = settings.MasterVolume * 100f; // convert 0-1 to 0-100
            if (ImGui.SliderFloat("Master", ref master, 0f, 100f, "%.1f%%"))
            {
                settings.MasterVolume = master / 100f; // store back as 0-1
            }
            // SFX Volume
            CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float sfx = settings.SfxVolume * 100f;
            if (ImGui.SliderFloat("SFX", ref sfx, 0f, 100f, "%.1f%%"))
            {
                settings.SfxVolume = sfx / 100f;
            }
            // Music Volume
            CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float music = settings.MusicVolume * 100f;
            if (ImGui.SliderFloat("Music", ref music, 0f, 100f, "%.1f%%"))
            {
                settings.MusicVolume = music / 100f;
            }
            // UI Volume
            CenterNextItem(buttonWidth);
            ImGui.SetNextItemWidth(buttonWidth);
            float ui = settings.UiVolume * 100f;
            if (ImGui.SliderFloat("UI", ref ui, 0f, 100f, "%.1f%%"))
            {
                settings.UiVolume = ui / 100f;
            }

            ImGui.Dummy(new Vector2(0, spacing));

            CenterNextItem(buttonWidth);
            if (ImGui.Button("Back", new Vector2(buttonWidth, buttonHeight)))
            {
                currentPage = MenuPage.Main;
            }

            ImGui.End();
        }

        public static void DrawCreditsMenu(ref MenuPage currentPage, GameStateManager gsm, GameWindow window, InputManager input)
        {
            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);

            ImGui.Begin("Credits",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            string[] lines =
            {
                "Game Design by Steven",
                "Programming by Steven",
                "Art by Steven",
                "Music by Steven"
            };

            float spacing = 10f;
            float totalHeight = lines.Length * ImGui.GetTextLineHeight() + spacing * 4 + 60;
            BeginCenteredBlock(totalHeight);

            foreach (string line in lines)
            {
                float textWidth = ImGui.CalcTextSize(line).X;
                CenterNextItem(textWidth);
                ImGui.Text(line);
                ImGui.Dummy(new Vector2(0, spacing));
            }

            CenterNextItem(200f);
            if (ImGui.Button("Back", new Vector2(200, 50)))
            {
                currentPage = MenuPage.Main;
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
                DrawOptionsMenu(ref currentPage, gsm, window, input);
                return;
            }

            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);

            ImGui.Begin("Pause Menu",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            float buttonWidth = 200f;
            float buttonHeight = 50f;
            float spacing = 15f;

            float totalHeight = 3 * buttonHeight + 2 * spacing;
            BeginCenteredBlock(totalHeight);

            CenterNextItem(buttonWidth);
            if (ImGui.Button("Resume", new Vector2(buttonWidth, buttonHeight)))
            {
                gm.ExitPause();
                gsm.ChangeState(pausedState);
            }

            ImGui.Dummy(new Vector2(0, spacing));

            CenterNextItem(buttonWidth);
            if (ImGui.Button("Options", new Vector2(buttonWidth, buttonHeight)))
            {
                currentPage = MenuPage.Options;
            }

            ImGui.Dummy(new Vector2(0, spacing));

            CenterNextItem(buttonWidth);
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
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);

            ImGui.Begin("Stage Select",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            float buttonWidth = 180f;
            float buttonHeight = 60f;
            float spacing = 20f;

            int stageCount = 3;
            float totalWidth =
                stageCount * buttonWidth +
                (stageCount - 1) * spacing;

            float startX = (ImGui.GetContentRegionAvail().X - totalWidth) / 2f;
            float centerY = ImGui.GetWindowHeight() * 0.5f;

            ImGui.SetCursorPosY(centerY - buttonHeight * 0.5f);
            ImGui.SetCursorPosX(startX);

            for (int stage = 1; stage <= stageCount; stage++)
            {
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
            ImGui.Dummy(new Vector2(0, 80f));
            CenterNextItem(200f);
            if (ImGui.Button("Back", new Vector2(200, 50)))
            {
                currentPage = MenuPage.Main;
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

        private static void BeginCenteredBlock(float totalHeight)
        {
            float windowHeight = ImGui.GetWindowHeight();
            ImGui.SetCursorPosY((windowHeight - totalHeight) / 2f);
        }
        private static void CenterNextItem(float itemWidth)
        {
            float available = ImGui.GetContentRegionAvail().X;
            ImGui.SetCursorPosX((available - itemWidth) / 2f);
        }
    }
}
