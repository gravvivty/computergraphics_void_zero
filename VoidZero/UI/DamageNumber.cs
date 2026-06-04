using OpenTK.Mathematics;

namespace VoidZero.UI
{
    public class DamageNumber
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public float Damage;

        public float Lifetime = 0.5f;
        public float Remaining = 0.5f;

        public bool Dead => Remaining <= 0f;

        public DamageNumber(Vector2 position, float damage)
        {
            Position = position;
            Damage = damage;

            Random rng = Random.Shared;

            Velocity = new Vector2(
                rng.NextSingle() * 60f - 30f,
                -120f - rng.NextSingle() * 40f
            );
        }

        public void Update(float dt)
        {
            Remaining -= dt;

            Position += Velocity * dt;

            Velocity.Y += 250f * dt;
        }

        public float Alpha => Math.Clamp(Remaining / Lifetime, 0f, 1f);
    }
}