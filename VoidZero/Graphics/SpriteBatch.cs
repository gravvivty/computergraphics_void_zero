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
        public Vector4 GlobalTint { get; set; } = Vector4.One;
        public float Grayscale { get; set; } = 0f;


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
            _whiteTexture = new Texture2D(1, 1, [255, 255, 255, 255]);

        }

        public void Begin(Matrix4 projection, float grayscale = 0f)
        {
            Grayscale = grayscale;
            _shader.Use();
            _shader.SetMatrix4("projection", projection);
            _shader.SetFloat("grayscale", Grayscale);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "texture0"), 0);

            GL.BindVertexArray(_vao);

            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void Draw(Texture2D texture, Vector2 position, Vector2 size)
        {
            DrawInternal(texture, position, size, new Vector4(0, 0, 1, 1), Vector4.One);
        }

        public void DrawDebug(Texture2D texture, Vector2 position, Vector2 size, Vector4 color)
        {
            DrawInternal(texture, position, size, new Vector4(0, 0, 1, 1), color);
        }


        public void DrawFrame(Texture2D texture, Vector2 position, Rectangle source, Vector4 color, float scale = 1f)
        {
            // Convert source rect to normalized UVs
            float u0 = source.X / (float)texture.Width;
            float v0 = source.Y / (float)texture.Height;
            float u1 = (source.X + source.Width) / (float)texture.Width;
            float v1 = (source.Y + source.Height) / (float)texture.Height;

            float w = source.Width * scale;
            float h = source.Height * scale;

            // Pass the UV rectangle as (u, v, width, height)
            DrawInternal(
                texture,
                position,
                new Vector2(w, h),
                new Vector4(u0, v0, u1 - u0, v1 - v0), // UV rectangle
                color
            );
        }

        private void DrawInternal(Texture2D texture, Vector2 position, Vector2 size, Vector4 uv, Vector4 color)
        {
            Vector4 finalColor = color * GlobalTint;

            float x = position.X;
            float y = position.Y;
            float w = size.X;
            float h = size.Y;

            float u = uv.X;
            float v = uv.Y;
            float uw = uv.Z;
            float vh = uv.W;

            float r = finalColor.X;
            float g = finalColor.Y;
            float b = finalColor.Z;
            float a = finalColor.W;

            float[] vertices =
            {
                // pos              // uv            // color
                x,     y + h, u,     v + vh, r,g,b,a,
                x + w, y,     u + uw,v,      r,g,b,a,
                x,     y,     u,     v,      r,g,b,a,

                x,     y + h, u,     v + vh, r,g,b,a,
                x + w, y + h, u + uw,v + vh, r,g,b,a,
                x + w, y,     u + uw,v,      r,g,b,a
            };

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

            GL.BufferSubData(
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                vertices.Length * sizeof(float),
                vertices);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void DrawRectangle(RectangleF rectangle, Color color, bool filled = false, float thickness = 2f)
        {
            if (filled)
            {
                DrawDebug(
                    _whiteTexture,
                    new Vector2(rectangle.X, rectangle.Y),
                    new Vector2(rectangle.Width, rectangle.Height),
                    new Vector4(
                        color.R / 255f,
                        color.G / 255f,
                        color.B / 255f,
                        color.A / 255f
                    )
                );
                return;
            }

            // Top
            DrawDebug(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y),
                new Vector2(rectangle.Width, thickness),
                ColorToVec4(color));

            // Bottom
            DrawDebug(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y + rectangle.Height - thickness),
                new Vector2(rectangle.Width, thickness),
                ColorToVec4(color));

            // Left
            DrawDebug(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y),
                new Vector2(thickness, rectangle.Height),
                ColorToVec4(color));

            // Right
            DrawDebug(_whiteTexture,
                new Vector2(rectangle.X + rectangle.Width - thickness, rectangle.Y),
                new Vector2(thickness, rectangle.Height),
                ColorToVec4(color));
        }

        public void End() { }

        private Vector4 ColorToVec4(Color c)
        {
            return new Vector4(

                c.R / 255f,
                c.G / 255f,
                c.B / 255f,
                c.A / 255f
            );
        }
    }
}
