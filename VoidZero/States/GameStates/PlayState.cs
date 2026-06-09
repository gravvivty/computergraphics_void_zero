using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using VoidZero.Core;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States.Stages;
using VoidZero.UI;
using VoidZero.Utils;
using VoidZero.States.Graph;
using static VoidZero.Core.GameManager;

namespace VoidZero.States.GameStates
{
    public class PlayState : GameState
    {
        private readonly InputManager _input;
        public GameWindow Window { get; private set; }
        private readonly GameStateManager _gameStateManager;
        private readonly GameManager _gameManager;
        private StageComposer _stageComposer;
        public BulletManager Bullets { get; } = new();

        public Background _background { get; }
        public List<Entity> Entities { get; } = new();

        public Player _player { get; private set; }
        private Shield _playerShield;

        private bool _isDying = false;
        private float _deathTimer = 0f;
        private const float DeathAnimDuration = 1f;
        private const float BulletSlowFactor = 0.6f;

        public StageStats Stats { get; } = new();
        private bool _stageCompleted;
        private bool _focusVictoryNextFrame;

        private float _rainbowTimer = 0f;

        // ── World graph ───────────────────────────────────────────────────────
        private readonly WorldGraph _worldGraph;
        private readonly WorldNode _selectedNode;

        // Legacy: only used when running outside of the graph flow
        public int StageIndex { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Graph-driven constructor. Stage definition comes from the selected node.
        /// </summary>
        public PlayState(
            GameStateManager gsm,
            GameWindow window,
            InputManager input,
            Background bg,
            GameManager gm,
            WorldGraph worldGraph,
            WorldNode selectedNode)
        {
            _gameStateManager = gsm;
            Window = window;
            _input = input;
            _background = bg;
            _gameManager = gm;
            _worldGraph = worldGraph;
            _selectedNode = selectedNode;

            IStageDefinition stage = selectedNode.StageDefinition ?? new Stage1();
            Init(stage);
        }

        /// <summary>
        /// Legacy constructor — keeps the old stage-select menu working unchanged.
        /// </summary>
        public PlayState(
            GameStateManager gsm,
            GameWindow window,
            InputManager input,
            Background bg,
            GameManager gm,
            int stageIndex = 1)
        {
            _gameStateManager = gsm;
            Window = window;
            _input = input;
            _background = bg;
            _gameManager = gm;
            StageIndex = stageIndex;

            IStageDefinition stage = stageIndex switch
            {
                1 => new Stage1(),
                2 => new Stage2(),
                3 => new Stage3(),
                _ => new Stage1()
            };

            Init(stage);
        }

        /// <summary>
        /// Shared setup called by both constructors after the stage is resolved.
        /// </summary>
        private void Init(IStageDefinition stage)
        {
            _stageComposer = stage.Build();

            Texture2D playerTexture = GameServices.Instance.Content.GetTexture("player");
            _player = new Player(playerTexture, new Vector2(1230, 1250), _input, Bullets);
            _player.SetPosition(_player.Position);

            Texture2D shieldTexture = GameServices.Instance.Content.GetTexture("shield");
            _playerShield = new Shield(shieldTexture, _player);

            Entities.Add(_player);
            Entities.Add(_playerShield);
            _isDying = false;
        }

        // ── Update ────────────────────────────────────────────────────────────

        public override void Update(float dt)
        {
            _rainbowTimer += dt;

            if (!_isDying && _input.ConsumePausePressed())
            {
                _gameManager.EnterPause();
                _gameStateManager.ChangeState(
                    new PauseState(_gameStateManager, Window, _input, this, _gameManager)
                );
                return;
            }

            if (_gameManager.CurrentMode == GameMode.Paused)
                return;

            float bulletDt = _isDying ? dt * BulletSlowFactor : dt;
            Bullets.Update(bulletDt);
            GameServices.Instance.ParticleSystem.Update(dt);

            if (!_isDying)
            {
                HandleBulletHits(dt);
            }

            if (_isDying)
            {
                _deathTimer += dt;

                if (_deathTimer >= DeathAnimDuration)
                {
                    _gameStateManager.ChangeState(
                        new DeathState(_gameStateManager, Window, _gameManager)
                    );
                }

                foreach (var entity in Entities.ToList())
                    entity.Update(dt);

                return;
            }

            _stageComposer.Update(dt, this);

            foreach (var entity in Entities.ToList())
            {
                entity.Update(dt);

                bool remove =
                    entity.IsDead ||
                    entity.Components.OfType<MovementLifecycleComponent>().Any(c => c.IsExpired);

                if (remove)
                    Entities.Remove(entity);
            }

            Stats.Update(dt);
        }

        // ── Draw ──────────────────────────────────────────────────────────────

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Entity entity in Entities)
                entity.Draw(spriteBatch);

            Bullets.Draw(spriteBatch);
        }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            if (_stageCompleted)
                DrawStageCompleteUI();
        }

        // ── Collision ─────────────────────────────────────────────────────────

        private void HandleBulletHits(float dt)
        {
            for (int i = Bullets.Bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = Bullets.Bullets[i];

                if (bullet.Owner == BulletOwner.Player)
                {
                    foreach (Entity entity in Entities)
                    {
                        if (entity is not Enemy enemy) continue;
                        if (enemy.IsDying) continue;
                        if (bullet.HitEntities.Contains(enemy)) continue;
                        if (!bullet.Hitbox.IntersectsWith(entity.Hitbox)) continue;

                        float healthBefore = entity.CurrentHealth;
                        entity.CurrentHealth -= bullet.Damage;
                        float healthAfter = entity.CurrentHealth;

                        Console.WriteLine($"[HIT] {enemy.GetType().Name} | Damage: {bullet.Damage} | HP: {healthBefore} -> {healthAfter}");

                        bullet.HitEntities.Add(enemy);

                        Vector2 impactDir = bullet.Velocity.Normalized();
                        GameServices.Instance.ParticleSystem.SpawnSparks(enemy.VisualCenter, impactDir, 25);

                        Bullets.Bullets.RemoveAt(i);

                        if (entity.CurrentHealth <= 0)
                        {
                            entity.Kill();
                            _player.IncreaseScore(entity.Score);
                            Stats.RegisterKill(entity);

                            if (entity.IsBoss)
                                OnBossKilled();
                        }

                        Vector2 spawnPos = enemy.Position + new Vector2(
                            Random.Shared.NextSingle() * 100f,
                            Random.Shared.NextSingle() * 100f
                        );

                        GameServices.Instance.DamageNumbers.Spawn(spawnPos, bullet.Damage);
                        break;
                    }
                }
                else if (bullet.Owner == BulletOwner.Enemy)
                {
                    bool shieldAbsorbs =
                        bullet.Energy == _player.ActiveShield &&
                        bullet.Energy != BulletEnergy.Purple;

                    bool damageHit = _player.Hitbox.IntersectsWith(bullet.Hitbox);
                    bool grazeHit = bullet.GrazeHitbox.IntersectsWith(_player.Hitbox) && !damageHit && !shieldAbsorbs;

                    if (grazeHit)
                        _player.RegisterGraze();

                    if (damageHit)
                    {
                        if (_player.IsInvulnerable)
                        {
                            Bullets.Bullets.RemoveAt(i);
                            continue;
                        }

                        if (shieldAbsorbs)
                        {
                            _playerShield.Flash();
                            _player.FillAbilityBar(_player.AbsorbFillAmount);
                        }
                        else
                        {
                            _player.CurrentHealth -= bullet.Damage;
                            Stats.RegisterHit();

                            if (_player.CurrentHealth <= 0)
                            {
                                _player.CurrentHealth = 0;
                                _player.Kill();
                                OnPlayerDied();
                            }
                            else
                            {
                                _player.OnDamaged();
                                _gameManager.Shake(0.25f, 30f);
                            }
                        }

                        Bullets.Bullets.RemoveAt(i);
                    }
                }
            }
        }

        // ── Stage events ──────────────────────────────────────────────────────

        private void OnPlayerDied()
        {
            if (_isDying) return;

            _isDying = true;
            _playerShield.Kill();
            _gameManager.EnterDeath();
        }

        private void OnBossKilled()
        {
            Stats.Complete();

            // Mark the completed node and unlock its children for the next map visit
            _selectedNode?.Complete();

            if (_worldGraph != null)
            {
                // Graph run: go to map so the player picks their next node
                _gameStateManager.ChangeState(new MapState(
                    _gameStateManager,
                    Window,
                    _input,
                    _background,
                    _gameManager,
                    _worldGraph,
                    justCompleted: _selectedNode));

                return;
            }

            // Legacy run (no graph): show the old in-place victory screen
            /* _stageCompleted = true;
            _focusVictoryNextFrame = true;
            StageHighScores.Instance.Submit(StageIndex, Stats);
            */
        }

        // ── Victory UI (legacy / non-graph path only) ─────────────────────────

        private void DrawStageCompleteUI()
        {
            var io = ImGui.GetIO();

            ImGui.SetNextWindowPos(
                new System.Numerics.Vector2(
                    io.DisplaySize.X * 0.5f - 250,
                    io.DisplaySize.Y * 0.35f));

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 350));

            ImGui.Begin(
                "Stage Complete",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground
            );

            ImGuiHelpers.BeginCenteredBlock(350);

            ImGuiHelpers.CenterColoredText("STAGE COMPLETE", new Vector4(0.2f, 1f, 0.2f, 1f));

            ImGui.Spacing();
            ImGui.Spacing();

            ImGuiHelpers.CenterText($"Time:            {TimeSpan.FromSeconds(Stats.CompletionTime):mm\\:ss}");
            ImGuiHelpers.CenterText($"Enemies Killed:  {Stats.EnemiesKilled}");
            ImGuiHelpers.CenterText($"Hits Taken:      {Stats.HitsTaken}");

            if (Stats.HitsTaken == 0)
            {
                ImGui.Spacing();
                ImGuiHelpers.CenterRainbowText("FLAWLESS!!!", _rainbowTimer);
            }

            ImGui.Spacing();

            ImGuiHelpers.CenterColoredText($"Time Bonus:   x{Stats.TimeMultiplier:F2}", new Vector4(1f, 0.85f, 0.2f, 1f));
            ImGuiHelpers.CenterColoredText($"Kill Bonus:   x{Stats.KillMultiplier:F2}", new Vector4(1f, 0.85f, 0.2f, 1f));
            ImGuiHelpers.CenterColoredText($"Hit Bonus:    x{Stats.HitMultiplier:F2}", new Vector4(1f, 0.85f, 0.2f, 1f));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGuiHelpers.CenterColoredText($"FINAL SCORE: {Stats.FinalScore:N0}", new Vector4(0.2f, 1f, 0.2f, 1f));

            ImGui.Spacing();
            ImGui.Spacing();

            ImGuiHelpers.CenterNextItem(180);

            if (ImGui.Button("Back To Main Menu", new System.Numerics.Vector2(180, 60)))
            {
                _gameManager.EnterMenu();
                _gameStateManager.ChangeState(
                    new MenuState(_gameStateManager, Window, _input, _background, _gameManager));
            }

            if (_focusVictoryNextFrame)
            {
                ImGui.SetItemDefaultFocus();
                _focusVictoryNextFrame = false;
            }

            ImGui.End();
        }
    }
}