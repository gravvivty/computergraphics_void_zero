using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Game;

namespace VoidZero.Graphics
{
    public class Camera
    {
        public Vector2 ShakeOffset;
        public Vector2 Position;
        public float Zoom = 1f; // we dont need zoom but it is here just in case
        // as of now camera does not follow the player , we do this by design

        // Probably a bad implementaiton of the camera however we only use it for camera shake so might get away with this
        public Matrix4 GetViewMatrix()
        {
            Vector2 finalPos = Position + ShakeOffset;

            return Matrix4.CreateTranslation(
                -finalPos.X,
                -finalPos.Y,
                0f
            );
        }

        public Matrix4 GetProjectionMatrix()
        {
            float worldWidth =
                GameServices.Instance.Settings.WorldWidth / Zoom;

            float worldHeight =
                GameServices.Instance.Settings.WorldHeight / Zoom;

            return Matrix4.CreateOrthographicOffCenter(
                0,
                worldWidth,
                worldHeight,
                0,
                -1,
                1
            );
        }

        public Matrix4 GetViewProjectionMatrix()
        {
            return GetProjectionMatrix() * GetViewMatrix();
        }

        public Vector2 WorldToScreen(
            Vector2 world,
            Vector2 viewportOffset,
            Vector2 viewportSize)
        {
            Vector2 cameraSpace =
                world - (Position + ShakeOffset);

            float visibleWorldWidth =
                GameServices.Instance.Settings.WorldWidth / Zoom;

            float visibleWorldHeight =
                GameServices.Instance.Settings.WorldHeight / Zoom;

            float scaleX =
                viewportSize.X / visibleWorldWidth;

            float scaleY =
                viewportSize.Y / visibleWorldHeight;

            return new Vector2(
                viewportOffset.X + cameraSpace.X * scaleX,
                viewportOffset.Y + cameraSpace.Y * scaleY
            );
        }
    }
}
