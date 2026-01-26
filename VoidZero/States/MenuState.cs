using ImGuiNET;
using OpenTK.Windowing.Desktop;
using System;
using VoidZero.Core;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.UI;

namespace VoidZero.States
{
    public class MenuState : GameState
    {
        private readonly GameStateManager _gameStateManager;
        private enum MenuPage { Main, Options, Credits }
        private MenuUI.MenuPage _currentPage = MenuUI.MenuPage.Main;
        private readonly GameWindow _window;
        private readonly InputManager _input;
        private readonly Background _background;
        private readonly GameManager _gameManager;


        public MenuState(GameStateManager gsm, GameWindow window, InputManager input, Background background, GameManager gm)
        {
            _gameStateManager = gsm;
            _window = window;
            _input = input;
            _background = background;
            _gameManager = gm;
            _gameManager.EnterMenu();
        }

        public override void Update(float dt) { }
        public override void Draw(SpriteBatch spriteBatch) { }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            MenuUI.DrawMenu(_gameStateManager, _window, _input, ref _currentPage, _gameManager._background, _gameManager);
        }
    }
}
