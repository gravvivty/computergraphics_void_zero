using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Game.Entities;
using VoidZero.Game.Input;
using OpenTK.Windowing.Desktop;
using VoidZero.Core;
using System.Drawing;

namespace VoidZero.States
{
    public class PlayState : GameState
    {
        private readonly InputManager _input;
        private readonly GameWindow _window;
        private readonly GameStateManager _gsm;
        private readonly GameManager _gm;
        public Background _background { get; }


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
            // Debug player hitbox
            //spriteBatch.DrawRectangle(_player.Hitbox, Color.Red);
        }

        public override void DrawUI()
        {
            // HUD etc
        }

        public void OnResize(int newWidth, int newHeight)
        {
            _player.OnResize(newWidth, newHeight);
            // loop through other entities if you have more
        }
    }
}