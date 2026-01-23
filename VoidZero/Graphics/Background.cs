using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using VoidZero.Game;
using VoidZero.Graphics;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace VoidZero.Core
{
    public class Background
    {
        private struct Planet
        {
            public float XNorm;   // normalized horizontal position [0..1]
            public float Y;       // vertical position in pixels
            public float Speed;
            public float Scale;
        }

        private struct Star
        {
            public float XNorm;
            public float Y;
            public float Speed;
            public float Scale;
        }

        private float _spaceOffset = 500f;

        private const float BaseSpaceSpeed = 80f;
        private const float BaseStarSpeed = 40f;
        private const float BasePlanetSpeed = 120f;

        private readonly Texture2D _spaceTexture;
        private readonly Texture2D _starsTexture;
        private readonly Texture2D _planetsTexture;

        private int _screenWidth;
        private int _screenHeight;

        private readonly List<Planet> _planets = new();
        private readonly List<Star> _stars = new();
        private readonly Random _rng = new();

        private const int StarCount = 80;
        private const int MaxPlanets = 8;

        public Background(int screenWidth, int screenHeight, Texture2D space, Texture2D stars, Texture2D planets)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            _spaceTexture = space;
            _starsTexture = stars;
            _planetsTexture = planets;

            // Spawn initial planets and stars
            for (int i = 0; i < MaxPlanets; i++)
            {
                SpawnPlanet(false);
            }

            for (int i = 0; i < StarCount; i++)
            {
                SpawnStar(false);
            }
        }

        public void Resize(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                return;
            }

            _screenWidth = width;
            _screenHeight = height;
        }

        public void Update(float dt)
        {
            float multiplier = GameServices.Instance.Settings.BackgroundSpeedMultiplier;

            // Scroll background downward
            _spaceOffset += BaseSpaceSpeed * dt * multiplier;
            float scale = FillScale();
            float scaledHeight = _spaceTexture.Height * scale;
            WrapOffset(ref _spaceOffset, scaledHeight);

            UpdatePlanets(dt, multiplier);
            UpdateStars(dt, multiplier);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, _spaceTexture, _spaceOffset, FillScale());
            DrawStars(spriteBatch);
            DrawPlanets(spriteBatch);
        }

        private void DrawLayer(SpriteBatch spriteBatch, Texture2D texture, float offset, float scale)
        {
            Vector2 size = new Vector2(texture.Width * scale, texture.Height * scale);

            // Downward scrolling
            Vector2 pos1 = new Vector2(0, offset);
            Vector2 pos2 = new Vector2(0, offset - size.Y);

            spriteBatch.Draw(texture, pos1, size);
            spriteBatch.Draw(texture, pos2, size);
        }

        private void SpawnPlanet(bool spawnAbove)
        {
            float scale = _rng.NextSingle() * 1.5f + 1.5f;
            float y = spawnAbove ? -_planetsTexture.Height * scale : _rng.Next(0, _screenHeight);
            float xNorm = _rng.NextSingle(); // normalized horizontal position

            _planets.Add(new Planet
            {
                XNorm = xNorm,
                Y = y,
                Speed = BasePlanetSpeed + _rng.NextSingle() * 20f, // some variation
                Scale = scale
            });
        }

        private void SpawnStar(bool spawnAbove)
        {
            float scale = _rng.NextSingle() * 2.0f + 0.2f;
            float y = spawnAbove ? -_starsTexture.Height * scale : _rng.Next(0, _screenHeight);
            float xNorm = _rng.NextSingle(); // normalized horizontal position

            _stars.Add(new Star
            {
                XNorm = xNorm,
                Y = y,
                Speed = BaseStarSpeed + _rng.NextSingle() * 15f, // some variation
                Scale = scale
            });
        }

        private void UpdatePlanets(float dt, float multiplier)
        {
            for (int i = _planets.Count - 1; i >= 0; i--)
            {
                Planet planet = _planets[i];
                planet.Y += planet.Speed * dt * multiplier;

                if (planet.Y > _screenHeight + _planetsTexture.Height * planet.Scale)
                {
                    _planets.RemoveAt(i);
                    SpawnPlanet(true); // spawn at top only
                }
                else
                {
                    _planets[i] = planet;
                }
            }
        }

        private void UpdateStars(float dt, float multiplier)
        {
            for (int i = _stars.Count - 1; i >= 0; i--)
            {
                Star star = _stars[i];
                star.Y += star.Speed * dt * multiplier;

                if (star.Y > _screenHeight + _starsTexture.Height * star.Scale)
                {
                    _stars.RemoveAt(i);
                    SpawnStar(true);
                }
                else
                {
                    _stars[i] = star;
                }
            }
        }

        private void DrawPlanets(SpriteBatch spriteBatch)
        {
            foreach (Planet planet in _planets)
            {
                Vector2 size = new Vector2(_planetsTexture.Width * planet.Scale, _planetsTexture.Height * planet.Scale);
                Vector2 position = new Vector2(planet.XNorm * _screenWidth, planet.Y);
                spriteBatch.Draw(_planetsTexture, position, size);
            }
        }

        private void DrawStars(SpriteBatch spriteBatch)
        {
            foreach (Star star in _stars)
            {
                Vector2 size = new Vector2(_starsTexture.Width * star.Scale, _starsTexture.Height * star.Scale);
                Vector2 position = new Vector2(star.XNorm * _screenWidth, star.Y);
                spriteBatch.Draw(_starsTexture, position, size);
            }
        }

        private float FillScale()
        {
            return MathF.Max((float)_screenWidth / _spaceTexture.Width, (float)_screenHeight / _spaceTexture.Height);
        }

        private void WrapOffset(ref float offset, float height)
        {
            if (offset >= height)
            {
                offset -= height;
            }
        }
    }
}
