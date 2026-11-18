using UnityEngine;

namespace Emberfall.Data
{
    public enum EnemyKind { Patroller, Shooter, Charger }

    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Emberfall/Enemy Stats")]
    public class EnemyStats : ScriptableObject
    {
        public EnemyKind kind = EnemyKind.Patroller;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public float chaseSpeedMultiplier = 1.6f;

        [Header("Health")]
        public int maxHP = 40;
        public int contactDamage = 1;

        [Header("Player Interaction")]
        [Tooltip("Damage dealt when the player lands on top of this enemy (the stomp). " +
                 "Defaults well above any maxHP so a stomp is always a one-hit kill, same as " +
                 "touching the enemy from the side is always a hit against the player.")]
        public int stompDamage = 999;
        [Tooltip("Upward velocity given to the player immediately after a successful stomp.")]
        public float stompBounce = 9f;

        [Header("Senses")]
        [Tooltip("Meters. Beyond this the enemy cannot notice the player at all.")]
        public float visionRange = 6f;
        [Tooltip("Half-angle of the vision cone, degrees. 180 = sees all around.")]
        public float visionAngle = 70f;
        [Tooltip("Layers that block the sight raycast (walls).")]
        public LayerMask visionBlockers;

        [Header("Attack timing (fair-play requirement: 0.3-0.8s visible wind-up)")]
        [Range(0.3f, 0.8f)] public float windUpTime = 0.6f;
        public float attackCooldown = 1.4f;
        [Tooltip("Shooter only: projectile speed.")]
        public float projectileSpeed = 7f;
        [Tooltip("Shooter only: max range it will fire from.")]
        public float attackRange = 5f;
        [Tooltip("Charger only: dash speed multiplier during the charge state.")]
        public float chargeSpeedMultiplier = 3.2f;
        [Tooltip("Charger only: how long the charge dash lasts, seconds.")]
        public float chargeDuration = 0.5f;

        [Header("Patrol")]
        public float patrolWaitAtPoint = 1f;
        [Tooltip("If the player is lost from sight for longer than this, give up the chase.")]
        public float loseInterestTime = 2.5f;
    }
}
