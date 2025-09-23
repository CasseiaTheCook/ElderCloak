using UnityEngine;
using UnityEngine.InputSystem;
using ElderCloak.Interfaces;

namespace ElderCloak.Player
{
    /// <summary>
    /// Main player controller for 2D platformer movement
    /// Handles basic movement, jumping, double jumping, and dashing mechanics
    /// Designed to work with Unity's new Input System
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float doubleJumpForce = 8f;
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        
        [Header("Ground Detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        [Header("Physics")]
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float maxFallSpeed = 20f;
        
        // Components
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private PlayerInput playerInput;
        
        // Movement state
        private Vector2 moveInput;
        private bool isGrounded;
        private bool hasDoubleJump;
        private bool isDashing;
        private bool canDash = true;
        private float dashTimer;
        private float dashCooldownTimer;
        private Vector2 dashDirection;
        
        // Input state
        private bool jumpPressed;
        private bool jumpHeld;
        private bool dashPressed;
        
        /// <summary>
        /// Current movement speed (affected by dashing)
        /// </summary>
        public float CurrentMoveSpeed => isDashing ? dashSpeed : moveSpeed;
        
        /// <summary>
        /// Whether the player is currently on the ground
        /// </summary>
        public bool IsGrounded => isGrounded;
        
        /// <summary>
        /// Whether the player is currently dashing
        /// </summary>
        public bool IsDashing => isDashing;
        
        /// <summary>
        /// Whether the player can currently dash
        /// </summary>
        public bool CanDash => canDash && dashCooldownTimer <= 0f;
        
        private void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerInput = GetComponent<PlayerInput>();
            
            // Set up ground check if not assigned
            if (groundCheck == null)
            {
                GameObject groundCheckGO = new GameObject("GroundCheck");
                groundCheckGO.transform.SetParent(transform);
                groundCheckGO.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = groundCheckGO.transform;
            }
        }
        
        private void OnEnable()
        {
            // Input events will be handled through Unity Events or direct input calls
        }
        
        private void OnDisable()
        {
            // Cleanup if needed
        }
        
        private void Update()
        {
            HandleInput();
            UpdateTimers();
        }
        
        private void FixedUpdate()
        {
            CheckGrounded();
            HandleMovement();
            HandleJumping();
            HandleDashing();
            ApplyPhysics();
        }
        
        /// <summary>
        /// Handle input reading
        /// </summary>
        private void HandleInput()
        {
            // Read movement input
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            // Read jump input
            if (Input.GetButtonDown("Jump"))
                jumpPressed = true;
            
            jumpHeld = Input.GetButton("Jump");
            
            // Read dash input (Left Alt key)
            if (Input.GetKeyDown(KeyCode.LeftAlt))
                dashPressed = true;
        }
        
        /// <summary>
        /// Update various timers
        /// </summary>
        private void UpdateTimers()
        {
            if (dashTimer > 0f)
                dashTimer -= Time.deltaTime;
            
            if (dashCooldownTimer > 0f)
                dashCooldownTimer -= Time.deltaTime;
        }
        
        /// <summary>
        /// Check if player is grounded
        /// </summary>
        private void CheckGrounded()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
            
            // Reset double jump when landing
            if (isGrounded && !wasGrounded)
            {
                hasDoubleJump = true;
            }
        }
        
        /// <summary>
        /// Handle horizontal movement
        /// </summary>
        private void HandleMovement()
        {
            if (isDashing) return;
            
            float targetVelocityX = moveInput.x * moveSpeed;
            rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);
            
            // Handle sprite flipping
            if (moveInput.x > 0.1f)
                spriteRenderer.flipX = false;
            else if (moveInput.x < -0.1f)
                spriteRenderer.flipX = true;
        }
        
        /// <summary>
        /// Handle jumping and double jumping
        /// </summary>
        private void HandleJumping()
        {
            if (!jumpPressed) return;
            
            if (isGrounded)
            {
                // Regular jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                hasDoubleJump = true;
            }
            else if (hasDoubleJump && !isDashing)
            {
                // Double jump
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                hasDoubleJump = false;
            }
            
            jumpPressed = false;
        }
        
        /// <summary>
        /// Handle dash mechanics
        /// </summary>
        private void HandleDashing()
        {
            if (dashPressed && CanDash)
            {
                StartDash();
                dashPressed = false;
            }
            
            if (isDashing)
            {
                if (dashTimer <= 0f)
                {
                    EndDash();
                }
                else
                {
                    // Apply dash movement
                    rb.velocity = dashDirection * dashSpeed;
                }
            }
        }
        
        /// <summary>
        /// Start a dash in the current movement direction
        /// </summary>
        private void StartDash()
        {
            isDashing = true;
            canDash = false;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            
            // Determine dash direction
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                dashDirection = new Vector2(Mathf.Sign(moveInput.x), 0f);
            }
            else
            {
                // Default to facing direction if no input
                dashDirection = new Vector2(spriteRenderer.flipX ? -1f : 1f, 0f);
            }
            
            // Reset gravity scale during dash
            rb.gravityScale = 0f;
        }
        
        /// <summary>
        /// End the dash and restore normal physics
        /// </summary>
        private void EndDash()
        {
            isDashing = false;
            rb.gravityScale = 1f;
            
            // Allow dashing again when grounded
            if (isGrounded)
            {
                canDash = true;
                dashCooldownTimer = 0f;
            }
        }
        
        /// <summary>
        /// Apply enhanced physics for better feel
        /// </summary>
        private void ApplyPhysics()
        {
            if (isDashing) return;
            
            // Apply fall multiplier for better jump feel
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0 && !jumpHeld)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
            
            // Clamp fall speed
            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
            }
        }
        
        #region Input Event Handlers (for Unity Events)
        /// <summary>
        /// Handle jump input through Unity Events
        /// </summary>
        /// <param name="context">Input action context</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                jumpPressed = true;
        }
        
        /// <summary>
        /// Handle dash input through Unity Events  
        /// </summary>
        /// <param name="context">Input action context</param>
        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
                dashPressed = true;
        }
        
        /// <summary>
        /// Handle move input through Unity Events
        /// </summary>
        /// <param name="context">Input action context</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Force stop any current dash
        /// </summary>
        public void StopDash()
        {
            if (isDashing)
            {
                EndDash();
            }
        }
        
        /// <summary>
        /// Reset dash cooldown (for power-ups or abilities)
        /// </summary>
        public void ResetDashCooldown()
        {
            dashCooldownTimer = 0f;
            if (isGrounded)
            {
                canDash = true;
            }
        }
        
        /// <summary>
        /// Set whether the player can dash
        /// </summary>
        /// <param name="canDashValue">Whether the player can dash</param>
        public void SetCanDash(bool canDashValue)
        {
            canDash = canDashValue;
        }
        #endregion
        
        #region Debug
        private void OnDrawGizmosSelected()
        {
            // Draw ground check radius
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
        #endregion
    }
}