using UnityEngine;
using Emberfall.Core;
using Emberfall.Data;
using Emberfall.Player;

namespace Emberfall.World
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private float bobSpeed = 2.5f;

        private Vector3 _start;

        private void Awake() => _start = transform.position;
        private void Update() => transform.position = _start + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<PlayerHealth>();
            if (health == null) return;

            if (item.kind == ItemKind.EmberShard)
                GameManager.Instance?.AddScore(item.scoreValue);
            else
                health.Heal(item.healAmount);

            Destroy(gameObject);
        }
    }
}
