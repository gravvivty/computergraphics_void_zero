using OpenTK.Mathematics;
using VoidZero.Graphics;
using System.Drawing;

namespace VoidZero.Graphics.Particles
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 InitialVelocity;
        public float Lifetime;
        public float MaxLifetime;
        public float Size;

        public Vector4 StartColor;
        public Vector4 EndColor = new Vector4(0.3f, 0.3f, 0.3f, 1f); // dull gray
        public Vector4 CurrentColor;

        public bool IsAlive => Lifetime < MaxLifetime;

        public void Update(float dt)
        {
            Lifetime += dt;
            Position += Velocity * dt;

            // drag
            Velocity *= 0.98f;

            // Color fade: first 80% slow, last 20% fast
            float t = Lifetime / MaxLifetime;
            if (t < 0.8f)
            {
                CurrentColor = Vector4.Lerp(StartColor, EndColor, t / 0.8f);
            }
            else
            {
                float tFast = (t - 0.8f) / 0.2f;
                CurrentColor = Vector4.Lerp(StartColor, EndColor, 0.8f + tFast * 0.2f);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            int r = (int)(CurrentColor.X * 255);
            int g = (int)(CurrentColor.Y * 255);
            int b = (int)(CurrentColor.Z * 255);
            int a = (int)(CurrentColor.W * 255);

            batch.DrawRectangle(
                new RectangleF(Position.X, Position.Y, Size, Size),
                Color.FromArgb(a, r, g, b),
                filled: true
            );
        }
    }
}