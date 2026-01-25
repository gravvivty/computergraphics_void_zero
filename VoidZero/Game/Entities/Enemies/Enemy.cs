using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Tools;

namespace VoidZero.Game.Entities.Enemies
{
    // Enemy inherits Entity but doesn't change behavior
    public abstract class Enemy : Entity
    {
        protected Enemy(Texture2D texture, Vector2 position, float width, float height)
            : base(texture, position, width, height)
        {
        }
    }
}
