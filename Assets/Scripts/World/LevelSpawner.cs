using UnityEngine;
using Emberfall.Core;
using Emberfall.Data;
using Emberfall.Enemies;

namespace Emberfall.World
{

    public class LevelSpawner : MonoBehaviour
    {
        [Header("Fallback for testing Game.unity directly in the Editor")]
        [SerializeField] private LevelData fallbackLevel;

        [Header("Prefabs")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private GameObject patrollerPrefab;
        [SerializeField] private GameObject shooterPrefab;
        [SerializeField] private GameObject chargerPrefab;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private GameObject goalPrefab;
        [SerializeField] private GameObject hazardPrefab;

        private float _elapsed;
        public float Elapsed => _elapsed;
        public LevelData ActiveLevel { get; private set; }

        private void Start()
        {
            ActiveLevel = GameManager.Instance != null && GameManager.Instance.CurrentLevel != null
                ? GameManager.Instance.CurrentLevel
                : fallbackLevel;

            if (ActiveLevel == null)
            {
                Debug.LogError("[LevelSpawner] No LevelData available - assign a fallbackLevel for Editor testing.");
                return;
            }

            BuildLevel(ActiveLevel);
        }

        private void Update() => _elapsed += Time.deltaTime;

        private void BuildLevel(LevelData data)
        {
            foreach (var p in data.platforms)
            {
                if (platformPrefab == null) continue;
                var go = Instantiate(platformPrefab, p.center, Quaternion.identity, transform);
                go.transform.localScale = new Vector3(p.size.x, p.size.y, 1f);
            }

            foreach (var spawn in data.enemies)
            {
                GameObject prefab = spawn.kind switch
                {
                    EnemyKind.Patroller => patrollerPrefab,
                    EnemyKind.Shooter => shooterPrefab,
                    EnemyKind.Charger => chargerPrefab,
                    _ => null
                };
                if (prefab == null) continue;
                var enemy = Instantiate(prefab, spawn.position, Quaternion.identity, transform);
                var sr = enemy.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.flipX = !spawn.faceRight;

                enemy.GetComponent<EnemyBase>()?.Configure(spawn.patrolRadius);
            }

            foreach (var pos in data.pickups)
            {
                if (pickupPrefab == null) continue;

                if (IsNearStationaryEnemy(pos, data)) continue;
                Instantiate(pickupPrefab, pos, Quaternion.identity, transform);
            }

            foreach (var h in data.hazards)
            {
                if (hazardPrefab == null) continue;
                var go = Instantiate(hazardPrefab, h.center, Quaternion.identity, transform);
                go.transform.localScale = new Vector3(h.size.x, h.size.y, 1f);
            }

            if (goalPrefab != null) Instantiate(goalPrefab, data.goalPosition, Quaternion.identity, transform);

            if (player != null)
            {
                player.position = data.playerStart;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        private const float StationaryEnemyClearance = 0.8f;

        private static bool IsNearStationaryEnemy(Vector2 pos, LevelData data)
        {
            foreach (var e in data.enemies)
                if (e.kind == EnemyKind.Shooter && Vector2.Distance(pos, e.position) < StationaryEnemyClearance)
                    return true;
            return false;
        }
    }
}
