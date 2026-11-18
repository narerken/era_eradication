using UnityEngine;
using Emberfall.Player;

namespace Emberfall.World
{

    public class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        private void OnTriggerEnter2D(Collider2D other) => Hurt(other);
        private void OnTriggerStay2D(Collider2D other) => Hurt(other);

        private void Hurt(Collider2D other)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage, transform.position);
        }
    }
}
