using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Game.Entities;
using VoidZero.Game.Input;
using OpenTK.Windowing.Desktop;
using VoidZero.Core;
using System.Drawing;
using System.Collections.Generic;

namespace VoidZero.States
{
    public class PlayState : GameState, IResizableState
    {
        private readonly InputManager _input;
        private readonly GameWindow _window;
        private readonly GameStateManager _gsm;
        private readonly GameManager _gm;
        public Background _background { get; }
        public List<Entity> Entities { get; } = new();



        private Player _player;

        public PlayState(GameStateManager gsm, GameWindow window, InputManager input, Background bg, GameManager gm)
        {
            _gsm = gsm;
            _window = window;
            _input = input;
            _background = bg;
            _gm = gm;

            var tex = GameServices.Instance.Content.GetTexture("player");
            _player = new Player(tex, new Vector2(500, 500), _input);
            _player.SetPositionRelative(_player.Position,window.Size.X, window.Size.Y);

            Entities.Add(_player);

        }

        public override void Update(float dt)
        {
            _background.Update(dt);
            _player.Update(dt);

            // Pause logic
            if (_input.PausePressed)
            {
                _gsm.ChangeState(new PauseState(_gsm, _window, _input, this, _gm));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _player.Draw(spriteBatch);
        }

        public override void DrawUI()
        {
            // HUD etc
        }

        public void OnResize(int newWidth, int newHeight)
        {
            foreach (var entity in Entities)
                entity.OnResize(newWidth, newHeight);
        }
    }
}