using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Graphics;
using OpenTK.Mathematics;

public class CardinalPattern : IBulletPattern
{
    private readonly Texture2D _tex;

    public CardinalPattern(Texture2D tex)
    {
        _tex = tex;
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
            shooter.Position.Y + (owner == BulletOwner.Player ? 0 : shooter.Height)
        );


        foreach (var dir in dirs)
        {
            bullets.Add(new Bullet(
                _tex,
                spawnPos,
                dir,
                1500f,
                damage,
                energy,
                owner
            ));
        }
    }
}
