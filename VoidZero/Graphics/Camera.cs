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

        // Probably a bad implementaiton of the camera however we only use it for camera shake so might get away with this
        public Matrix4 GetProjection()
        {
            return Matrix4.CreateOrthographicOffCenter(
                0 + ShakeOffset.X,
                GameServices.Instance.Settings.WorldWidth + ShakeOffset.X,
                GameServices.Instance.Settings.WorldHeight + ShakeOffset.Y,
                0 + ShakeOffset.Y,
                -1, 1
            );
        }
    }
}
