using ImGuiNET;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Core;
using VoidZero.Game.Input;
using VoidZero.Graphics;

namespace VoidZero.States
{
    public class DeathState : GameState
    {
        private readonly GameStateManager _gsm;
        private readonly GameWindow _window;
        private readonly GameManager _gm;

        public DeathState(GameStateManager gsm, GameWindow window, GameManager gm)
        {
            _gsm = gsm;
            _window = window;
            _gm = gm;
        }

        public override void Update(float dt) { }

        public override void Draw(SpriteBatch spriteBatch) { }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);

            ImGui.Begin("Death Screen",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            float spacing = 40f;
            float buttonWidth = 260f;
            float buttonHeight = 60f;

            float totalHeight = 120 + buttonHeight + spacing;
            float centerY = ImGui.GetWindowHeight() / 2f;

            ImGui.SetCursorPosY(centerY - totalHeight / 2f);

            string title = "SYSTEM FAILURE";
            var textSize = ImGui.CalcTextSize(title);
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - textSize.X) / 2f);
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.2f, 0.2f, 1f), title);

            ImGui.Dummy(new System.Numerics.Vector2(0, spacing));

            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - buttonWidth) / 2f);
            if (ImGui.Button("Return to Main Menu", new System.Numerics.Vector2(buttonWidth, buttonHeight)))
            {
                _gm.ResetTime();
                _gm.ExitPause();
                _gsm.ChangeState(
                    new MenuState(_gsm, _window, _gm._input, _gm._background, _gm)
                );
            }

            ImGui.End();
        }
    }

}
