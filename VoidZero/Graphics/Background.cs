using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Graphics;
using VoidZero.Graphics.Particles;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace VoidZero.Core
{
    public class Background
    {

        private class Planet
        {
            public float XNorm; // horizontal position as 0..1, multiplied by screen width when drawing
            public float Y;
            public float Speed;
            public float Scale;
        }

        private class Star
        {
            public float XNorm;
            public float Y;
            public float Speed;
            public float Scale;
        }

        private class Galaxy
        {
            public float XNorm;
            public float Y;
            public float Speed;
            public float Scale;
            public Vector4 Color;
            public AnimationManager Animations;
        }

        private const float BaseSpaceSpeed = 80f;
        private const float BaseStarSpeed = 40f;
        private const float BasePlanetSpeed = 120f;
        private const float BaseGalaxySpeed = 5f;

        private const int StarCount = 80;
        private const int MaxPlanets = 3;
        private const int GalaxyCount = 4;

        private readonly Texture2D _spaceTexture;
        private readonly Texture2D _starsTexture;
        private readonly Texture2D _planetsTexture;
        private readonly Texture2D _galaxyTexture;

        private readonly int _screenWidth;
        private readonly int _screenHeight;

        private readonly List<Planet> _planets = new();
        private readonly List<Star> _stars = new();
        private readonly List<Galaxy> _galaxies = new();

        private float _spaceOffset = 500f;

        private readonly Random _rng = new();


        public Background(
            int screenWidth, int screenHeight,
            Texture2D space, Texture2D stars, Texture2D planets, Texture2D galaxies)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _spaceTexture = space;
            _starsTexture = stars;
            _planetsTexture = planets;
            _galaxyTexture = galaxies;

            for (int i = 0; i < MaxPlanets; i++) _planets.Add(SpawnPlanet(spawnAbove: false));
            for (int i = 0; i < StarCount; i++) _stars.Add(SpawnStar(spawnAbove: false));
            for (int i = 0; i < GalaxyCount; i++) _galaxies.Add(SpawnGalaxy(spawnAbove: false));
        }

        public void Update(float dt)
        {
            float mult = GameServices.Instance.Settings.BackgroundSpeedMultiplier;

            // Scroll the tiled space backdrop and wrap it when it's moved a full tile height
            _spaceOffset += BaseSpaceSpeed * dt * mult;
            float tileHeight = _spaceTexture.Height * SpaceFillScale();
            if (_spaceOffset >= tileHeight)
                _spaceOffset -= tileHeight;

            UpdateStars(dt, mult);
            UpdatePlanets(dt, mult);
            UpdateGalaxies(dt, mult);
        }

        private void UpdateStars(float dt, float mult)
        {
            for (int i = _stars.Count - 1; i >= 0; i--)
            {
                Star s = _stars[i];
                s.Y += s.Speed * dt * mult;

                if (s.Y > _screenHeight + _starsTexture.Height * s.Scale)
                {
                    _stars[i] = SpawnStar(spawnAbove: true);
                }
            }
        }

        private void UpdatePlanets(float dt, float mult)
        {
            for (int i = _planets.Count - 1; i >= 0; i--)
            {
                Planet p = _planets[i];
                p.Y += p.Speed * dt * mult;

                if (p.Y > _screenHeight + _planetsTexture.Height * p.Scale)
                {
                    _planets[i] = SpawnPlanet(spawnAbove: true);
                }
            }
        }

        private void UpdateGalaxies(float dt, float mult)
        {
            for (int i = _galaxies.Count - 1; i >= 0; i--)
            {
                Galaxy g = _galaxies[i];
                g.Y += g.Speed * dt * mult;
                g.Animations.Update(dt);

                if (g.Y > _screenHeight + 64f * g.Scale)
                {
                    _galaxies[i] = SpawnGalaxy(spawnAbove: true);
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            DrawStars(batch);
            DrawGalaxies(batch);
            DrawSpaceTile(batch);
            DrawPlanets(batch);
        }

        private void DrawSpaceTile(SpriteBatch batch)
        {
            float scale = SpaceFillScale();
            Vector2 size = new(_spaceTexture.Width * scale, _spaceTexture.Height * scale);

            // Draw two tiles stacked vertically so there's never a gap while scrolling
            batch.Draw(_spaceTexture, new Vector2(0, _spaceOffset), size, Vector4.One);
            batch.Draw(_spaceTexture, new Vector2(0, _spaceOffset - size.Y), size, Vector4.One);
        }

        private void DrawStars(SpriteBatch batch)
        {
            foreach (Star s in _stars)
            {
                Vector2 pos = new(s.XNorm * _screenWidth, s.Y);
                Vector2 size = new(_starsTexture.Width * s.Scale, _starsTexture.Height * s.Scale);
                batch.Draw(_starsTexture, pos, size, Vector4.One);
            }
        }

        private void DrawPlanets(SpriteBatch batch)
        {
            foreach (Planet p in _planets)
            {
                Vector2 pos = new(p.XNorm * _screenWidth, p.Y);
                Vector2 size = new(_planetsTexture.Width * p.Scale, _planetsTexture.Height * p.Scale);
                batch.Draw(_planetsTexture, pos, size, Vector4.One);
            }
        }

        private void DrawGalaxies(SpriteBatch batch)
        {
            foreach (Galaxy g in _galaxies)
            {
                batch.GlobalTint = Vector4.One;
                g.Animations.Draw(batch, new Vector2(g.XNorm * _screenWidth, g.Y), g.Scale, g.Color);
            }

            batch.GlobalTint = Vector4.One;
        }

        private Planet SpawnPlanet(bool spawnAbove)
        {
            float scale = _rng.NextSingle() * 1.5f + 0.5f;

            return new Planet
            {
                XNorm = _rng.NextSingle(),
                Y = spawnAbove ? -_planetsTexture.Height * scale : _rng.Next(0, _screenHeight),
                Speed = BasePlanetSpeed + _rng.NextSingle() * 20f,
                Scale = scale,
            };
        }

        private Star SpawnStar(bool spawnAbove)
        {
            float scale = _rng.NextSingle() * 2.0f + 0.2f;

            return new Star
            {
                XNorm = _rng.NextSingle(),
                Y = spawnAbove ? -_starsTexture.Height * scale : _rng.Next(0, _screenHeight),
                Speed = BaseStarSpeed + _rng.NextSingle() * 15f,
                Scale = scale,
            };
        }

        private Galaxy SpawnGalaxy(bool spawnAbove)
        {
            float scale = _rng.NextSingle() * 1.2f + 1.0f;

            var anim = new AnimationManager();
            anim.Add("Idle", new Animation(
                _galaxyTexture,
                frameWidth: 100,
                frameHeight: 100,
                frameCount: 10,
                frameTime: 0.12f,
                column: 0,
                loop: true));
            anim.Play("Idle");

            return new Galaxy
            {
                XNorm = _rng.NextSingle(),
                Y = spawnAbove ? -64f * scale : _rng.Next(0, _screenHeight),
                Speed = BaseGalaxySpeed + _rng.NextSingle() * 8f,
                Scale = scale,
                Color = RandomGalaxyColor(),
                Animations = anim,
            };
        }

        // Returns the scale needed to make the space texture fill the screen without any gaps
        private float SpaceFillScale()
        {
            return MathF.Max(
                (float)_screenWidth / _spaceTexture.Width,
                (float)_screenHeight / _spaceTexture.Height);
        }

        private Vector4 RandomGalaxyColor()
        {
            float dark = 0.05f + _rng.NextSingle() * 0.08f;
            float pinkBias = 0.08f + _rng.NextSingle() * 0.12f;
            float noise = (_rng.NextSingle() - 0.5f) * 0.03f;

            float r = dark + pinkBias * 0.9f + noise;
            float g = dark + pinkBias * 0.6f - noise * 0.5f;
            float b = dark + pinkBias * 0.8f + noise * 0.3f;

            // Pull the color slightly toward gray to desaturate it
            float gray = (r + g + b) / 3f;
            return new Vector4(
                MathHelper.Lerp(r, gray, 0.55f),
                MathHelper.Lerp(g, gray, 0.55f),
                MathHelper.Lerp(b, gray, 0.55f),
                0.45f);
        }
    }
}