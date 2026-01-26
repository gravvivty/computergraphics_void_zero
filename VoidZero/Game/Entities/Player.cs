using OpenTK.Mathematics;
using VoidZero.Graphics;
using VoidZero.Game.Input;
using System;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using VoidZero.Game.Entities.Tools;

namespace VoidZero.Game.Entities
{
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 10000f; // low -> on ice
        private readonly float _deceleration = 6000f;
        private readonly float _maxSpeed = 550f;
        private ShooterComponent _shooter;
        public BulletEnergy ActiveShield { get; private set; } = BulletEnergy.Red;
        // Grazing
        private bool _isCurrentlyGrazing = false;
        public float _maxGrazeDamageAfter { get; private set; } = 1f;
        const float _grazeDecayTimer = 10f;
        public float MaxGrazeMultiplier { get; private set; } = 3f;
        const float GrazeDecayRate = 1.5f;
        public float GrazeTimer { get; private set; } = 0f;
        private float _grazeBonus = 0f;
        public float DamageMultiplier => 1f + _grazeBonus;
        private const float GrazeGainPerSecond = 2f; // tuning knob
        private float MaxGrazeBonus => MaxGrazeMultiplier - 1f;
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
        // Damage flicker
        private float _damageFlashTimer = 0f;
        private const float DamageFlashDuration = 0.4f;
        private const float FlickerInterval = 0.06f;
        private float _flickerTimer = 0f;
        private bool _visibleThisFrame = true;
        // Health
        private const float HealthRegenDelay = 3f;
        private float _healthRegenTimer = 0f;
        public float HealthRegenTimer => _healthRegenTimer;

        private bool _isRegeneratingHealth = false;
        public bool IsCriticalHealth => CurrentHealth <= 1f;
        // Ability bar
        public float _abilityBar { get; private set; } = 0f; // current progress
        public float MaxAbilityBar { get; } = 100f; // full bar
        private const float AbilityFillRate = 4f; // per second when not shooting
        public float AbsorbFillAmount { get; } = 2f; // per bullet absorbed

        // Ability levels thresholds
        public float Level1Threshold { get; private set; } = 30f;
        public float Level2Threshold { get; private set; } = 60f;
        public float Level3Threshold { get; private set; } = 100f;

        // Active ability state
        private bool _abilityActive = false;
        private float _abilityTimer = 0f;
        private const float AbilityDuration = 10f;
        private int _currentAbilityLevel = 0; // 0 = none, 1 = level1, etc.
        private string _currentAnimationKey = "Idle";



        public Player(Texture2D texture, Vector2 startPosition, InputManager input, BulletManager bulletManager)
            : base(texture, startPosition, 16, 16)
        {
            MaxHealth = 3f;
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
            Animations.Play("Idle");
            AddDefaultDeathAnimation();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (IsDying)
            {
                Animations.Update(dt);

                // Optional: once animation finishes, mark dead or trigger game over
                if (Animations.IsFinished)
                {
                    IsDead = true;
                    // trigger game over
                }

                return; // skip all normal update logic
            }
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
            UpdateDamageFlicker(dt);
            ApplyAfterImages(dt);
            UpdateAfterImages(dt);
            ApplyMovement(inputDirection, dt, movementPriority);
            _shooter.TryShoot(this, dt, _input.ShootHeld);
            string animationKey = GetAnimationKey(hasMovementInput ? inputDirection : Velocity);

            // Only switch if animation changed
            if (animationKey != _currentAnimationKey)
            {
                Animations.Play(animationKey);
                _currentAnimationKey = animationKey;
            }

            // Always update current animation
            Animations.Update(dt);
            UpdateGraze(dt);
            UpdateHealthRegen(dt);

            if (_input.SwitchShieldPressed)
            {
                CycleShield();
            }

            if (_input.ActivateAbilityPressed && _abilityBar > 0f)
            {
                ActivateAbility();
            }

            // Ability bar fill when not shooting
            if (!_abilityActive && !_input.ShootHeld)
            {
                _abilityBar += AbilityFillRate * dt;
                if (_abilityBar > MaxAbilityBar) _abilityBar = MaxAbilityBar;
            }

            // Reset ability timer if active
            if (_abilityActive)
            {
                _abilityTimer -= dt;
                if (_abilityTimer <= 0f)
                {
                    _abilityActive = false;
                    _currentAbilityLevel = 0;
                    // Restore original bullet pattern
                    _shooter.SetPattern(new FixedDirectionPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY
                    ));
                }
            }
        }
        public void FillAbilityBar(float amount)
        {
            _abilityBar += amount;
            if (_abilityBar > MaxAbilityBar) _abilityBar = MaxAbilityBar;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (!_visibleThisFrame)
                return;

            Vector4 tint = Vector4.One;

            if (_damageFlashTimer > 0f)
            {
                tint = new Vector4(1f, 0.2f, 0.2f, 1f); // bright red
            }

            // Afterimages first
            foreach (AfterImage image in _afterImages)
            {
                float alpha = image.Lifetime / image.MaxLifetime;
                alpha = MathF.Sqrt(alpha);
                Vector4 afterTint = new Vector4(1f, 1f, 1f, alpha * 0.5f);
                Animations.Draw(batch, image.Position, Scale, afterTint);
            }

            Animations.Draw(batch, Position, Scale, tint);

            // Debug hitbox
            batch.DrawRectangle(Hitbox, Color.Red);
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
                _grazeBonus += GrazeGainPerSecond * dt;
            }
            else
            {
                float decayPerSecond = MaxGrazeBonus / _grazeDecayTimer;
                _grazeBonus -= decayPerSecond * dt;
            }

            _grazeBonus = Math.Clamp(_grazeBonus, 0f, MaxGrazeBonus);
            _isCurrentlyGrazing = false;
        }

        public void RegisterGraze()
        {
            if (IsInvulnerable)
            {
                return;
            }

            _isCurrentlyGrazing = true;
        }

        public void OnDamaged()
        {
            _damageFlashTimer = DamageFlashDuration;
            _flickerTimer = 0f;
            IsInvulnerable = true;
            // Reset health regen
            _healthRegenTimer = 0f;
            _isRegeneratingHealth = false;
            // Reset dmg mult and graze
            _grazeBonus = 0f;
        }

        public float HealthRegenProgress
        {
            get
            {
                if (CurrentHealth <= 1f)
                {
                    _healthRegenTimer = MathF.Min(_healthRegenTimer, HealthRegenDelay);
                    return Math.Clamp(_healthRegenTimer / HealthRegenDelay, 0f, 1f);
                }
                return 1f; // full color
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
                Velocity *= 0.929f; // high value -> drifting on ice
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
                BulletEnergy.Blue => BulletEnergy.Red,
                BulletEnergy.Red => BulletEnergy.Blue,
                _ => BulletEnergy.Red
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

        private void UpdateDamageFlicker(float dt)
        {
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= dt;
                _flickerTimer -= dt;

                if (_flickerTimer <= 0f)
                {
                    _flickerTimer = FlickerInterval;
                    _visibleThisFrame = !_visibleThisFrame;
                }

                if (_damageFlashTimer <= 0f)
                {
                    _damageFlashTimer = 0f;
                    _visibleThisFrame = true;
                    IsInvulnerable = false;
                }
            }
        }

        private void UpdateHealthRegen(float dt)
        {
            // Only regen if not at max
            if (CurrentHealth >= MaxHealth)
            {
                _healthRegenTimer = 0f;
                _isRegeneratingHealth = false;
                return;
            }

            _healthRegenTimer += dt;
            _isRegeneratingHealth = true;

            if (_healthRegenTimer >= HealthRegenDelay)
            {
                CurrentHealth += 1f;
                if (CurrentHealth > MaxHealth)
                    CurrentHealth = MaxHealth;

                _healthRegenTimer = 0f;

                // If still not full, keep chaining regen
                _isRegeneratingHealth = CurrentHealth < MaxHealth;
            }
        }

        private void ActivateAbility()
        {
            // Determine level
            if (_abilityBar >= Level3Threshold)
                _currentAbilityLevel = 3;
            else if (_abilityBar >= Level2Threshold)
                _currentAbilityLevel = 2;
            else if (_abilityBar >= Level1Threshold)
                _currentAbilityLevel = 1;
            else
                return; // not enough to activate

            _abilityActive = true;
            _abilityTimer = AbilityDuration;

            // Consume bar
            _abilityBar = 0f;

            // Assign new pattern based on level
            switch (_currentAbilityLevel)
            {
                case 1:
                    _shooter.SetPattern(new SpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        3, 30f, 1500f
                    ));
                    break;
                case 2:
                    _shooter.SetPattern(new SpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        6, 25f, 1800f
                    ));
                    break;
                case 3:
                    _shooter.SetPattern(new SpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        10, 15f, 2000f
                    ));
                    break;
            }
        }
    }
}