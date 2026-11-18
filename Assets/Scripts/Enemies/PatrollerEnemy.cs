using UnityEngine;

namespace Emberfall.Enemies
{
    public enum PatrollerState { Patrol, Chase, Attack, Return }

    public class PatrollerEnemy : EnemyBase
    {
        public PatrollerState State { get; private set; } = PatrollerState.Patrol;

        private float _waitTimer;
        private float _loseInterestTimer;
        private float _windUpTimer;
        private float _cooldownTimer;
        private bool _lunging;

        protected override void Awake()
        {
            base.Awake();
            currentPatrolTarget = patrolB;
        }

        private void Update()
        {
            switch (State)
            {
                case PatrollerState.Patrol: TickPatrol(); break;
                case PatrollerState.Chase: TickChase(); break;
                case PatrollerState.Attack: TickAttack(); break;
                case PatrollerState.Return: TickReturn(); break;
            }
        }

        private void TickPatrol()
        {
            if (CanSeePlayer()) { State = PatrollerState.Chase; _loseInterestTimer = stats.loseInterestTime; return; }

            float distToTarget = Mathf.Abs(transform.position.x - currentPatrolTarget.x);
            if (distToTarget < 0.1f)
            {
                Stop();
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= stats.patrolWaitAtPoint)
                {
                    _waitTimer = 0f;
                    currentPatrolTarget = currentPatrolTarget == patrolA ? patrolB : patrolA;
                }
            }
            else
            {
                MoveToward(currentPatrolTarget, stats.moveSpeed);
            }
        }

        private void TickChase()
        {
            if (CanSeePlayer())
            {
                _loseInterestTimer = stats.loseInterestTime;
                FacePlayer();

                float dist = Vector2.Distance(transform.position, player.position);
                if (dist < 1f)
                {
                    State = PatrollerState.Attack;
                    _windUpTimer = stats.windUpTime;
                    _lunging = false;
                    Stop();
                    return;
                }
                MoveToward(player.position, stats.moveSpeed * stats.chaseSpeedMultiplier);
            }
            else
            {
                _loseInterestTimer -= Time.deltaTime;
                if (_loseInterestTimer <= 0f) State = PatrollerState.Return;
            }
        }

        private void TickAttack()
        {
            if (!_lunging)
            {

                Stop();
                if (sprite != null) sprite.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 8f, 1f));
                _windUpTimer -= Time.deltaTime;
                if (_windUpTimer <= 0f)
                {
                    _lunging = true;
                    _cooldownTimer = stats.attackCooldown;
                    FacePlayer();
                }
                return;
            }

            if (sprite != null) sprite.color = Color.white;
            MoveToward(player != null ? (Vector2)player.position : transform.position,
                stats.moveSpeed * stats.chaseSpeedMultiplier * 1.3f);

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0f)
            {
                State = CanSeePlayer() ? PatrollerState.Chase : PatrollerState.Return;
                _loseInterestTimer = stats.loseInterestTime;
            }
        }

        private void TickReturn()
        {
            if (CanSeePlayer()) { State = PatrollerState.Chase; _loseInterestTimer = stats.loseInterestTime; return; }

            Vector2 nearest = Vector2.Distance(transform.position, patrolA) < Vector2.Distance(transform.position, patrolB) ? patrolA : patrolB;
            if (Vector2.Distance(transform.position, nearest) < 0.15f)
            {
                State = PatrollerState.Patrol;
                currentPatrolTarget = nearest;
            }
            else
            {
                MoveToward(nearest, stats.moveSpeed);
            }
        }
    }
}
