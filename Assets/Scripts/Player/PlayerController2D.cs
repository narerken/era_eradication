using UnityEngine;
using Emberfall.Data;
using Emberfall.Input;
using Emberfall.Visual;

namespace Emberfall.Player
{
    public enum PlayerState { Idle, Run, Jump, Fall, Hurt, Dead, LevelComplete }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.18f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private SimpleSpriteAnimator anim;

        public PlayerState State { get; private set; } = PlayerState.Idle;
        public bool IsGrounded { get; private set; }

        private Rigidbody2D _rb;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _facingRight = true;
        private PlayerHealth _health;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<PlayerHealth>();
            if (stats != null) _rb.gravityScale = stats.gravityScale;

            _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        private void Update()
        {
            MobileInput.EndFrame();

            if (State == PlayerState.Dead || State == PlayerState.LevelComplete) return;

            CheckFallDeath();
            if (State == PlayerState.Dead) return;

            GroundCheck();
            HandleTimers();
            HandleFacing();
            UpdateState();
        }

        private void CheckFallDeath()
        {
            if (stats == null || _health == null) return;
            if (transform.position.y < stats.fallDeathY)
                _health.TakeDamage(_health.CurrentHearts, transform.position);
        }

        private void FixedUpdate()
        {
            if (State == PlayerState.Dead || State == PlayerState.Hurt || State == PlayerState.LevelComplete) return;
            Move();
            ApplyFallGravity();
        }

        private void GroundCheck()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = groundCheck != null &&
                         Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (IsGrounded) _coyoteTimer = stats.coyoteTime;
            else if (wasGrounded) _coyoteTimer = stats.coyoteTime;
        }

        private void HandleTimers()
        {
            _coyoteTimer -= Time.deltaTime;
            _jumpBufferTimer -= Time.deltaTime;

            if (MobileInput.JumpPressedThisFrame)
            {
                _jumpBufferTimer = stats.jumpBufferTime;
                MobileInput.ConsumeJumpPressed();
            }

            bool canJump = _coyoteTimer > 0f && _jumpBufferTimer > 0f;
            if (canJump)
            {
                _coyoteTimer = 0f;
                _jumpBufferTimer = 0f;
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, stats.jumpForce);
            }
        }

        private void Move()
        {
            float h = MobileInput.Horizontal;
            float control = IsGrounded ? 1f : stats.airControl;
            float targetX = h * stats.moveSpeed;
            float newX = Mathf.Lerp(_rb.linearVelocity.x, targetX, control);
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
        }

        private void ApplyFallGravity()
        {
            _rb.gravityScale = _rb.linearVelocity.y < -0.01f
                ? stats.gravityScale * stats.fallGravityMultiplier
                : stats.gravityScale;
        }

        private void HandleFacing()
        {
            if (MobileInput.Horizontal > 0.05f) _facingRight = true;
            else if (MobileInput.Horizontal < -0.05f) _facingRight = false;
            if (sprite != null) sprite.flipX = !_facingRight;
        }

        private void UpdateState()
        {
            if (_health != null && _health.IsInvulnerable && State != PlayerState.Hurt)
            {

            }

            if (State == PlayerState.Hurt) return;

            if (!IsGrounded)
                State = _rb.linearVelocity.y > 0.05f ? PlayerState.Jump : PlayerState.Fall;
            else
                State = Mathf.Abs(MobileInput.Horizontal) > 0.05f ? PlayerState.Run : PlayerState.Idle;

            anim?.Play(ClipFor(State));
        }

        private static string ClipFor(PlayerState s) => s switch
        {
            PlayerState.Run => "Run",
            PlayerState.Jump => "Jump",
            PlayerState.Fall => "Fall",
            PlayerState.Hurt => "Hit",
            PlayerState.Dead => "Hit",
            _ => "Idle"
        };

        public void EnterHurt(Vector2 knockbackDir)
        {
            if (State == PlayerState.Dead || State == PlayerState.LevelComplete) return;
            State = PlayerState.Hurt;
            _rb.linearVelocity = knockbackDir.normalized * stats.knockbackForce;
            anim?.Play("Hit");
            Invoke(nameof(ExitHurt), 0.25f);
        }

        private void ExitHurt()
        {
            if (State == PlayerState.Hurt) State = PlayerState.Idle;
        }

        public void EnterDead()
        {
            if (State == PlayerState.LevelComplete) return;
            State = PlayerState.Dead;
            _rb.linearVelocity = Vector2.zero;
            anim?.Play("Hit");
        }

        public void EnterLevelComplete()
        {
            if (State == PlayerState.Dead) return;
            State = PlayerState.LevelComplete;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            anim?.Play("Idle");
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
