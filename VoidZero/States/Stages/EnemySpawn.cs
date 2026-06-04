using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Combat;
using VoidZero.Game.Entities;
using VoidZero.Game.Entities.Components;
using VoidZero.Game.Entities.Enemies;

namespace VoidZero.States.Stages
{
    /// <summary>
    /// Fluent builder that produces a <see cref="SpawnEnemyEvent"/> with much less boilerplate.
    ///
    /// Minimal usage — enemy slides in from the top-centre spawn ring cell and waits:
    ///   EnemySpawn.At(time: 0f)
    ///             .From(spawnCol: 3, spawnRow: 0)   // top ring, centre column
    ///             .MoveTo(targetCol: 3, targetRow: 2)
    ///             .TripleShot(BulletEnergy.Blue)
    ///
    /// With rotation:
    ///   EnemySpawn.At(5f)
    ///             .From(3, 0).MoveTo(3, 2)
    ///             .TripleShot(BulletEnergy.Purple)
    ///             .WithRotation(MathF.PI / 4f, duration: 1f)
    ///
    /// The exit direction is always the opposite of the entry direction (auto-derived).
    /// </summary>
    public class EnemySpawn
    {
        // ── state ────────────────────────────────────────────────────────────

        private float _time;
        private int _spawnCol, _spawnRow;
        private int _targetCol, _targetRow;
        private float _waitTime = 15f;
        private float _exitSpeed = 1500f;
        private float _exitDuration = 0.5f;
        private float _entrySpeed = 1500f;

        // rotation (optional)
        private bool _hasRotation;
        private float _rotationAngle;
        private float _rotationDelay;
        private float _rotationDuration;
        private bool _rotationLoop = true;

        // enemy kind
        private enum Kind {
            TripleShot,
            OmniShotRotate,
            OmniShot,
            CardinalSeperatorFull,
            CardinalSeperatorWeave,
            CardinalSequencer,
            CardinalSequencerFast,
            CardinalSpinner,
            CardinalSpinnerSpread,
            SpreadShot,
            SpreadShotFast
        }
        private Kind _kind;
        private BulletEnergy _energy;

        // ── entry point ──────────────────────────────────────────────────────

        public static EnemySpawn At(float time) => new EnemySpawn { _time = time };

        // ── grid placement ───────────────────────────────────────────────────

        /// <param name="spawnCol">Grid column (0 or 12 = spawn ring outside screen)</param>
        /// <param name="spawnRow">Grid row    (0 or 12 = spawn ring outside screen)</param>
        public EnemySpawn From(int spawnCol, int spawnRow)
        {
            _spawnCol = spawnCol;
            _spawnRow = spawnRow;
            return this;
        }

        /// <param name="targetCol">Grid column to move to once spawned (1–11 = play area)</param>
        /// <param name="targetRow">Grid row    to move to once spawned (1–11 = play area)</param>
        public EnemySpawn MoveTo(int targetCol, int targetRow)
        {
            _targetCol = targetCol;
            _targetRow = targetRow;
            return this;
        }

        // ── optional tweaks ──────────────────────────────────────────────────

        public EnemySpawn Wait(float seconds) { _waitTime = seconds; return this; }
        public EnemySpawn EntrySpeed(float speed) { _entrySpeed = speed; return this; }
        public EnemySpawn ExitSpeed(float speed) { _exitSpeed = speed; return this; }
        public EnemySpawn ExitDuration(float duration) { _exitDuration = duration; return this; }

        /// <summary>Adds a looping <see cref="RotationSequenceComponent"/> to the enemy.</summary>
        public EnemySpawn WithRotation(float angle, float duration, float delay = 0f, bool loop = true)
        {
            _hasRotation = true;
            _rotationAngle = angle;
            _rotationDuration = duration;
            _rotationDelay = delay;
            _rotationLoop = loop;
            return this;
        }

        // ── enemy kind ───────────────────────────────────────────────────────

        public SpawnEnemyEvent TripleShot(BulletEnergy energy)
        {
            _kind = Kind.TripleShot;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent SpreadShot(BulletEnergy energy)
        {
            _kind = Kind.TripleShot;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent SpreadShotFast(BulletEnergy energy)
        {
            _kind = Kind.TripleShot;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent OmniShot(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent OmniShotRotate(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSpinner(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSpinnerSpread(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSequencer(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSequencerFast(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSeperatorFull(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }

        public SpawnEnemyEvent CardinalSeperatorWeave(BulletEnergy energy)
        {
            _kind = Kind.OmniShotRotate;
            _energy = energy;
            return Build();
        }


        private SpawnEnemyEvent Build()
        {
            // Capture all values so the lambda is self-contained
            var spawnCol = _spawnCol;
            var spawnRow = _spawnRow;
            var targetCol = _targetCol;
            var targetRow = _targetRow;
            var waitTime = _waitTime;
            var entrySpeed = _entrySpeed;
            var exitSpeed = _exitSpeed;
            var exitDuration = _exitDuration;
            var kind = _kind;
            var energy = _energy;
            var hasRotation = _hasRotation;
            var rotAngle = _rotationAngle;
            var rotDelay = _rotationDelay;
            var rotDuration = _rotationDuration;
            var rotLoop = _rotationLoop;

            return new SpawnEnemyEvent(_time, state =>
            {
                var tex = GameServices.Instance.Content.GetTexture("witch");
                var bullets = state.Bullets;

                Entity enemy = kind switch
                {
                    Kind.TripleShot => new TripleShot(tex, Vector2.Zero, bullets, energy),
                    Kind.OmniShotRotate => new OmniShotRotate(tex, Vector2.Zero, bullets, energy),
                    Kind.OmniShot => new OmniShot(tex, Vector2.Zero, bullets, energy),
                    Kind.SpreadShot => new SpreadShot(tex, Vector2.Zero, bullets, energy),
                    Kind.SpreadShotFast => new SpreadShotFast(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSpinner => new CardinalSpinner(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSpinnerSpread => new CardinalSpinnerSpread(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSequencer => new CardinalSpinner(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSequencerFast => new CardinalSpinner(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSeperatorFull => new CardinalSeperatorFull(tex, Vector2.Zero, bullets, energy),
                    Kind.CardinalSeperatorWeave => new CardinalSeperatorWeave(tex, Vector2.Zero, bullets, energy),
                    _ => throw new InvalidOperationException("Unknown enemy kind")
                };

                enemy.SetPosition(StageGrid.CellPosition(spawnCol, spawnRow, enemy));

                Console.WriteLine($"[EnemySpawn] col={spawnCol} row={spawnRow} " +
                  $"norm=({StageGrid.GridNorm(spawnCol, spawnRow).X:F3}, {StageGrid.GridNorm(spawnRow, spawnRow).Y:F3}) " +
                  $"worldPos=({enemy.Position.X:F1}, {enemy.Position.Y:F1}) " +
                  $"worldSize=({GameServices.Instance.Settings.WorldWidth}, {GameServices.Instance.Settings.WorldHeight})");

                // derive entry direction from spawn -> target
                var spawnNorm = StageGrid.GridNorm(spawnCol, spawnRow);
                var targetNorm = StageGrid.GridNorm(targetCol, targetRow);

                var entryDir = Vector2.Normalize(targetNorm - spawnNorm);
                var exitDir = -entryDir;

                float w = GameServices.Instance.Settings.WorldWidth;
                float h = GameServices.Instance.Settings.WorldHeight;

                var spawnWorld = new Vector2(StageGrid.Norm(spawnCol) * w, StageGrid.Norm(spawnRow) * h);
                var targetWorld = new Vector2(StageGrid.Norm(targetCol) * w, StageGrid.Norm(targetRow) * h);
                float worldDist = Vector2.Distance(spawnWorld, targetWorld);
                float entryDuration = worldDist / entrySpeed;
                // Simpler alternative if you prefer to just hardcode: tweak via .EntrySpeed()

                enemy.AddComponent(new MovementLifecycleComponent(
                    entryMovement: new MovementTool(entryDir, entrySpeed, entryDuration),
                    waitTime: waitTime,
                    exitDirection: exitDir,
                    exitSpeed: exitSpeed,
                    exitDuration: exitDuration
                ));

                if (hasRotation)
                {
                    enemy.AddComponent(new RotationSequenceComponent(
                        loop: rotLoop,
                        new RotationComponent(rotAngle, rotDelay, rotDuration)
                    ));
                }

                return enemy;
            });
        }
    }
}