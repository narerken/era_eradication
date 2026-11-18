using System;
using UnityEngine;
using Emberfall.Data;

namespace Emberfall.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyStats stats;
        public event Action OnDied;

        private int _hp;

        private void Awake() => _hp = stats != null ? stats.maxHP : 30;

        public void TakeDamage(int amount)
        {
            _hp -= amount;
            if (_hp <= 0)
            {
                OnDied?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
