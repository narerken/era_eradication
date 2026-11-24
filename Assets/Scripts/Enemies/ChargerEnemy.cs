using UnityEngine;
using Emberfall.Audio;

namespace Emberfall.Enemies
{
    public enum ChargerState { Patrol, WindUp, Charge, Cooldown }

    public class ChargerEnemy : EnemyBase
    {
        private float _timer;
        private Vector2 _chargeDir;

        public ChargerState State { get; private set; } = ChargerState.Patrol;

        private void Update()
        {
            switch (State)
            {
                case ChargerState.Patrol: TickPatrol(); break;
                case ChargerState.WindUp: TickWindUp(); break;
                case ChargerState.Charge: TickCharge(); break;
                case ChargerState.Cooldown: TickCooldown(); break;
            }
        }

        private void TickPatrol()
        {
            if (CanSeePlayer())
            {
                State = ChargerState.WindUp;
                _timer = stats.windUpTime;
                Stop();
                FacePlayer();
                AudioManager.Instance?.PlayEnemyWindUp();
                return;
            }

            float distToTarget = Mathf.Abs(transform.position.x - currentPatrolTarget.x);
            if (distToTarget < 0.1f)
                currentPatrolTarget = currentPatrolTarget == patrolA ? patrolB : patrolA;
            else
                MoveToward(currentPatrolTarget, stats.moveSpeed);
        }

        private void TickWindUp()
        {
            Stop();

            if (sprite != null)
                transform.position += (Vector3)(Random.insideUnitCircle * 0.03f);
            if (sprite != null)
                sprite.color = Color.Lerp(Color.white, new Color(1f, 0.25f, 0.1f), Mathf.PingPong(Time.time * 12f, 1f));

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _chargeDir = player != null
                    ? ((Vector2)player.position - (Vector2)transform.position).normalized
                    : (facingRight ? Vector2.right : Vector2.left);

                _chargeDir = new Vector2(Mathf.Sign(_chargeDir.x), 0f);
                State = ChargerState.Charge;
                _timer = stats.chargeDuration;
                if (sprite != null) sprite.color = Color.white;
            }
        }

        private void TickCharge()
        {
            rb.linearVelocity = new Vector2(_chargeDir.x * stats.moveSpeed * stats.chargeSpeedMultiplier, rb.linearVelocity.y);
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                State = ChargerState.Cooldown;
                _timer = stats.attackCooldown;
                Stop();
            }
        }

        private void TickCooldown()
        {
            if (sprite != null) sprite.color = new Color(1f, 1f, 1f, 0.6f);
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                if (sprite != null) sprite.color = Color.white;
                State = ChargerState.Patrol;
            }
        }
    }
}
