using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;

namespace VoidZero.Game.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 10000f; // low -> on ice
        private readonly float _deceleration = 6000f;
        private readonly float _maxSpeed = 750f;
        private ShooterComponent _shooter;
        private bool _isCurrentlyGrazing = false;
        private float _grazeAccumulatedTime = 0f;
        private float _currentDamageMultiplier = 1f;
        const float _maxGrazeDamageAfter = 1f;
        const float _grazeDecayTimer = 10f;
        const float MaxGrazeMultiplier = 3f;
        const float GrazeDecayRate = 1.5f;
        public float GrazeTimer { get; private set; } = 0f;
        public float DamageMultiplier => _currentDamageMultiplier;

        public BulletEnergy ActiveShield { get; private set; } = BulletEnergy.Yellow;


        public Player(Texture2D texture, Vector2 startPos, InputManager input, BulletManager bulletManager)
            : base(texture, startPos, 16, 16)
        {
            MaxHealth = 100f;
            CurrentHealth = MaxHealth;
            _input = input;

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new FixedDirectionPattern(bulletTex, -Vector2.UnitY),
                bulletManager,
                BulletOwner.Player,
                0.125f,
                10f
            );
            _shooter.BulletEnergy = BulletEnergy.Neutral;

            // Do this for every entity
            this.Scale = 4f;
            Width = 16 * Scale;
            Height = 16 * Scale;

            Animations.Add("Up", new Animation(texture, 16, 16, 3, 0.1f, 0));
            Animations.Add("Right", new Animation(texture, 16, 16, 3, 0.1f, 2));
            Animations.Add("Down", new Animation(texture, 16, 16, 3, 0.1f, 4));
            Animations.Add("Left", new Animation(texture, 16, 16, 3, 0.1f, 6));

            Animations.Add("Idle", new Animation(texture, 16, 16, 1, 1f, 0));
        }

        public override void Update(float dt)
        {
            Vector2 inputDir = _input.MoveAxis;

            bool hasMovementInput = inputDir.LengthSquared > 0;
            bool movementPriority =
                hasMovementInput ||
                _input.ShootHeld;

            ApplyMovement(inputDir, dt, movementPriority);

            _shooter.TryShoot(this, dt, _input.ShootHeld);

            UpdateAnimation(GetAnimationKey(
                hasMovementInput ? inputDir : Velocity
            ), dt);

            if (_input.SwitchShieldPressed)
            {
                CycleShield();
            }

            if (_input.SwitchPatternPressed)
            {
                _shooter.SetPattern(
                    new ThreeWaySpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        40f
                    )
                );
            }

            UpdateGraze(dt);
            // Debug
            Console.WriteLine($"GrazeMultiplier: {DamageMultiplier:0.00}");
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
        }

        public void AddGraze(float dt)
        {
            GrazeTimer += dt;
            if (GrazeTimer > _maxGrazeDamageAfter)
                GrazeTimer = _maxGrazeDamageAfter;
        }

        public void UpdateGraze(float dt)
        {
            if (_isCurrentlyGrazing)
            {
                // Update multiplier based on accumulated graze time
                _grazeAccumulatedTime += dt;
                if (_grazeAccumulatedTime > _maxGrazeDamageAfter)
                    _grazeAccumulatedTime = _maxGrazeDamageAfter;

                _currentDamageMultiplier = MathHelper.Lerp(1f, MaxGrazeMultiplier, _grazeAccumulatedTime / _maxGrazeDamageAfter);
            }
            else
            {
                // Not grazing -> decay multiplier over _grazeDecayTimer seconds
                float decayPerSecond = (MaxGrazeMultiplier - 1f) / _grazeDecayTimer;
                _currentDamageMultiplier -= decayPerSecond * dt;
                if (_currentDamageMultiplier < 1f)
                    _currentDamageMultiplier = 1f;

                // Decay graze accumulation too
                _grazeAccumulatedTime -= dt;
                if (_grazeAccumulatedTime < 0f)
                    _grazeAccumulatedTime = 0f;
            }

            // Reset grazing flag for next frame
            _isCurrentlyGrazing = false;
        }

        public void RegisterGraze(float dt)
        {
            _isCurrentlyGrazing = true;

            // Increase graze accumulation
            _grazeAccumulatedTime += dt;
            if (_grazeAccumulatedTime > _maxGrazeDamageAfter)
                _grazeAccumulatedTime = _maxGrazeDamageAfter;
        }

        private string GetAnimationKey(Vector2 dir)
        {
            if (dir.LengthSquared == 0)
                return "Idle";

            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                return dir.X > 0 ? "Right" : "Left";
            else
                return dir.Y > 0 ? "Down" : "Up";
        }

        private void ApplyMovement(Vector2 input, float dt, bool movementPriority)
        {
            if (input.LengthSquared > 0)
            {
                Velocity += input * _acceleration * dt;
            }
            else if (!movementPriority)
            {
                // Full deceleration
                float speed = Velocity.Length;
                if (speed > 0)
                {
                    float drop = _deceleration * dt;
                    Velocity *= MathF.Max(speed - drop, 0) / speed;
                }
            }
            else
            {
                Velocity *= 0.959f; // high value -> drifting on ice
            }

            if (Velocity.Length > _maxSpeed)
                Velocity = Velocity.Normalized() * _maxSpeed;

            Position += Velocity * dt;
        }

        private void CycleShield()
        {
            ActiveShield = ActiveShield switch
            {
                BulletEnergy.Blue => BulletEnergy.Yellow,
                BulletEnergy.Yellow => BulletEnergy.Blue,
                _ => BulletEnergy.Yellow
            };
        }
    }
}