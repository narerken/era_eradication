using UnityEngine;

namespace Emberfall.Data
{

    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Emberfall/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 5.5f;
        public float jumpForce = 13f;
        public float airControl = 0.75f;
        [Tooltip("Extra ms after leaving a ledge during which jump still works.")]
        public float coyoteTime = 0.1f;
        [Tooltip("A jump pressed slightly before landing still fires.")]
        public float jumpBufferTime = 0.12f;

        [Header("Health")]
        public int maxHearts = 3;
        [Tooltip("Seconds of invulnerability / flashing after taking a hit.")]
        public float hurtInvulnerability = 0.8f;
        [Tooltip("Knockback impulse applied away from the hit source.")]
        public float knockbackForce = 6f;

        [Header("Feel")]
        public float gravityScale = 3.2f;
        public float fallGravityMultiplier = 1.4f;

        [Header("Safety")]
        [Tooltip("If the player's Y position drops below this (missed a jump over a gap), " +
                 "they die instantly instead of falling forever. Keep well below the lowest " +
                 "platform in any level.")]
        public float fallDeathY = -10f;
    }
}
