# Particle Hit-Glow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Spark particles spawned on bullet-enemy hit flash bright the instant they spawn, then fade back to their normal color over the first 20% of their lifetime, implemented as a real per-vertex shader effect.

**Architecture:** Add one new per-vertex float attribute (`aGlow`, 0–1 intensity) to the existing single shared `SpriteBatch` vertex format. The fragment shader brightens a fragment toward/past white proportional to `GlowIntensity * glowPower`. Every existing draw call defaults `glow` to `0f`, so nothing except particles changes visually. `Particle` computes its own `GlowIntensity` each frame the same way it already computes `CurrentColor`.

**Tech Stack:** C# / .NET 8, OpenTK (OpenGL 3.3 core), GLSL `#version 330 core`.

## Global Constraints

- No test project exists in this repo (`VoidZero.csproj` is the only project). Verification is `dotnet build` (compiles + links) plus a manual run of the game — do not add a test framework as part of this work.
- Every existing draw call site (UI, background, bullets, enemies, `DrawFrame`, the UV-rect `Draw` overload) must keep rendering pixel-identical to today. Only add `glow` parameters to the methods actually on the particle draw path (`DrawRectangle` and the position/size/color `Draw` overload) — do not touch `DrawFrame` or the UV overload, they're unused by this feature.
- Match existing code style: no comments except where a value/threshold isn't self-explanatory (e.g. the existing "first 80% slow, last 20% fast" comment on the color fade) — follow that same commenting density, not more.

---

### Task 1: Add a per-vertex glow attribute through the render pipeline

**Files:**
- Modify: `Graphics/SpriteBatch.cs`
- Modify: `Content/sprite.vert`
- Modify: `Content/sprite.frag`

**Interfaces:**
- Produces: `SpriteBatch.Draw(Texture2D texture, Vector2 position, Vector2 size, Vector4 color, float glow = 0f)` and `SpriteBatch.DrawRectangle(RectangleF rect, Color color, bool filled = false, float thickness = 2f, float glow = 0f)` — both now accept an optional glow intensity that Task 2 will use.

- [ ] **Step 1: Update the vertex layout doc comment and constants**

In `Graphics/SpriteBatch.cs`, replace the class doc comment and the buffer-size comment/constant:

```csharp
    /// <summary>
    /// Batches sprite draw calls into a single GPU upload per texture flush.
    /// All sprites sharing the same texture are packed into one <see cref="_vertexData"/>
    /// array and sent to the GPU in one <see cref="GL.DrawArrays"/> call.
    ///
    /// Vertex layout (9 floats per vertex, 6 vertices per quad = 2 triangles):
    ///   [x, y,  u, v,  r, g, b, a,  glow]
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
        // 10 000 sprites * 6 vertices * 9 floats = 540 000 floats
        private const int MaxSprites = 10_000;
        private const int FloatsPerVertex = 9;
```

- [ ] **Step 2: Add the vertex attribute binding in the constructor**

In `Graphics/SpriteBatch.cs`, immediately after the existing color attribute block in the constructor:

```csharp
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));
            GL.EnableVertexAttribArray(2); // color (rgba)

            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));
            GL.EnableVertexAttribArray(3); // glow intensity
```

- [ ] **Step 3: Thread `glow` through `PushVertex` and `PushQuad`**

Replace `PushVertex` and `PushQuad` in `Graphics/SpriteBatch.cs`:

```csharp
        private void PushVertex(float x, float y, float u, float v, Vector4 c, float glow)
        {
            _vertexData[_head++] = x;
            _vertexData[_head++] = y;
            _vertexData[_head++] = u;
            _vertexData[_head++] = v;
            _vertexData[_head++] = c.X;
            _vertexData[_head++] = c.Y;
            _vertexData[_head++] = c.Z;
            _vertexData[_head++] = c.W;
            _vertexData[_head++] = glow;
        }
```

```csharp
        private void PushQuad(
            Texture2D texture, Vector4 color,
            Vector2 topLeft, Vector2 topRight,
            Vector2 bottomLeft, Vector2 bottomRight,
            Vector4 uv, // (u0, v0, u1, v1)
            float glow = 0f)
        {
            if (_currentTexture != null && texture != _currentTexture)
                Flush();

            _currentTexture ??= texture;

            if (_head + VerticesPerSprite * FloatsPerVertex >= _vertexData.Length)
                Flush();

            Vector4 tinted = color * GlobalTint;
            float u0 = uv.X, v0 = uv.Y, u1 = uv.Z, v1 = uv.W;

            // Triangle 1
            PushVertex(topLeft.X, topLeft.Y, u0, v1, tinted, glow);
            PushVertex(topRight.X, topRight.Y, u1, v0, tinted, glow);
            PushVertex(bottomLeft.X, bottomLeft.Y, u0, v0, tinted, glow);
            // Triangle 2
            PushVertex(topLeft.X, topLeft.Y, u0, v1, tinted, glow);
            PushVertex(bottomRight.X, bottomRight.Y, u1, v1, tinted, glow);
            PushVertex(topRight.X, topRight.Y, u1, v0, tinted, glow);
        }
```

Note `PushQuad`'s other three call sites (`Draw` with uv, `DrawFrame`) aren't shown here — they're unchanged; `glow` defaults to `0f` for them since they don't pass it.

- [ ] **Step 4: Add optional `glow` to the position/size `Draw` overload**

Replace the first `Draw` overload in `Graphics/SpriteBatch.cs` (the one `DrawRectangle` calls):

```csharp
        /// <summary>Draws a texture stretched over an axis-aligned rectangle.</summary>
        public void Draw(Texture2D texture, Vector2 position, Vector2 size, Vector4 color, float glow = 0f)
        {
            float x = position.X, y = position.Y;
            float w = size.X, h = size.Y;
            PushQuad(texture, color,
                topLeft: new Vector2(x, y + h),
                topRight: new Vector2(x + w, y),
                bottomLeft: new Vector2(x, y),
                bottomRight: new Vector2(x + w, y + h),
                uv: new Vector4(0f, 0f, 1f, 1f), // u0, v0, u1, v1
                glow: glow);
        }
```

- [ ] **Step 5: Add optional `glow` to `DrawRectangle`**

Replace `DrawRectangle` in `Graphics/SpriteBatch.cs`:

```csharp
        /// <summary>Draws a filled or outlined axis-aligned rectangle using the white texture.</summary>
        public void DrawRectangle(RectangleF rect, Color color, bool filled = false, float thickness = 2f, float glow = 0f)
        {
            Vector4 c = ColorToVec4(color);

            if (filled)
            {
                Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height), c, glow);
                return;
            }

            // Top / Bottom / Left / Right edges
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(rect.Width, thickness), c, glow);
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y + rect.Height - thickness), new Vector2(rect.Width, thickness), c, glow);
            Draw(_whiteTexture, new Vector2(rect.X, rect.Y), new Vector2(thickness, rect.Height), c, glow);
            Draw(_whiteTexture, new Vector2(rect.X + rect.Width - thickness, rect.Y), new Vector2(thickness, rect.Height), c, glow);
        }
```

- [ ] **Step 6: Wire the attribute through the vertex shader**

Replace `Content/sprite.vert` entirely:

```glsl
#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTex;
layout (location = 2) in vec4 aColor;
layout (location = 3) in float aGlow;

out vec2 TexCoord;
out vec4 VertexColor;
out float GlowIntensity;

uniform mat4 projection;

void main()
{
    TexCoord = aTex;
    VertexColor = aColor;
    GlowIntensity = aGlow;
    gl_Position = projection * vec4(aPos, 0.0, 1.0);
}
```

- [ ] **Step 7: Apply the glow brightening in the fragment shader**

Replace `Content/sprite.frag` entirely:

```glsl
#version 330 core

in vec2 TexCoord;
in vec4 VertexColor;
in float GlowIntensity;

out vec4 FragColor;

uniform sampler2D texture0;
uniform float glowPower;

void main()
{
    vec4 color = texture(texture0, TexCoord) * VertexColor;
    color.rgb += color.rgb * GlowIntensity * glowPower;

    FragColor = color;
}
```

- [ ] **Step 8: Build and verify no regressions**

Run: `dotnet build`
Expected: `Build succeeded.` with 0 errors (the pre-existing `CS0649` warning on `PlayState._stageCompleted` is unrelated and still fine to see).

Then run the game (`dotnet run`) and confirm the title/menu screen, background, and any UI render exactly as before — nothing should look different yet, since every existing call site passes `glow: 0f` implicitly. This confirms the new attribute plumbing didn't break anything before Task 2 makes it visible.

- [ ] **Step 9: Commit**

```bash
git add Graphics/SpriteBatch.cs Content/sprite.vert Content/sprite.frag
git commit -m "feat: add per-vertex glow attribute to sprite render pipeline"
```

---

### Task 2: Fade a hot-flash glow on spark particles

**Files:**
- Modify: `Graphics/Particles/Particle.cs`

**Interfaces:**
- Consumes: `SpriteBatch.DrawRectangle(RectangleF rect, Color color, bool filled = false, float thickness = 2f, float glow = 0f)` from Task 1.
- Produces: `Particle.GlowIntensity` (public float field, 0–1), readable by anything that wants to know a particle's current flash strength (nothing else needs it yet — YAGNI).

- [ ] **Step 1: Add the `GlowIntensity` field**

In `Graphics/Particles/Particle.cs`, add it next to the existing color fields:

```csharp
        public Vector4 StartColor;
        public Vector4 EndColor = new Vector4(0.3f, 0.3f, 0.3f, 1f); // Gray
        public Vector4 CurrentColor;
        public float GlowIntensity;
```

- [ ] **Step 2: Compute the glow fade in `Update`**

Replace the body of `Update` in `Graphics/Particles/Particle.cs`:

```csharp
        public void Update(float dt)
        {
            Lifetime += dt;
            Position += Velocity * dt;

            // Drag
            Velocity *= 0.98f;

            // Color fade -> first 80% slow, last 20% fast
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

            // Glow fade -> hot flash on spawn, fully cooled by 20% of lifetime
            GlowIntensity = t < 0.2f ? MathHelper.Lerp(1f, 0f, t / 0.2f) : 0f;
        }
```

- [ ] **Step 3: Pass the glow intensity into the draw call**

Replace the body of `Draw` in `Graphics/Particles/Particle.cs`:

```csharp
        public void Draw(SpriteBatch batch)
        {
            int r = (int)(CurrentColor.X * 255);
            int g = (int)(CurrentColor.Y * 255);
            int b = (int)(CurrentColor.Z * 255);
            int a = (int)(CurrentColor.W * 255);

            batch.DrawRectangle(
                new RectangleF(Position.X, Position.Y, Size, Size),
                Color.FromArgb(a, r, g, b),
                filled: true,
                glow: GlowIntensity
            );
        }
```

- [ ] **Step 4: Build and manually verify the effect**

Run: `dotnet build`
Expected: `Build succeeded.` with 0 errors.

Run the game (`dotnet run`), start a level, and shoot an enemy. Expected: on impact, the spark particles flash noticeably brighter/whiter than their spawn color, then visibly cool down to their normal orange/yellow spark color within roughly the first tenth of a second (20% of their 0.5s `MaxLifetime`). Confirm bullets, enemies, background, and UI still look unchanged (they never set `glow`, so `GlowIntensity` stays irrelevant to them).

- [ ] **Step 5: Commit**

```bash
git add Graphics/Particles/Particle.cs
git commit -m "feat: fade a hot-flash glow on spark particles after enemy hits"
```
