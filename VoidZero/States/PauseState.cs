using ImGuiNET;
using OpenTK.Windowing.Desktop;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.UI;
using VoidZero.Core;
using static VoidZero.UI.MenuUI;

namespace VoidZero.States
{
    public class PauseState : GameState, IResizableState
    {
        private readonly GameStateManager _gameStateManager;
        private readonly GameWindow _window;
        private readonly InputManager _input;
        private readonly GameManager _gameManager;
        public PlayState _pausedState { get; set; } // The PlayState we’re pausing
        private MenuPage _currentPage = MenuPage.Main;

        public PauseState(GameStateManager gsm, GameWindow window, InputManager input, PlayState pausedState, GameManager gm)
        {
            _gameStateManager = gsm;
            _window = window;
            _input = input;
            _pausedState = pausedState; // store the paused game
            _gameManager = gm;
            _gameManager.EnterPause();
        }

        public override void Update(float dt)
        {
            // Press ESC again to resume
            if (_input.ConsumePausePressed())
            {
                _gameManager.ExitPause();
                _gameStateManager.ChangeState(_pausedState);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _pausedState.Draw(spriteBatch);
        }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            DrawPauseMenu(_gameStateManager, _window, _input, ref _currentPage, _pausedState, _gameManager);
        }

        public void OnResize(int newWidth, int newHeight)
        {
            _pausedState.OnResize(newWidth, newHeight);
        }
    }
}
