using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberfall.Data
{
    [Serializable]
    public struct PlatformSpec
    {
        public Vector2 center;
        public Vector2 size;
    }

    [Serializable]
    public struct EnemySpawnSpec
    {
        public EnemyKind kind;
        public Vector2 position;
        public bool faceRight;
        [Tooltip("For Patroller/Charger: how far left/right of 'position' it patrols.")]
        public float patrolRadius;
    }

    [Serializable]
    public struct HazardSpec
    {
        public Vector2 center;
        public Vector2 size;
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "Emberfall/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex = 1;
        public string displayName = "Level 1";
        [Tooltip("Shown on the Level Select screen; also used as the playtest par time.")]
        public float timeLimitSeconds = 90f;

        public Vector2 playerStart;
        public Vector2 goalPosition;

        public List<PlatformSpec> platforms = new List<PlatformSpec>();
        public List<EnemySpawnSpec> enemies = new List<EnemySpawnSpec>();
        public List<Vector2> pickups = new List<Vector2>();
        public List<HazardSpec> hazards = new List<HazardSpec>();

        [TextArea] public string designNotes;
    }
}
