using ImGuiNET;
using OpenTK.Windowing.Desktop;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.UI;
using static VoidZero.UI.MenuUI;
using VoidZero.Core;

namespace VoidZero.States
{
    public class PauseState : GameState
    {
        private readonly GameStateManager _gsm;
        private readonly GameWindow _window;
        private readonly InputManager _input;
        private readonly GameManager _gameManager;
        private PlayState _pausedState; // The PlayState we’re pausing
        private MenuPage _currentPage = MenuPage.Main;

        public PauseState(GameStateManager gsm, GameWindow window, InputManager input, PlayState pausedState, GameManager gm)
        {
            _gsm = gsm;
            _window = window;
            _input = input;
            _pausedState = pausedState; // store the paused game
            _gameManager = gm;
        }

        public override void Update(float dt)
        {
            // Press ESC again to resume
            if (_input.PausePressed)
            {
                _gsm.ChangeState(_pausedState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _pausedState.Draw(spriteBatch);
        }

        public override void DrawUI()
        {
            MenuUI.DrawPauseMenu(_gsm, _window, _input, ref _currentPage, _pausedState, _gameManager);
        }
    }
}
