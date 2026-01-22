using OpenTK.Mathematics;
using VoidZero.Game.Entities;

public class MovementComponent
{
    private Vector2 _direction;
    private float _speed;      // units per second
    private float _duration;   // seconds
    private float _timer;      // elapsed time

    public bool Finished => _timer >= _duration;

    public MovementComponent(Vector2 direction, float speed, float duration)
    {
        _direction = direction.Normalized();
        _speed = speed;
        _duration = duration;
        _timer = 0f;
    }

    public Vector2 Update(Entity entity, float dt)
    {
        if (Finished) return entity.Position;

        _timer += dt;
        float moveDist = _speed * dt;
        entity.Position += _direction * moveDist;

        return entity.Position;
    }
}
