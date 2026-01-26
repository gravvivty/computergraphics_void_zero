using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Combat;
using VoidZero.Utils;
using System;

namespace VoidZero.Game.Entities
{
    public class Shield : Entity
    {
        private readonly Player _player;
        private float _flashTimer = 0f;
        private float FlashDuration = 0.5f;

        public Shield(Texture2D texture, Player player)
            : base(texture, player.Position, 32, 32)
        {
            _player = player;

            Scale = 4f;
            Width = 32 * Scale;
            Height = 32 * Scale;

            Animations.Add("Idle", new Animation(texture, 32, 32, 3, 0.1f));
            Animations.Play("Idle");
        }

        public override void Update(float dt)
        {
            Position = _player.Position +
                new Vector2(
                    (_player.Width - Width) * 0.5f,
                    (_player.Height - Height) * 0.5f
                );

            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
            }

            Animations.Update(dt);
        }

        public override void Draw(SpriteBatch batch)
        {
            Vector4 baseTint = BulletColorHelper.GetTint(_player.ActiveShield);

            float time = _flashTimer / FlashDuration;
            time = MathF.Max(0f, MathF.Min(1f, time));

            float shieldFlash = MathF.Sqrt(time);

            Animations.Draw(batch, Position, Scale, baseTint);

            // Flash ring (bigger, centered)
            if (shieldFlash > 0f)
            {
                float flashScale = Scale * 1.25f;
                Vector4 flashTint = Vector4.One * shieldFlash;

                float scaleRatio = flashScale / Scale;

                Vector2 flashOffset = new Vector2(
                    Width * (scaleRatio - 1f) * 0.5f,
                    Height * (scaleRatio - 1f) * 0.5f
                );

                Animations.Draw(
                    batch,
                    Position - flashOffset,
                    flashScale,
                    flashTint
                );
            }
        }

        public void Flash()
        {
            _flashTimer = FlashDuration;
        }
    }
}
