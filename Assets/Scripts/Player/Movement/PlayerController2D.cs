using UnityEngine;
using UnityEngine.InputSystem;
using ElderCloak.Core.Interfaces;

namespace ElderCloak.Player.Movement
{
    /// <summary>
    /// Hollow Knight-inspired 2D character controller with movement, double jump, and dash
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour, IMoveable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float airAcceleration = 10f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 16f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private float lowJumpGravityMultiplier = 3f;
        [SerializeField] private int maxJumps = 2; // 1 ground jump + 1 air jump (double jump)
        
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.8f;
        [SerializeField] private bool canDashInAir = true;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        // Components
        private Rigidbody2D rb;
        private PlayerInput playerInput;
        
        // Input
        private Vector2 moveInput;
        private bool jumpInputPressed;
        private bool jumpInputReleased;
        private bool dashInputPressed;
        
        // Movement state
        private int jumpCount;
        private bool isGrounded;
        private bool wasDashing;
        private float lastDashTime = -1f;
        
        // Dash state
        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;
        
        // Properties
        public Vector2 Velocity => rb.linearVelocity;
        public bool IsGrounded => isGrounded;
        public bool IsDashing => isDashing;
        public bool CanDash => Time.time - lastDashTime >= dashCooldown && (isGrounded || canDashInAir);
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerInput = GetComponent<PlayerInput>();
            
            // Create ground check point if not assigned
            if (groundCheckPoint == null)
            {
                GameObject groundCheck = new GameObject("GroundCheckPoint");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheckPoint = groundCheck.transform;
            }
        }
        
        private void Update()
        {
            CheckGroundStatus();
            HandleJumpCount();
            HandleDash();
        }
        
        private void FixedUpdate()
        {
            if (!isDashing)
            {
                HandleMovement();
                HandleJump();
                AdjustGravity();
            }
            else
            {
                HandleDashMovement();
            }
        }
        
        private void CheckGroundStatus()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayerMask);
            
            // Reset jump count when landing
            if (!wasGrounded && isGrounded)
            {
                jumpCount = 0;
            }
        }
        
        private void HandleJumpCount()
        {
            // Reset jump count when grounded
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                jumpCount = 0;
            }
        }
        
        private void HandleMovement()
        {
            float targetVelocity = moveInput.x * moveSpeed;
            float currentVelocityX = rb.linearVelocity.x;
            
            float accelerationRate;
            if (isGrounded)
            {
                accelerationRate = Mathf.Abs(targetVelocity) > 0.01f ? acceleration : deceleration;
            }
            else
            {
                accelerationRate = airAcceleration;
            }
            
            float velocityDiff = targetVelocity - currentVelocityX;
            float movement = velocityDiff * accelerationRate;
            
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }
        
        private void HandleJump()
        {
            if (jumpInputPressed && jumpCount < maxJumps)
            {
                // Stop vertical velocity before jumping
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpCount++;
                jumpInputPressed = false;
            }
            
            // Variable jump height
            if (jumpInputReleased && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                jumpInputReleased = false;
            }
        }
        
        private void HandleDash()
        {
            if (dashInputPressed && CanDash)
            {
                StartDash();
                dashInputPressed = false;
            }
            
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0)
                {
                    EndDash();
                }
            }
        }
        
        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
            
            // Determine dash direction
            if (moveInput.magnitude > 0.1f)
            {
                dashDirection = moveInput.normalized;
            }
            else
            {
                // Default to facing direction or right if no input
                dashDirection = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
            }
            
            // Stop current velocity and start dash
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        }
        
        private void HandleDashMovement()
        {
            // Maintain dash velocity
            rb.linearVelocity = dashDirection * dashForce;
        }
        
        private void EndDash()
        {
            isDashing = false;
            wasDashing = true;
            
            // Reduce velocity after dash
            rb.linearVelocity = rb.linearVelocity * 0.5f;
        }
        
        private void AdjustGravity()
        {
            if (rb.linearVelocity.y < 0)
            {
                // Falling
                rb.gravityScale = fallGravityMultiplier;
            }
            else if (rb.linearVelocity.y > 0 && !jumpInputPressed)
            {
                // Rising but not holding jump
                rb.gravityScale = lowJumpGravityMultiplier;
            }
            else
            {
                // Default gravity
                rb.gravityScale = 1;
            }
        }
        
        // Input System callbacks
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            
            // Handle sprite flipping
            if (moveInput.x > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (moveInput.x < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                jumpInputPressed = true;
            }
            else if (context.canceled)
            {
                jumpInputReleased = true;
            }
        }
        
        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                dashInputPressed = true;
            }
        }
        
        // IMoveable implementation
        public void Move(Vector2 direction)
        {
            moveInput = direction;
        }
        
        public void StopMovement()
        {
            moveInput = Vector2.zero;
        }
        
        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
            }
        }
    }
}