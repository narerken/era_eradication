using UnityEngine;
using Emberfall.Player;

namespace Emberfall.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        private int _damage;

        public void Launch(Vector2 direction, float speed, int damage)
        {
            _damage = damage;
            GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other) => HandleHit(other);
        private void OnCollisionEnter2D(Collision2D other) => HandleHit(other.collider);

        private void HandleHit(Collider2D other)
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage, transform.position);
                Destroy(gameObject);
                return;
            }
            if (other.GetComponent<Projectile>() == null && !other.isTrigger)
                Destroy(gameObject);
        }
    }
}
