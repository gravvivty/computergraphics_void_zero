using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Graphics;
using OpenTK.Mathematics;
using System;

public class CardinalPattern : IBulletPattern
{
    private readonly Texture2D _bulletTexture;
    private readonly float _bulletSpeed;

    public CardinalPattern(Texture2D bulletTexture, float bulletSpeed = 1500f)
    {
        _bulletTexture = bulletTexture;
        _bulletSpeed = bulletSpeed;
    }

    public void Shoot(Entity shooter, BulletManager bullets, BulletOwner owner, float damage, BulletEnergy energy)
    {
        float rotation = shooter.Rotation;
        Vector2[] directions =
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

        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = Rotate(directions[i], rotation);
        }

        foreach (Vector2 direction in directions)
        {
            bullets.Add(new Bullet(
                _bulletTexture,
                spawnPos,
                direction,
                _bulletSpeed,
                damage,
                energy,
                owner
            ));
        }
    }

    private static Vector2 Rotate(Vector2 vector, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new Vector2(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos
        ).Normalized();
    }
}
