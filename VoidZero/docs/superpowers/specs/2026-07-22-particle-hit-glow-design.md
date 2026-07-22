# Particle Hit-Glow Design

**Goal:** Spark particles spawned when a bullet hits an enemy flash bright ("hot") the instant they spawn, then fade back down to their normal color over the first portion of their lifetime — implemented as a real shader effect, not a CPU-side color hack.

## Architecture

The existing `SpriteBatch` renders every sprite (UI, background, bullets, enemies, particles) through one shared shader and one batched vertex buffer. All draws in a frame share a single `Begin()`/`End()` pair, so any glow value must travel per-vertex (a uniform can't vary within one batched draw call, and particles from different hits can be mid-flash simultaneously).

We add one new per-vertex float attribute, `aGlow` (0–1 intensity), alongside the existing `aColor`. The fragment shader uses it to brighten a fragment's color toward/past white:

```glsl
color.rgb += color.rgb * GlowIntensity * glowPower;
```

`glowPower` is a uniform that already exists (`SpriteBatch.Begin()` sets it to `3f`) but is currently unused by `sprite.frag` — dead code left over from an earlier, unfinished attempt at this same feature. This design wires it up instead of introducing a second uniform.

`GlowIntensity` is 0 for every existing draw call (UI, background, bullets, enemies) — they simply don't pass a glow value, so nothing about their rendering changes. Only `Particle.Draw()` passes a non-zero value.

## Components

### `Graphics/SpriteBatch.cs`

- Vertex layout grows from 8 floats/vertex (`x, y, u, v, r, g, b, a`) to 9 (`x, y, u, v, r, g, b, a, glow`). Update the `FloatsPerVertex` constant, the doc comment, and the `GL.VertexAttribPointer` calls (existing color attribute's stride changes; new attribute added at location 3).
- `PushQuad(...)` gains a `float glow = 0f` parameter, written into the vertex buffer alongside color for all 6 vertices of the quad.
- `Draw(...)` and `DrawRectangle(...)` (the public draw APIs) gain a matching `float glow = 0f` parameter, threaded through to `PushQuad`. Default keeps every current call site (UI, background, bullets, enemies) source-compatible and visually unchanged.

### `Content/sprite.vert`

- New input: `layout (location = 3) in float aGlow;`
- New output: `out float GlowIntensity;`
- `GlowIntensity = aGlow;` in `main()`.

### `Content/sprite.frag`

- New input: `in float GlowIntensity;`
- New uniform: `uniform float glowPower;` (already set from C#, just not declared/used in this file yet).
- After sampling: `color.rgb += color.rgb * GlowIntensity * glowPower;`
  - At `GlowIntensity == 0` this is a no-op (matches current visuals exactly).
  - At `GlowIntensity == 1` and `glowPower == 3`, color is boosted 4x, which — combined with the existing alpha blend against an 8-bit backbuffer — clips toward white, giving the "hot flash" look.

### `Graphics/Particles/Particle.cs`

- New field: `public float GlowIntensity;`
- In `Update()`, alongside the existing color-fade block, compute the glow fade:
  ```csharp
  const float glowFadeFraction = 0.2f; // matches the existing 80/20 split used for color fade
  float glowT = Lifetime / MaxLifetime;
  GlowIntensity = glowT < glowFadeFraction
      ? MathHelper.Lerp(1f, 0f, glowT / glowFadeFraction)
      : 0f;
  ```
- In `Draw()`, pass `GlowIntensity` into the existing `batch.DrawRectangle(...)` call via the new `glow` parameter.

### `Graphics/Particles/ParticleSystem.cs`

No changes needed — `SpawnSparks` already constructs `Particle` instances; `GlowIntensity` starts at its default (`0f`) and is computed from `Update()` on the very first tick before the first `Draw()`, same lifecycle as `CurrentColor`.

## Data Flow

1. `PlayState` calls `ParticleSystem.SpawnSparks(...)` on bullet-enemy hit → `Particle`s are created with `Lifetime = 0`.
2. Each frame, `ParticleSystem.Update(dt)` → `Particle.Update(dt)` advances `Lifetime`, recomputes `CurrentColor` (existing) and `GlowIntensity` (new).
3. `ParticleSystem.Draw(batch)` → `Particle.Draw(batch)` calls `batch.DrawRectangle(rect, color, glow: GlowIntensity, filled: true)`.
4. `SpriteBatch` writes `glow` into the vertex buffer for that quad; unrelated draws in the same batch (background, enemies, UI) pass `glow: 0f` implicitly via the default parameter.
5. GPU: vertex shader passes `GlowIntensity` through; fragment shader brightens only the particle's fragments.

## Edge Cases

- **`MaxLifetime` of 0:** not possible today (`SpawnSparks` hardcodes `MaxLifetime = 0.5f`); no guard needed (YAGNI — matches how the existing color-fade math already assumes `MaxLifetime > 0`).
- **Multiple simultaneous hits:** each particle carries its own `GlowIntensity` computed from its own `Lifetime`, so overlapping bursts at different points in their flash independently — this is exactly why the value must be per-vertex, not a shared uniform.
- **Non-particle sprites:** unaffected — they never pass a `glow` argument, so it defaults to `0f` and the shader's added term is `0`.

## Testing

This is a purely visual effect with no discrete inputs/outputs suitable for unit tests (no existing test project in this repo). Verification is manual:
- Build the project (`dotnet build`) to confirm shader/vertex-format changes compile and link (attribute location mismatches would still build in C# but fail/misrender at runtime, so also run the game).
- Run the game, shoot an enemy, and visually confirm sparks flash bright on impact and cool down to their normal spark color over roughly the first 20% of their 0.5s lifetime.
- Confirm no visual regression elsewhere (UI, background, bullets, enemies still render identically), since their `glow` defaults to `0`.
