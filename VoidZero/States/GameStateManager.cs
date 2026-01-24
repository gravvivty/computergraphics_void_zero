using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Graphics;

namespace VoidZero.States
{
    public class GameStateManager
    {
        public GameState _current {  get; private set; }

        public void ChangeState(GameState newState)
        {
            _current?.Exit();
            _current = newState;
            _current.Enter();
        }

        public void Update(float dt)
        {
            _current?.Update(dt);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _current?.Draw(spriteBatch);
        }

        public void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            _current?.DrawUI(spriteBatch, dt);
        }
    }
}
