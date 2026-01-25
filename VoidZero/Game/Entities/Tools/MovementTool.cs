using OpenTK.Mathematics;
using VoidZero.Game.Entities;

public class MovementTool
{
    private Vector2 _direction;
    private float _speed; // units per second
    private float _duration; // seconds
    private float _timer;

    public bool Finished => _timer >= _duration;

    public MovementTool(Vector2 direction, float speed, float duration)
    {
        _direction = direction.Normalized();
        _speed = speed;
        _duration = duration;
        _timer = 0f;
    }

    public Vector2 Update(Entity entity, float dt)
    {
        if (Finished)
        {
            return entity.Position;
        }

        _timer += dt;
        float moveDistance = _speed * dt;
        entity.Position += _direction * moveDistance;

        return entity.Position;
    }
}
