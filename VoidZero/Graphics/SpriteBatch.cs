using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoidZero.Graphics
{
    public class SpriteBatch
    {
        private int _vao, _vbo;
        private Shader _shader;
        private Texture2D _whiteTexture;

        public SpriteBatch()
        {
            _shader = new Shader("Content/sprite.vert", "Content/sprite.frag");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                6 * 8 * sizeof(float), // 6 vertices, 8 floats each
                IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            int stride = 8 * sizeof(float);

            // position
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // texcoords
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // color
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            // Create a 1x1 white texture
            _whiteTexture = new Texture2D(1, 1, new byte[] { 255, 255, 255, 255 });

        }

        public void Begin(Matrix4 projection)
        {
            _shader.Use();
            _shader.SetMatrix4("projection", projection);
            GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "texture0"), 0);
            GL.BindVertexArray(_vao);
        }

        public void Draw(Texture2D tex, Vector2 pos, Vector2 size)
        {
            Draw(tex, pos, size, Vector4.One); // Color.White
        }

        public void Draw(Texture2D tex, Vector2 pos, Vector2 size, System.Drawing.Color color)
        {
            Draw(tex, pos, size, new Vector4(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f));
        }

        private void Draw(Texture2D tex, Vector2 pos, Vector2 size, Vector4 color)
        {
            float r = color.X;
            float g = color.Y;
            float b = color.Z;
            float a = color.W;


            float[] vertices =
            {
                // pos              // uv      // color
                pos.X, pos.Y+size.Y, 0, 1,      r,g,b,a,
                pos.X+size.X, pos.Y, 1, 0,      r,g,b,a,
                pos.X, pos.Y,        0, 0,      r,g,b,a,

                pos.X, pos.Y+size.Y, 0, 1,      r,g,b,a,
                pos.X+size.X,pos.Y+size.Y,1,1,  r,g,b,a,
                pos.X+size.X,pos.Y,  1, 0,      r,g,b,a,
            };

            GL.BindTexture(TextureTarget.Texture2D, tex.Handle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void DrawRectangle(RectangleF rect, Color color)
        {
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height), color);
        }

        public void End() { }
    }
}
