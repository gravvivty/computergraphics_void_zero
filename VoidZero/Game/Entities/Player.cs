using OpenTK.Mathematics;
using System.Drawing;
using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States.Stages;

namespace VoidZero.Game.Entities
{
    // Player class
    public class Player : Entity
    {
        private readonly InputManager _input;

        private readonly float _acceleration = 10000f; // Low -> On ice
        private readonly float _deceleration = 6000f;
        private readonly float _maxSpeed = 650f;
        private ShooterComponent _shooter;
        public int currentScore { get; private set; } = 0;
        public BulletEnergy ActiveShield { get; private set; } = BulletEnergy.Yellow;
        // Grazing
        private bool _isCurrentlyGrazing = false;
        public float _maxGrazeDamageAfter { get; private set; } = 1f;
        const float _grazeDecayTimer = 10f;
        public float MaxGrazeMultiplier { get; private set; } = 3f;
        public float GrazeTimer { get; private set; } = 0f;
        private float _grazeBonus = 0f;
        public float DamageMultiplier => 1f + _grazeBonus;
        private const float GrazeGainPerSecond = 2f;
        private float MaxGrazeBonus => MaxGrazeMultiplier - 1f;
        // Dashing
        private const float DashDistance = 300f;
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
        private const float HealthRegenDelay = 2f;
        private float _healthRegenTimer = 0f;

        private bool _isRegeneratingHealth = false;
        // Ability bar
        public float _abilityBar { get; private set; } = 0f; // current progress
        public float MaxAbilityBar { get; } = 100f; // full bar
        private const float AbilityFillRate = 4f; // per second when not shooting
        public float AbsorbFillAmount { get; } = 2f; // per bullet absorbed

        // Ability levels thresholds
        public float Level1Threshold { get; private set; } = 25f;
        public float Level2Threshold { get; private set; } = 70f;
        public float Level3Threshold { get; private set; } = 95f;

        // Active ability state
        private bool _abilityActive = false;
        private float _abilityTimer = 0f;
        private const float AbilityDuration = 10f;
        private int _currentAbilityLevel = 0; // 0 = none, 1 = level1, etc.
        private string _currentAnimationKey = "Idle";



        public Player(Texture2D texture, Vector2 startPosition, InputManager input, BulletManager bulletManager)
            : base(texture, startPosition, 32, 32)
        {
            MaxHealth = 3f;
            CurrentHealth = MaxHealth;
            _input = input;

            var bulletTex = GameServices.Instance.Content.GetTexture("VanillaBullet");
            _shooter = new ShooterComponent(
                new FixedDirectionPattern(bulletTex, -Vector2.UnitY, 2500),
                bulletManager,
                BulletOwner.Player,
                0.125f,
                20f
            );
            _shooter.BulletEnergy = BulletEnergy.Neutral;
            int spriteWidth = 32;
            int spriteHeight = spriteWidth;

            // Apply for every entity
            this.Scale = 3f;
            Width = spriteWidth * Scale;
            Height = spriteHeight * Scale;

            Animations.Add("Idle", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 0));
            Animations.Add("IdleShoot", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 1));

            Animations.Add("Left", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 2));
            Animations.Add("LeftShoot", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 3));

            Animations.Add("Up", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 4));
            Animations.Add("UpShoot", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 5));

            Animations.Add("Right", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 6));
            Animations.Add("RightShoot", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 7));

            Animations.Add("Down", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 8));
            Animations.Add("DownShoot", new Animation(texture, spriteWidth, spriteWidth, 3, 0.1f, 9));


            Animations.Play("Idle");
            AddDefaultDeathAnimation();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (IsDying)
            {
                Animations.Update(dt);

                if (Animations.IsFinished)
                {
                    IsDead = true;
                    // Game over
                }

                return; // Skip all usually update logic after death
            }
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= dt;
            }

            Vector2 inputDirection = _input.MoveAxis;
            bool hasMovementInput = inputDirection.LengthSquared > 0;
            bool movementPriority = hasMovementInput || _input.ShootHeld;

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
                tint = new Vector4(1f, 0.2f, 0.2f, 1f); // Bright red
            }

            // Afterimages first
            foreach (AfterImage image in _afterImages)
            {
                float alpha = image.Lifetime / image.MaxLifetime;
                alpha = MathF.Sqrt(alpha);
                Vector4 afterTint = new Vector4(1f, 1f, 1f, alpha * 0.5f);
                Animations.Draw(batch, image.Position, Scale, afterTint);
            }

            DrawGrazingBar(batch);
            DrawHealthBar(batch);
            DrawAbilityBar(batch);
            Animations.Draw(batch, Position, Scale, tint);

            // Debug hitbox
            if (GameServices.Instance.Settings.ShowHitboxes)
            {
                batch.DrawRectangle(Hitbox, Color.Red);
            }
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
            // Reset damage multiplier and graze
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
                return 1f; // Full color
            }
        }

        private string GetAnimationKey(Vector2 movementDir)
        {
            bool isMoving = movementDir.LengthSquared > 0;
            bool shooting = _input.ShootHeld;

            if (!isMoving)
            {
                return shooting ? "IdleShoot" : "Idle";
            }

            if (Math.Abs(movementDir.X) > Math.Abs(movementDir.Y))
            {
                if (movementDir.X > 0)
                    return shooting ? "RightShoot" : "Right";
                else
                    return shooting ? "LeftShoot" : "Left";
            }
            else
            {
                if (movementDir.Y > 0)
                    return shooting ? "DownShoot" : "Down";
                else
                    return shooting ? "UpShoot" : "Up";
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
                    Velocity = Vector2.Zero; // No sliding after dash
                }

                // Ensure player stays within world bounds while dashing
                ClampToWorldBounds();

                return; // Skip usual movement
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
                Velocity *= 0.659f; // High value -> drifting on ice
            }

            if (Velocity.Length > _maxSpeed)
            {
                Velocity = Velocity.Normalized() * _maxSpeed;
            }

            Position += Velocity * dt;

            // Clamp position to world bounds so the player cannot leave the playable area
            ClampToWorldBounds();
        }

        // Prevent the player from moving outside the game world defined in settings
        private void ClampToWorldBounds()
        {
            var settings = GameServices.Instance.Settings;

            float minX = 0f;
            float minY = 0f;
            float maxX = settings.WorldWidth - Width;
            float maxY = settings.WorldHeight - Height;

            if (maxX < minX) maxX = minX;
            if (maxY < minY) maxY = minY;

            Position = new Vector2(
                Math.Clamp(Position.X, minX, maxX),
                Math.Clamp(Position.Y, minY, maxY)
            );
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
                return; // No direction -> no dash consumed
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
            // Only regenerate if not at max health
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
                {
                    CurrentHealth = MaxHealth;
                }

                _healthRegenTimer = 0f;

                // If still not full, keep chaining regeneration
                _isRegeneratingHealth = CurrentHealth < MaxHealth;
            }
        }

        private void ActivateAbility()
        {
            // Determine level
            if (_abilityBar >= Level3Threshold)
            {
                _currentAbilityLevel = 3;
            }
            else if (_abilityBar >= Level2Threshold)
            {
                _currentAbilityLevel = 2;
            }
            else if (_abilityBar >= Level1Threshold)
            {
                _currentAbilityLevel = 1;
            }
            else
            {
                return; // Not enough to activate
            }

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
                        3, 25f, 1500f
                    ));
                    break;
                case 2:
                    _shooter.SetPattern(new SpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        5, 15f, 1800f
                    ));
                    break;
                case 3:
                    _shooter.SetPattern(new SpreadPattern(
                        GameServices.Instance.Content.GetTexture("VanillaBullet"),
                        -Vector2.UnitY,
                        10, 2f, 2000f
                    ));
                    break;
            }
        }

        public void DrawHealthBar(SpriteBatch spriteBatch)
        {
            Texture2D barTex = GameServices.Instance.Content.GetTexture("healthbar");
            Texture2D life1Tex = GameServices.Instance.Content.GetTexture("life1");
            Texture2D life2Tex = GameServices.Instance.Content.GetTexture("life2");
            Texture2D life3Tex = GameServices.Instance.Content.GetTexture("life3");

            const float Scale = 3f;

            float barW = barTex.Width * Scale;
            float barH = barTex.Height * Scale;
            Vector2 size = new Vector2(barW, barH);
            Vector2 pos = new Vector2(Position.X - Texture.Width / 8, Position.Y + Texture.Height / 1.2f);

            // Empty base frame
            spriteBatch.Draw(barTex, pos, size, Vector4.One);

            // Overlay the sprite that matches current HP
            int hp = (int)MathF.Round(CurrentHealth);
            Texture2D lifeTex = hp switch
            {
                1 => life1Tex,
                2 => life2Tex,
                _ => life3Tex   // 3 or full
            };

            // Only draw the life overlay when the player actually has health
            if (hp > 0)
            {
                spriteBatch.Draw(lifeTex, pos, size, Vector4.One);
            }
        }

        public void DrawGrazingBar(SpriteBatch spriteBatch)
        {
            Texture2D barTex = GameServices.Instance.Content.GetTexture("graze");
            Texture2D fillTex = GameServices.Instance.Content.GetTexture("graze_fill");

            const float BarScale = 2f;
            const float XOffset = -8f;

            float barW = barTex.Width * BarScale;
            float barH = barTex.Height * BarScale;

            Vector2 pos = new Vector2(
                Position.X - barW + XOffset,
                Position.Y + (Height - barH) / 2f
            );

            Vector2 size = new Vector2(barW, barH);

            // Draw background frame
            spriteBatch.Draw(barTex, pos, size, Vector4.One);

            // Calculate fill fraction
            float grazeBonus = DamageMultiplier - 1f;
            float maxBonus = MaxGrazeMultiplier - 1f;
            float fill = Math.Clamp(grazeBonus / maxBonus, 0f, 1f);

            if (fill > 0f)
            {
                int capPx = 8;  // Dead pixels at top and bottom

                // In UV space, the active region runs from capTop to capBottom
                float capTop = capPx / (float)fillTex.Height;
                float capBottom = 1f - capPx / (float)fillTex.Height;

                // Fill upward from capBottom
                float activeRange = capBottom - capTop; // The clippable UV span
                float v0 = capBottom - (activeRange * fill); // Top of clipped region
                float v1 = capBottom; // Always anchor to bottom cap

                // Destination height matches the clipped UV region
                float activeBarH = (fillTex.Height - capPx * 2) * BarScale;
                float destH = activeBarH * fill;
                float destY = (pos.Y + capPx * BarScale) + (activeBarH - destH); // Anchored above bottom cap

                spriteBatch.Draw(
                    fillTex,
                    new Vector2(pos.X, destY),
                    new Vector2(barW, destH),
                    new Vector4(0f, v0, 1f, v1),
                    Vector4.One
                );
            }
        }

        public void DrawAbilityBar(SpriteBatch spriteBatch)
        {
            Texture2D barTex = GameServices.Instance.Content.GetTexture("ability");
            Texture2D fillTex = GameServices.Instance.Content.GetTexture("ability_fill");

            const float BarScale = 2f;
            const float XOffset = 8f;  // gap to the right of the sprite

            float barW = barTex.Width * BarScale;
            float barH = barTex.Height * BarScale;

            Vector2 pos = new Vector2(
                Position.X + Width + XOffset,
                Position.Y + (Height - barH) / 2f
            );

            // Draw background frame
            spriteBatch.Draw(barTex, pos, new Vector2(barW, barH), Vector4.One);

            int capPx = 8;
            float capTop = capPx / (float)fillTex.Height;
            float capBottom = 1f - capPx / (float)fillTex.Height;
            float activeRange = capBottom - capTop;
            float activeBarH = (fillTex.Height - capPx * 2) * BarScale;

            // Gap settings — 3px gaps at level thresholds, drawn as dark slices over the fill
            const float GapPx = 3f;
            float[] thresholds = { Level1Threshold / MaxAbilityBar,
                                  Level2Threshold / MaxAbilityBar };  // level3 is the top, no gap needed

            float fill = Math.Clamp(_abilityBar / MaxAbilityBar, 0f, 1f);

            if (fill > 0f)
            {
                float v0 = capBottom - (activeRange * fill);
                float v1 = capBottom;
                float destH = activeBarH * fill;
                float destY = (pos.Y + capPx * BarScale) + (activeBarH - destH);

                spriteBatch.Draw(
                    fillTex,
                    new Vector2(pos.X, destY),
                    new Vector2(barW, destH),
                    new Vector4(0f, v0, 1f, v1),
                    Vector4.One
                );
            }

            // Draw gaps over the fill at each threshold
            // Each gap is a filled transparent rectangle sliced across the bar
            foreach (float t in thresholds)
            {
                float gapCentreY = (pos.Y + capPx * BarScale) + activeBarH * (1f - t);

                spriteBatch.DrawRectangle(
                    new RectangleF(pos.X, gapCentreY - GapPx / 2f, barW, GapPx),
                    Color.Transparent,
                    filled: true
                );
            }
        }

        public void IncreaseScore(int score)
        {
            currentScore += score;
            Console.WriteLine($"[SCORE] previousScore={currentScore - score} | enemyScore={score} | newScore={currentScore}");
        }
    }
}