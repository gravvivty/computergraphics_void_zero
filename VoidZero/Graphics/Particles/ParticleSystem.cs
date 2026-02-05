using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Graphics.Particles
{
    public class ParticleSystem
    {
        private readonly List<Particle> _particles = new();
        private readonly Random _rng = new();

        public void Update(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update(dt);

                if (!_particles[i].IsAlive)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Particle particle in _particles)
            {
                particle.Draw(batch);
            }
        }

        public void SpawnSparks(
            Vector2 position,
            Vector2 impactDirection,
            int minCount = 15,
            int maxCount = 30)
        {
            int count = _rng.Next(minCount, maxCount + 1);

            // Normalize direction
            impactDirection = impactDirection.LengthSquared > 0
                ? impactDirection.Normalized()
                : -Vector2.UnitY;

            // Base direction = behind impact
            Vector2 baseDir = new Vector2(
                -impactDirection.X,
                impactDirection.Y
            );

            // Randomly choose left or right 15°
            float sideAngle = _rng.Next(0, 2) == 0 ? -15f : 15f;
            baseDir = Rotate(baseDir, MathHelper.DegreesToRadians(sideAngle));
            float coneHalfAngle = MathHelper.DegreesToRadians(20f);

            for (int i = 0; i < count; i++)
            {
                float angleOffset = MathHelper.Lerp(
                    -coneHalfAngle,
                    coneHalfAngle,
                    (float)_rng.NextDouble()
                );

                Vector2 dir = Rotate(baseDir, angleOffset);

                float speed = MathHelper.Lerp(800f, 1500f, (float)_rng.NextDouble());

                var velocity = dir * speed;

                Vector4 sparkColor = new Vector4(
                    1f,
                    MathHelper.Lerp(0.3f, 1f, (float)_rng.NextDouble()), // Green varies
                    0f,
                    1f
                );

                _particles.Add(new Particle
                {
                    Position = position,
                    Velocity = velocity,
                    InitialVelocity = velocity,
                    Lifetime = 0f,
                    MaxLifetime = 0.5f,
                    Size = _rng.Next(4, 6),
                    StartColor = sparkColor
                });
            }
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);
            return new Vector2(
                v.X * cos - v.Y * sin,
                v.X * sin + v.Y * cos
            );
        }
    }
}
