using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoidZero.Graphics.Particles;

namespace VoidZero.Graphics
{
    public class SpriteBatch
    {
        private int _vao;
        private int _vbo;
        private Shader _shader;
        private Texture2D _whiteTexture;
        private float[] _vertexData;
        private int _index = 0;
        private Texture2D _currentTexture;
        private int _textureLocation;
        public Vector4 GlobalTint { get; set; } = Vector4.One;
        public float Grayscale { get; set; } = 0f;


        public SpriteBatch()
        {
            _shader = new Shader("Content/sprite.vert", "Content/sprite.frag");

            _vao = GL.GenVertexArray(); // Vertex Array Object -> how to read buffer data
            _vbo = GL.GenBuffer(); // Vertex Buffer Object -> data
            _vertexData = new float[10000 * 6 * 8]; // 10k sprites max

            _textureLocation = GL.GetUniformLocation(_shader.Handle, "texture");

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                _vertexData.Length * sizeof(float),
                IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            int stride = 8 * sizeof(float);

            // Position
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            // Texture Coordinates / uv
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            // Color
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            // Create a 1x1 white texture -> for debugging hitboxes etc.
            _whiteTexture = new Texture2D(1, 1, [255, 255, 255, 255]);
        }

        public void Begin(Matrix4 projection, float grayscale = 0f)
        {
            Grayscale = grayscale;
            _shader.Use();
            _shader.SetMatrix4("projection", projection);
            _shader.SetFloat("grayscale", Grayscale);
            _shader.SetFloat("glowPower", 3f);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Uniform1(_textureLocation, 0);

            GL.BindVertexArray(_vao);

            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void Draw(Texture2D texture, Vector2 position, Vector2 size, Vector4 color)
        {
            if (_currentTexture != null && texture != _currentTexture)
                Flush();

            _currentTexture ??= texture;

            float x = position.X;
            float y = position.Y;
            float width = size.X;
            float height = size.Y;

            float u0 = 0f;
            float v0 = 0f;
            float u1 = 1f;
            float v1 = 1f;

            Vector4 tintedColor = color * GlobalTint;

            // Auto flush if full
            if (_index + 6 * 8 >= _vertexData.Length)
                Flush();

            // Triangle 1
            PushVertex(x, y + height, u0, v1, tintedColor);
            PushVertex(x + width, y, u1, v0, tintedColor);
            PushVertex(x, y, u0, v0, tintedColor);
            // Triangle 2
            PushVertex(x, y + height, u0, v1, tintedColor);
            PushVertex(x + width, y + height, u1, v1, tintedColor);
            PushVertex(x + width, y, u1, v0, tintedColor);
        }


        public void DrawFrame(
            Texture2D texture,
            Vector2 position,
            Rectangle source,
            Vector4 color,
            float scale = 1f,
            float rotation = 0f)
        {
            if (_currentTexture != null && texture != _currentTexture)
                Flush();

            _currentTexture ??= texture;

            // UV coordinates
            float u0 = source.X / (float)texture.Width;
            float v0 = source.Y / (float)texture.Height;
            float u1 = (source.X + source.Width) / (float)texture.Width;
            float v1 = (source.Y + source.Height) / (float)texture.Height;

            // Size
            float width = source.Width * scale;
            float height = source.Height * scale;

            Vector2 origin = new Vector2(width / 2f, height / 2f);

            Vector2[] corners =
            {
                new(-origin.X, -origin.Y),
                new( origin.X, -origin.Y),
                new(-origin.X,  origin.Y),
                new( origin.X,  origin.Y),
            };

            for (int i = 0; i < 4; i++)
            {
                corners[i] = Rotate(corners[i], rotation) + position + origin;
            }

            Vector4 tintedColor = color * GlobalTint;

            // Auto flush safety
            if (_index + 6 * 8 >= _vertexData.Length)
            {
                Flush();
            } 

            // Triangle 1
            PushVertex(corners[2].X, corners[2].Y, u0, v1, tintedColor);
            PushVertex(corners[1].X, corners[1].Y, u1, v0, tintedColor);
            PushVertex(corners[0].X, corners[0].Y, u0, v0, tintedColor);
            // Triangle 2
            PushVertex(corners[2].X, corners[2].Y, u0, v1, tintedColor);
            PushVertex(corners[3].X, corners[3].Y, u1, v1, tintedColor);
            PushVertex(corners[1].X, corners[1].Y, u1, v0, tintedColor);
        }

        public void DrawRectangle(RectangleF rectangle, Color color, bool filled = false, float thickness = 2f)
        {
            if (filled)
            {
                Draw(
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

            // Rectangle circumference
            // Top
            Draw(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y),
                new Vector2(rectangle.Width, thickness),
                ColorToVec4(color));
            // Bottom
            Draw(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y + rectangle.Height - thickness),
                new Vector2(rectangle.Width, thickness),
                ColorToVec4(color));
            // Left
            Draw(_whiteTexture,
                new Vector2(rectangle.X, rectangle.Y),
                new Vector2(thickness, rectangle.Height),
                ColorToVec4(color));
            // Right
            Draw(_whiteTexture,
                new Vector2(rectangle.X + rectangle.Width - thickness, rectangle.Y),
                new Vector2(thickness, rectangle.Height),
                ColorToVec4(color));
        }

        public void End()
        {
            Flush();
        }

        private void Flush()
        {
            if (_index == 0)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _currentTexture.Handle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferSubData(
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                _index * sizeof(float),
                _vertexData);

            int vertexCount = _index / 8;

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);

            _index = 0;
            _currentTexture = null;
        }

        private void PushVertex(float x, float y, float u, float v, Vector4 c)
        {
            _vertexData[_index++] = x;
            _vertexData[_index++] = y;
            _vertexData[_index++] = u;
            _vertexData[_index++] = v;
            _vertexData[_index++] = c.X;
            _vertexData[_index++] = c.Y;
            _vertexData[_index++] = c.Z;
            _vertexData[_index++] = c.W;
        }

        private Vector4 ColorToVec4(Color c)
        {
            return new Vector4(

                c.R / 255f,
                c.G / 255f,
                c.B / 255f,
                c.A / 255f
            );
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
