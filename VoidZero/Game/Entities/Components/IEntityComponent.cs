namespace VoidZero.Game.Entities.Components
{
    // Interface for the custom ECS we created by ourselves
    // It is possible to attach components to Entities to give them additional functionality
    public interface IEntityComponent
    {
        void Attach(Entity entity);
        void Update(Entity entity, float dt);
    }
}
