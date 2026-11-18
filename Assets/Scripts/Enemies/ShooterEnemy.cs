using UnityEngine;

namespace Emberfall.Enemies
{
    public enum ShooterState { Idle, Aim, Shoot, Cooldown }

    public class ShooterEnemy : EnemyBase
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;

        public ShooterState State { get; private set; } = ShooterState.Idle;
        private float _timer;

        protected override void Awake()
        {
            base.Awake();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Update()
        {
            switch (State)
            {
                case ShooterState.Idle: TickIdle(); break;
                case ShooterState.Aim: TickAim(); break;
                case ShooterState.Shoot: TickShoot(); break;
                case ShooterState.Cooldown: TickCooldown(); break;
            }
        }

        private void TickIdle()
        {
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist <= stats.attackRange && CanSeePlayer())
                {
                    State = ShooterState.Aim;
                    _timer = stats.windUpTime;
                }
            }
        }

        private void TickAim()
        {
            FacePlayer();

            if (sprite != null)
            {
                float t = 1f - Mathf.Clamp01(_timer / Mathf.Max(0.01f, stats.windUpTime));
                sprite.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.15f), t);
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                if (CanSeePlayer()) { State = ShooterState.Shoot; }
                else { State = ShooterState.Idle; if (sprite != null) sprite.color = Color.white; }
            }
        }

        private void TickShoot()
        {
            if (sprite != null) sprite.color = Color.white;
            if (projectilePrefab != null && player != null)
            {
                Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
                Vector2 dir = ((Vector2)player.position - (Vector2)spawnPos).normalized;
                var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                proj.Launch(dir, stats.projectileSpeed, stats.contactDamage);
            }
            State = ShooterState.Cooldown;
            _timer = stats.attackCooldown;
        }

        private void TickCooldown()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) State = ShooterState.Idle;
        }
    }
}
