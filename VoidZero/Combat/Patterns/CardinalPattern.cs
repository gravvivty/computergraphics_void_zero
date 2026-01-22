using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Graphics;
using OpenTK.Mathematics;
using System;

public class CardinalPattern : IBulletPattern
{
    private readonly Texture2D _tex;
    private readonly float _bulletSpeed;
    private float _rotation;

    public CardinalPattern(Texture2D tex, float bulletSpeed = 1500f)
    {
        _tex = tex;
        _bulletSpeed = bulletSpeed;
        _rotation = 0f;
    }

    public void SetRotation(float rotation)
    {
        _rotation = rotation;
    }

    public void Shoot(Entity shooter, BulletManager bullets, BulletOwner owner, float damage, BulletEnergy energy)
    {
        Vector2[] dirs =
        {
            Vector2.UnitX,
            -Vector2.UnitX,
            Vector2.UnitY,
            -Vector2.UnitY
        };

        Vector2 spawnPos = new(
            shooter.Position.X + shooter.Width / 4f,
            shooter.Position.Y + (owner == BulletOwner.Player ? 0 : shooter.Height/2f)
        );

        for (int i = 0; i < dirs.Length; i++)
        {
            dirs[i] = Rotate(dirs[i], _rotation);
        }

        foreach (var dir in dirs)
        {
            bullets.Add(new Bullet(
                _tex,
                spawnPos,
                dir,
                _bulletSpeed,
                damage,
                energy,
                owner
            ));
        }
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos
        ).Normalized();
    }
}
