using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace VoidZero.Graphics
{
    /// <summary>
    /// Batches sprite draw calls into a single GPU upload per texture flush.
    /// All sprites sharing the same texture are packed into one <see cref="_vertexData"/>
    /// array and sent to the GPU in one <see cref="GL.DrawArrays"/> call.
    ///
    /// Vertex layout (8 floats per vertex, 6 vertices per quad = 2 triangles):
    ///   [x, y,  u, v,  r, g, b, a]
    /// GPUs draw triangles natively; quads are always decomposed into two triangles.
    /// </summary>
    public class SpriteBatch
    {
        // GPU objects
        private readonly int _vao;
        private readonly int _vbo;
        private readonly Shader _shader;

        // 1x1 white texture used for solid color primitives (rectangles, debug boxes).
        private readonly Texture2D _whiteTexture;

        // CPU-side vertex buffer
        // 10 000 sprites * 6 vertices * 8 floats = 480 000 floats
        private const int MaxSprites = 10_000;
        private const int FloatsPerVertex = 8;
        private const int VerticesPerSprite = 6;
        private readonly float[] _vertexData = new float[MaxSprites * VerticesPerSprite * FloatsPerVertex];
        private int _head; // write cursor into _vertexData

        // State
        private Texture2D _currentTexture;
        private readonly int _textureUniformLocation;

        /// <summary>Applied as a multiplicative tint on top of every draw call's color.</summary>
        public Vector4 GlobalTint { get; set; } = Vector4.One;

        /// <summary>0 = full color, 1 = fully grayscale.</summary>
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
                _vertexData.Length * sizeof(float),
                IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            int stride = FloatsPerVertex * sizeof(float);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0); // position (xy)

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1); // tex coord (uv)

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));
            GL.EnableVertexAttribArray(2); // color (rgba)

            _textureUniformLocation = GL.GetUniformLocation(_shader.Handle, "texture");

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
            GL.Uniform1(_textureUniformLocation, 0);

            GL.BindVertexArray(_vao);

            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void End() => Flush();

        /// <summary>Draws a texture stretched over an axis-aligned rectangle.</summary>
        public void Draw(Texture2D texture, Vector2 position, Vector2 size, Vector4 color)
        {
            float x = position.X, y = position.Y;
            float w = size.X, h = size.Y;
            PushQuad(texture, color,
                topLeft: new Vector2(x, y + h),
                topRight: new Vector2(x + w, y),
                bottomLeft: new Vector2(x, y),
                bottomRight: new Vector2(x + w, y + h),
                uv: new Vector4(0f, 0f, 1f, 1f)); // u0, v0, u1, v1
        }

        /// <summary>
        /// Draws a sub-rectangle (sprite sheet frame) with optional scale and rotation.
        /// Rotation is in radians, around the frame's center.
        /// </summary>
        public void DrawFrame(
            Texture2D texture,
            Vector2 position,
            Rectangle source,
            Vector4 color,
            float scale = 1f,
            float rotation = 0f)
        {
            float u0 = source.X / (float)texture.Width;
            float v0 = source.Y / (float)texture.Height;
            float u1 = (source.X + source.Width) / (float)texture.Width;
            float v1 = (source.Y + source.Height) / (float)texture.Height;

            float hw = source.Width * scale * 0.5f;
            float hh = source.Height * scale * 0.5f;
            Vector2 origin = new(hw, hh);

            // Local space corners (centered on origin)
            Vector2 tl = Rotate(new(-hw, -hh), rotation) + position + origin;
            Vector2 tr = Rotate(new(hw, -hh), rotation) + position + origin;
            Vector2 bl = Rotate(new(-hw, hh), rotation) + position + origin;
            Vector2 br = Rotate(new(hw, hh), rotation) + position + origin;

            PushQuad(texture, color,
                topLeft: bl,
                topRight: tr,
                bottomLeft: tl,
                bottomRight: br,
                uv: new Vector4(u0, v0, u1, v1));
        }

        /// <summary>Draws a filled or outlined axis-aligned rectangle using the white texture.</summary>
        public void DrawRectangle(RectangleF rect, Color color, bool filled = false, float thickness = 2f)
        {
            Vector4 c = ColorToVec4(color);

            if (filled)
            {
                Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height), c);
                return;
            }

            // Top / Bottom / Left / Right edges
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(rect.Width, thickness), c);
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y + rect.Height - thickness), new Vector2(rect.Width, thickness), c);
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(thickness, rect.Height), c);
            Draw(_whiteTexture, new Vector2(rect.X + rect.Width - thickness, rect.Y), new Vector2(thickness, rect.Height), c);
        }

        /// <summary>
        /// Core quad-push routine shared by all draw methods.
        /// Flushes first if the texture switches or the buffer is full.
        ///
        /// Corner naming matches screen-space where Y increases downward:
        ///   bottomLeft  = visual top-left of the sprite (low Y)
        ///   topLeft     = visual bottom-left            (high Y)
        /// </summary>
        private void PushQuad(
            Texture2D texture, Vector4 color,
            Vector2 topLeft, Vector2 topRight,
            Vector2 bottomLeft, Vector2 bottomRight,
            Vector4 uv) // (u0, v0, u1, v1)
        {
            if (_currentTexture != null && texture != _currentTexture)
                Flush();

            _currentTexture ??= texture;

            if (_head + VerticesPerSprite * FloatsPerVertex >= _vertexData.Length)
                Flush();

            Vector4 tinted = color * GlobalTint;
            float u0 = uv.X, v0 = uv.Y, u1 = uv.Z, v1 = uv.W;

            // Triangle 1
            PushVertex(topLeft.X, topLeft.Y, u0, v1, tinted);
            PushVertex(topRight.X, topRight.Y, u1, v0, tinted);
            PushVertex(bottomLeft.X, bottomLeft.Y, u0, v0, tinted);
            // Triangle 2
            PushVertex(topLeft.X, topLeft.Y, u0, v1, tinted);
            PushVertex(bottomRight.X, bottomRight.Y, u1, v1, tinted);
            PushVertex(topRight.X, topRight.Y, u1, v0, tinted);
        }

        private void Flush()
        {
            if (_head == 0 || _currentTexture == null)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _currentTexture.Handle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferSubData(
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                _head * sizeof(float),
                _vertexData);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _head / FloatsPerVertex);

            _head = 0;
            _currentTexture = null;
        }

        private void PushVertex(float x, float y, float u, float v, Vector4 c)
        {
            _vertexData[_head++] = x;
            _vertexData[_head++] = y;
            _vertexData[_head++] = u;
            _vertexData[_head++] = v;
            _vertexData[_head++] = c.X;
            _vertexData[_head++] = c.Y;
            _vertexData[_head++] = c.Z;
            _vertexData[_head++] = c.W;
        }

        private static Vector4 ColorToVec4(Color c) =>
            new(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }
    }
}