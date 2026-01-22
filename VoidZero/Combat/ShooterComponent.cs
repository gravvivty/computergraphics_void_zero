using VoidZero.Game.Combat.Patterns;
using VoidZero.Game.Entities;

namespace VoidZero.Game.Combat
{
    public class ShooterComponent
    {
        private IBulletPattern _pattern;
        private readonly BulletManager _bulletManager;
        private readonly BulletOwner _owner;

        private readonly float _damage;
        private float _cooldown;
        private float _timer;
        private BulletEnergy _bulletEnergy = BulletEnergy.Green;

        public ShooterComponent(
            IBulletPattern pattern,
            BulletManager manager,
            BulletOwner owner,
            float cooldown,
            float damage)
        {
            _pattern = pattern;
            _bulletManager = manager;
            _owner = owner;
            _cooldown = cooldown;
            _damage = damage;
        }

        public BulletEnergy BulletEnergy
        {
            get => _bulletEnergy;
            set => _bulletEnergy = value;
        }

        public void SetPattern(IBulletPattern newPattern)
        {
            _pattern = newPattern;
        }

        public void TryShoot(Entity entity, float dt, bool trigger)
        {
            _timer -= dt;
            // Clamp to zero to avoid bursts if dt spikes
            if (_timer < 0f) _timer = 0f;

            if (!trigger || _timer > 0f)
                return;

            _timer = _cooldown;
            _pattern.Shoot(entity, _bulletManager, _owner, _damage, BulletEnergy);
        }
    }
}
