using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace VoidZero.Game.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 10000f; // low -> on ice
        private readonly float _deceleration = 3000f;
        private readonly float _maxSpeed = 550f;
        private ShooterComponent _shooter;
        public BulletEnergy ActiveShield { get; private set; } = BulletEnergy.Yellow;
        // Grazing
        private bool _isCurrentlyGrazing = false;
        private float _grazeAccumulatedTime = 0f;
        private float _currentDamageMultiplier = 1f;
        const float _maxGrazeDamageAfter = 1f;
        const float _grazeDecayTimer = 10f;
        const float MaxGrazeMultiplier = 3f;
        const float GrazeDecayRate = 1.5f;
        public float GrazeTimer { get; private set; } = 0f;
        public float DamageMultiplier => _currentDamageMultiplier;
        // Dashing
        private const float DashDistance = 200f;
        private const float DashCooldown = 0.5f;
        private const float DashDuration = 0.08f;
        private float _dashCooldownTimer = 0f;
        private float _dashTimer = 0f;
        private Vector2 _dashVelocity;
        private bool _isDashing = false;
        public bool IsInvulnerable { get; private set; }
        // Afterimages
        private const int MaxAfterImages = 5;
        private const float AfterImageSpawnRate = 0.015f;
        private const float AfterImageLifetime = 0.15f;
        private float _afterImageTimer = 0f;
        private readonly Queue<AfterImage> _afterImages = new();


        public Player(Texture2D texture, Vector2 startPosition, InputManager input, BulletManager bulletManager)
            : base(texture, startPosition, 16, 16)
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
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= dt;
            }

            Vector2 inputDirection = _input.MoveAxis;
            bool hasMovementInput = inputDirection.LengthSquared > 0;
            bool movementPriority = hasMovementInput ||  _input.ShootHeld;

            if (_input.DashPressed && !_isDashing && _dashCooldownTimer <= 0f)
            {
                TryStartDash();
            }
            ApplyAfterImages(dt);
            UpdateAfterImages(dt);
            ApplyMovement(inputDirection, dt, movementPriority);
            _shooter.TryShoot(this, dt, _input.ShootHeld);
            string animationKey = GetAnimationKey(hasMovementInput ? inputDirection : Velocity);
            UpdateAnimation(animationKey, dt);
            UpdateGraze(dt);

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
            // Debug
            Console.WriteLine($"GrazeMultiplier: {DamageMultiplier:0.00}");
        }

        public override void Draw(SpriteBatch batch)
        {
            foreach (AfterImage image in _afterImages)
            {
                float alpha = image.Lifetime / image.MaxLifetime;
                alpha = MathF.Sqrt(alpha);
                Vector4 tint = new Vector4(1f, 1f, 1f, alpha * 0.5f);

                Animations.Draw(batch, image.Position, Scale, tint);
            }

            base.Draw(batch);
        }

        public void AddGraze(float dt)
        {
            GrazeTimer += dt;
            if (GrazeTimer > _maxGrazeDamageAfter)
            {
                GrazeTimer = _maxGrazeDamageAfter;
            } 
        }

        public void UpdateGraze(float dt)
        {
            if (_isCurrentlyGrazing)
            {
                // Update multiplier based on accumulated graze time
                _grazeAccumulatedTime += dt;
                if (_grazeAccumulatedTime > _maxGrazeDamageAfter)
                {
                    _grazeAccumulatedTime = _maxGrazeDamageAfter;
                } 

                _currentDamageMultiplier = MathHelper.Lerp(1f, MaxGrazeMultiplier, _grazeAccumulatedTime / _maxGrazeDamageAfter);
            }
            else
            {
                // Not grazing -> decay multiplier over _grazeDecayTimer seconds
                float decayPerSecond = (MaxGrazeMultiplier - 1f) / _grazeDecayTimer;
                _currentDamageMultiplier -= decayPerSecond * dt;
                if (_currentDamageMultiplier < 1f)
                {
                    _currentDamageMultiplier = 1f;
                }

                // Decay graze accumulation too
                _grazeAccumulatedTime -= dt;
                if (_grazeAccumulatedTime < 0f)
                {
                    _grazeAccumulatedTime = 0f;
                }
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
            {
                _grazeAccumulatedTime = _maxGrazeDamageAfter;
            }
        }

        private string GetAnimationKey(Vector2 dir)
        {
            if (dir.LengthSquared == 0)
            {
                return "Idle";
            } 

            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
            {
                return dir.X > 0 ? "Right" : "Left";
            }
            else
            {
                return dir.Y > 0 ? "Down" : "Up";
            }
        }

        private void ApplyMovement(Vector2 input, float dt, bool movementPriority)
        {
            if (_isDashing)
            {
                Position += _dashVelocity * dt;
                _dashTimer -= dt;

                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    IsInvulnerable = false;
                    Velocity = Vector2.Zero; // no sliding after dash
                }

                return; // skip normal movement
            }

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
            {
                Velocity = Velocity.Normalized() * _maxSpeed;
            }

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

        private void TryStartDash()
        {
            Vector2 direction = _input.MoveAxis;

            if (direction.LengthSquared == 0)
            {
                return; // no direction, no dash
            }

            direction = direction.Normalized();

            _afterImageTimer = 0f;
            _isDashing = true;
            IsInvulnerable = true;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
            _dashVelocity = direction * (DashDistance / DashDuration);
        }

        private void ApplyAfterImages(float dt)
        {
            if (_isDashing)
            {
                _afterImageTimer -= dt;

                if (_afterImageTimer <= 0f)
                {
                    _afterImageTimer = AfterImageSpawnRate;

                    if (_afterImages.Count >= MaxAfterImages)
                        _afterImages.Dequeue();

                    _afterImages.Enqueue(new AfterImage
                    {
                        Position = Position,
                        Lifetime = AfterImageLifetime,
                        MaxLifetime = AfterImageLifetime
                    });
                }
            }
        }

        private void UpdateAfterImages(float dt)
        {
            int count = _afterImages.Count;

            for (int i = 0; i < count; i++)
            {
                AfterImage image = _afterImages.Dequeue();
                image.Lifetime -= dt;

                if (image.Lifetime > 0f)
                {
                    _afterImages.Enqueue(image);
                }
            }
        }
    }
}