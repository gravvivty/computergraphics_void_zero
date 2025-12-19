using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Graphics;

namespace VoidZero.States
{
    public abstract class GameState
    {
        public virtual void Enter() { }
        public virtual void Exit() { }

        public abstract void Update(float dt);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void DrawUI();
    }
}
