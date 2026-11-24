using System;
using UnityEngine;
using Emberfall.Audio;
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
            AudioManager.Instance?.PlayHit();
            if (_hp <= 0)
            {
                OnDied?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
