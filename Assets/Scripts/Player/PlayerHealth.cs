using System;
using UnityEngine;
using Emberfall.Core;
using Emberfall.Data;

namespace Emberfall.Player
{
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private SpriteRenderer sprite;

        public int CurrentHearts { get; private set; }
        public bool IsInvulnerable { get; private set; }

        public event Action<int, int> OnHeartsChanged;

        private PlayerController2D _controller;
        private float _invulnTimer;

        private void Awake()
        {
            _controller = GetComponent<PlayerController2D>();
            CurrentHearts = stats != null ? stats.maxHearts : 3;
        }

        private void Update()
        {
            if (_invulnTimer > 0f)
            {
                _invulnTimer -= Time.deltaTime;
                if (sprite != null) sprite.color = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f
                    ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
                if (_invulnTimer <= 0f)
                {
                    IsInvulnerable = false;
                    if (sprite != null) sprite.color = Color.white;
                }
            }
        }

        public void TakeDamage(int amount, Vector2 sourcePosition)
        {
            if (IsInvulnerable || CurrentHearts <= 0) return;

            CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
            OnHeartsChanged?.Invoke(CurrentHearts, stats.maxHearts);
            GameEvents.RaiseHeartsChanged(CurrentHearts, stats.maxHearts);

            Vector2 away = ((Vector2)transform.position - sourcePosition);
            if (away.sqrMagnitude < 0.01f) away = Vector2.up;
            _controller.EnterHurt(away + Vector2.up * 0.5f);

            if (CurrentHearts <= 0)
            {
                _controller.EnterDead();

                Invoke(nameof(RaiseDeathEvent), 0.4f);
            }
            else
            {
                IsInvulnerable = true;
                _invulnTimer = stats.hurtInvulnerability;
            }
        }

        private void RaiseDeathEvent() => GameEvents.RaisePlayerDied();

        public void Heal(int amount)
        {
            CurrentHearts = Mathf.Min(stats.maxHearts, CurrentHearts + amount);
            OnHeartsChanged?.Invoke(CurrentHearts, stats.maxHearts);
            GameEvents.RaiseHeartsChanged(CurrentHearts, stats.maxHearts);
        }
    }
}
