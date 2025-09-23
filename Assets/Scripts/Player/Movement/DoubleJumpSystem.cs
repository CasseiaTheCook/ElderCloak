using UnityEngine;

namespace ElderCloak.Player.Movement
{
    [System.Serializable]
    public class DoubleJumpSystem
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float doubleJumpForce = 10f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;
        
        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private float groundCheckDistance = 0.1f;
        
        private int jumpCount;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool wasGrounded;
        
        private Rigidbody2D rb;
        private Transform groundCheck;
        
        public bool IsGrounded { get; private set; }
        public bool CanJump => (IsGrounded && jumpCount == 0) || (jumpCount < maxJumps && jumpCount > 0) || coyoteTimer > 0;
        public int RemainingJumps => Mathf.Max(0, maxJumps - jumpCount);
        
        public void Initialize(Rigidbody2D rigidbody, Transform groundCheckTransform)
        {
            rb = rigidbody;
            groundCheck = groundCheckTransform;
            jumpCount = 0;
        }
        
        public void Update()
        {
            CheckGrounded();
            UpdateTimers();
        }
        
        private void CheckGrounded()
        {
            IsGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            
            if (IsGrounded && !wasGrounded)
            {
                // Just landed
                jumpCount = 0;
            }
            
            if (!IsGrounded && wasGrounded)
            {
                // Just left ground, start coyote time
                coyoteTimer = coyoteTime;
            }
            
            wasGrounded = IsGrounded;
        }
        
        private void UpdateTimers()
        {
            if (coyoteTimer > 0)
                coyoteTimer -= Time.deltaTime;
            
            if (jumpBufferTimer > 0)
                jumpBufferTimer -= Time.deltaTime;
        }
        
        public void RequestJump()
        {
            jumpBufferTimer = jumpBufferTime;
        }
        
        public bool TryJump()
        {
            if (jumpBufferTimer <= 0)
                return false;
            
            if (CanJump)
            {
                PerformJump();
                jumpBufferTimer = 0; // Consume the jump buffer
                return true;
            }
            
            return false;
        }
        
        private void PerformJump()
        {
            float jumpPower = jumpCount == 0 ? jumpForce : doubleJumpForce;
            
            // Reset Y velocity before applying jump force for consistent jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            
            jumpCount++;
            coyoteTimer = 0; // Reset coyote time when jumping
            
            Debug.Log($"Performed jump {jumpCount}/{maxJumps}");
        }
        
        public void ResetJumps()
        {
            jumpCount = 0;
        }
        
        // For debugging
        public void DrawGizmos()
        {
            if (groundCheck != null)
            {
                Color rayColor = IsGrounded ? Color.green : Color.red;
                Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, rayColor);
            }
        }
    }
}