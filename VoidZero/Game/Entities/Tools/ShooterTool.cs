using VoidZero.Game.Combat;
using VoidZero.Game.Combat.Patterns;

namespace VoidZero.Game.Entities.Tools
{
    public class ShooterComponent
    {
        public IBulletPattern Pattern { get; private set; }
        public BulletEnergy BulletEnergy { get; set; }
        private readonly BulletManager _bulletManager;
        private readonly BulletOwner _owner;

        private float _damage;
        private float _cooldown;
        private float _timer;

        public ShooterComponent(
            IBulletPattern pattern,
            BulletManager manager,
            BulletOwner owner,
            float cooldown,
            float damage)
        {
            Pattern = pattern;
            _bulletManager = manager;
            _owner = owner;
            _cooldown = cooldown;
            _damage = damage;
        }

        public void SetPattern(IBulletPattern newPattern)
        {
            Pattern = newPattern;
        }

        public void Configure(
            IBulletPattern pattern,
            float cooldown,
            float damage)
        {
            Pattern = pattern;
            _cooldown = cooldown;
            _damage = damage;
            _timer = 0f;
        }

        public void TryShoot(Entity entity, float dt, bool trigger)
        {
            _timer -= dt;
            // Clamp to zero to avoid bursts if dt spikes
            if (_timer < 0f)
            {
                _timer = 0f;
            }

            if (!trigger || _timer > 0f)
            {
                return;
            }

            _timer = _cooldown;

            float finalDamage = _damage;

            if (entity is Player player)
            {
                finalDamage *= player.DamageMultiplier;
            }

            Pattern.Shoot(entity, _bulletManager, _owner, finalDamage, BulletEnergy);
        }
    }
}
