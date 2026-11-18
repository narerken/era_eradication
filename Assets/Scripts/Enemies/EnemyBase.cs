using UnityEngine;
using Emberfall.Data;
using Emberfall.Player;
using Emberfall.Visual;

namespace Emberfall.Enemies
{

    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected EnemyStats stats;
        [SerializeField] protected SpriteRenderer sprite;
        [SerializeField] protected float patrolRadius = 2.5f;
        [SerializeField] protected SimpleSpriteAnimator anim;

        protected Rigidbody2D rb;
        protected Transform player;
        protected EnemyHealth health;
        protected bool facingRight = true;

        protected Vector2 spawnPoint;
        protected Vector2 patrolA, patrolB;
        protected Vector2 currentPatrolTarget;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<EnemyHealth>();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;

            spawnPoint = transform.position;
            RecomputePatrolPoints();
        }

        private void RecomputePatrolPoints()
        {
            patrolA = spawnPoint + Vector2.left * patrolRadius;
            patrolB = spawnPoint + Vector2.right * patrolRadius;
            currentPatrolTarget = patrolB;
        }

        public void Configure(float radius)
        {
            if (radius <= 0f) return;
            patrolRadius = radius;
            RecomputePatrolPoints();
        }

        protected bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
            float distance = toPlayer.magnitude;
            if (distance > stats.visionRange) return false;

            Vector2 facing = facingRight ? Vector2.right : Vector2.left;
            float angle = Vector2.Angle(facing, toPlayer);
            if (angle > stats.visionAngle) return false;

            var hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, stats.visionBlockers);
            return hit.collider == null;
        }

        protected void FacePlayer()
        {
            if (player == null) return;
            SetFacing(player.position.x > transform.position.x);
        }

        protected void SetFacing(bool right)
        {
            facingRight = right;
            if (sprite != null) sprite.flipX = !right;
        }

        protected void MoveToward(Vector2 worldTarget, float speed)
        {
            float dir = Mathf.Sign(worldTarget.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
            if (Mathf.Abs(dir) > 0.01f) SetFacing(dir > 0f);
        }

        protected void Stop() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        protected virtual void LateUpdate()
        {
            if (anim == null) return;
            anim.Play(Mathf.Abs(rb.linearVelocity.x) > 0.05f ? "Run" : "Idle");
        }

        protected void OnCollisionEnter2D(Collision2D other) => HandlePlayerContact(other);
        protected void OnCollisionStay2D(Collision2D other) => HandlePlayerContact(other);

        private void HandlePlayerContact(Collision2D other)
        {
            var playerHealth = other.collider.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            bool fromAbove = false;
            foreach (var contact in other.contacts)
            {

                if (contact.normal.y > 0.55f) { fromAbove = true; break; }
            }

            var playerRb = other.rigidbody;
            bool descending = playerRb == null || playerRb.linearVelocity.y <= 0.1f;

            if (fromAbove && descending && health != null)
            {
                health.TakeDamage(stats.stompDamage);
                if (playerRb != null)
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, stats.stompBounce);
                return;
            }

            playerHealth.TakeDamage(stats.contactDamage, transform.position);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
            if (!Application.isPlaying)
            {
                Vector2 a = (Vector2)transform.position + Vector2.left * patrolRadius;
                Vector2 b = (Vector2)transform.position + Vector2.right * patrolRadius;
                Gizmos.DrawLine(a, b);
            }
        }
    }
}
