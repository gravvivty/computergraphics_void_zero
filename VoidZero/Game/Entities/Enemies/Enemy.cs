using OpenTK.Mathematics;
using VoidZero.Game.Entities.Components;
using VoidZero.Graphics;

namespace VoidZero.Game.Entities.Enemies
{
    // Enemy inherits Entity but does not change desired behavior
    public abstract class Enemy : Entity
    {
        protected bool CanShoot => Components.OfType<MovementLifecycleComponent>().FirstOrDefault()?.IsActive ?? true;
        protected Enemy(Texture2D texture, Vector2 position, float width, float height)
            : base(texture, position, width, height)
        {

        }
    }
}
