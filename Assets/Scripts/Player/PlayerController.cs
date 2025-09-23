using UnityEngine;
using ElderCloak.Interfaces;
using ElderCloak.Input;
using ElderCloak.Health;

namespace ElderCloak.Player
{
    /// <summary>
    /// Hollow Knight-inspired 2D character controller.
    /// Features: Ground and aerial movement, double jump, dash mechanics, wall sliding.
    /// Designed for modularity and extensibility with future Unity versions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IMovementController, IDamageable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 25f;
        [SerializeField] private float airAcceleration = 15f;
        [SerializeField] private float airDeceleration = 10f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 16f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;
        [SerializeField] private float maxFallSpeed = 25f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [Header("Double Jump Settings")]
        [SerializeField] private bool enableDoubleJump = true;
        [SerializeField] private float doubleJumpForce = 14f;
        [SerializeField] private int maxAirJumps = 1;

        [Header("Dash Settings")]
        [SerializeField] private bool enableDash = true;
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private int maxAirDashes = 1;
        [SerializeField] private bool dashResetsAirJumps = false;

        [Header("Ground Detection")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayerMask = -1;

        [Header("Wall Detection")]
        [SerializeField] private Transform wallCheckPoint;
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 1.5f);
        [SerializeField] private float wallSlideSpeed = 3f;
        [SerializeField] private bool enableWallSlide = true;

        // Component references
        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerInputHandler inputHandler;
        private HealthSystem healthSystem;

        // Movement state
        private Vector2 moveInput;
        private bool isFacingRight = true;
        private bool isGrounded;
        private bool wasGrounded;
        private bool isTouchingWall;
        private int wallDirection;

        // Jump state
        private bool isJumpPressed;
        private bool isJumpHeld;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private int airJumpsUsed;

        // Dash state
        private bool isDashing;
        private float dashTimer;
        private float lastDashTime;
        private int airDashesUsed;
        private Vector2 dashDirection;

        // Other state
        private bool isSprinting;

        #region Unity Lifecycle

        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            
            // Initialize health system
            healthSystem = new HealthSystem();
            healthSystem.Initialize(this);

            // Set up Rigidbody2D for optimal 2D platformer physics
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Create ground check point if not assigned
            if (groundCheckPoint == null)
            {
                GameObject groundCheck = new GameObject("GroundCheck");
                groundCheck.transform.SetParent(transform);
                groundCheck.transform.localPosition = new Vector3(0, -col.bounds.extents.y, 0);
                groundCheckPoint = groundCheck.transform;
            }

            // Create wall check point if not assigned
            if (wallCheckPoint == null)
            {
                GameObject wallCheck = new GameObject("WallCheck");
                wallCheck.transform.SetParent(transform);
                wallCheck.transform.localPosition = new Vector3(col.bounds.extents.x, 0, 0);
                wallCheckPoint = wallCheck.transform;
            }
        }

        private void Start()
        {
            SetupInputHandling();
        }

        private void Update()
        {
            healthSystem.Update();
            HandleTimers();
            CheckGroundedState();
            CheckWallState();
            HandleJumpInput();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                HandleDash();
            }
            else
            {
                HandleMovement();
                HandleGravity();
            }
        }

        #endregion

        #region Input Setup

        /// <summary>
        /// Set up input handling with the input system.
        /// </summary>
        private void SetupInputHandling()
        {
            // Try to find input handler in scene
            var inputHandlerComponent = FindObjectOfType<MonoBehaviour>() as PlayerInputHandler;
            if (inputHandlerComponent != null)
            {
                inputHandler = inputHandlerComponent;
                SubscribeToInputEvents();
            }
        }

        /// <summary>
        /// Set input handler manually for better control.
        /// </summary>
        /// <param name="handler">The input handler to use</param>
        public void SetInputHandler(PlayerInputHandler handler)
        {
            if (inputHandler != null)
                UnsubscribeFromInputEvents();
            
            inputHandler = handler;
            if (inputHandler != null)
                SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            if (inputHandler == null) return;

            inputHandler.OnMove += HandleMoveInput;
            inputHandler.OnJumpStarted += HandleJumpPressed;
            inputHandler.OnJumpCanceled += HandleJumpReleased;
            inputHandler.OnDashStarted += HandleDashPressed;
            inputHandler.OnSprintStarted += HandleSprintPressed;
            inputHandler.OnSprintCanceled += HandleSprintReleased;
        }

        private void UnsubscribeFromInputEvents()
        {
            if (inputHandler == null) return;

            inputHandler.OnMove -= HandleMoveInput;
            inputHandler.OnJumpStarted -= HandleJumpPressed;
            inputHandler.OnJumpCanceled -= HandleJumpReleased;
            inputHandler.OnDashStarted -= HandleDashPressed;
            inputHandler.OnSprintStarted -= HandleSprintPressed;
            inputHandler.OnSprintCanceled -= HandleSprintReleased;
        }

        #endregion

        #region Input Handlers

        private void HandleMoveInput(Vector2 input)
        {
            moveInput = input;
        }

        private void HandleJumpPressed()
        {
            isJumpPressed = true;
            isJumpHeld = true;
            jumpBufferTimer = jumpBufferTime;
        }

        private void HandleJumpReleased()
        {
            isJumpHeld = false;
        }

        private void HandleDashPressed()
        {
            if (CanDash())
            {
                Vector2 dashDir = moveInput.magnitude > 0.1f ? moveInput.normalized : 
                                 (isFacingRight ? Vector2.right : Vector2.left);
                Dash(dashDir);
            }
        }

        private void HandleSprintPressed()
        {
            isSprinting = true;
        }

        private void HandleSprintReleased()
        {
            isSprinting = false;
        }

        #endregion

        #region Ground and Wall Detection

        private void CheckGroundedState()
        {
            wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayerMask);

            // Reset air abilities when landing
            if (isGrounded && !wasGrounded)
            {
                airJumpsUsed = 0;
                airDashesUsed = 0;
                coyoteTimer = coyoteTime;
            }
        }

        private void CheckWallState()
        {
            Vector2 wallCheckPos = wallCheckPoint.position;
            wallCheckPos.x += (isFacingRight ? 1 : -1) * wallCheckSize.x * 0.5f;
            
            bool touchingWall = Physics2D.OverlapBox(wallCheckPos, wallCheckSize, 0f, groundLayerMask);
            isTouchingWall = touchingWall && !isGrounded;
            wallDirection = isFacingRight ? 1 : -1;
        }

        #endregion

        #region Movement Logic

        private void HandleMovement()
        {
            float targetSpeed = moveInput.x * moveSpeed;
            if (isSprinting) targetSpeed *= sprintMultiplier;

            // Apply different acceleration based on grounded state
            float accel = isGrounded ? acceleration : airAcceleration;
            float decel = isGrounded ? deceleration : airDeceleration;

            float speedDif = targetSpeed - rb.velocity.x;
            float movement = speedDif * (Mathf.Abs(speedDif) > 0.01f ? 
                           (Mathf.Abs(speedDif) > Mathf.Abs(targetSpeed) ? decel : accel) : decel);

            rb.AddForce(movement * Time.fixedDeltaTime * Vector2.right, ForceMode2D.Force);

            // Handle sprite flipping
            if (moveInput.x > 0 && !isFacingRight) Flip();
            else if (moveInput.x < 0 && isFacingRight) Flip();

            // Wall sliding
            if (enableWallSlide && isTouchingWall && rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }

        #endregion

        #region Jump Logic

        private void HandleTimers()
        {
            // Coyote time countdown
            if (!isGrounded && wasGrounded)
                coyoteTimer = coyoteTime;
            else if (coyoteTimer > 0)
                coyoteTimer -= Time.deltaTime;

            // Jump buffer countdown
            if (jumpBufferTimer > 0)
                jumpBufferTimer -= Time.deltaTime;

            // Dash timer countdown
            if (dashTimer > 0)
                dashTimer -= Time.deltaTime;
        }

        private void HandleJumpInput()
        {
            if (jumpBufferTimer > 0 && CanJump())
            {
                Jump();
                jumpBufferTimer = 0;
            }

            isJumpPressed = false;
        }

        private void HandleGravity()
        {
            // Apply enhanced gravity for better jump feel
            if (rb.velocity.y < 0 || (!isJumpHeld && rb.velocity.y > 0))
            {
                rb.gravityScale = fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale = 1f;
            }

            // Clamp fall speed
            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
            }
        }

        #endregion

        #region Dash Logic

        private void HandleDash()
        {
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.gravityScale = 1f;
                return;
            }

            // Apply dash force
            rb.velocity = dashDirection * dashForce;
            rb.gravityScale = 0f; // Disable gravity during dash
        }

        #endregion

        #region IMovementController Implementation

        public void Move(Vector2 direction)
        {
            moveInput = direction;
        }

        public void Jump()
        {
            if (isGrounded || coyoteTimer > 0)
            {
                // Ground jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                coyoteTimer = 0;
            }
            else if (enableDoubleJump && airJumpsUsed < maxAirJumps)
            {
                // Air jump
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                airJumpsUsed++;
            }
        }

        public void Dash(Vector2 direction)
        {
            if (!CanDash()) return;

            isDashing = true;
            dashDirection = direction.normalized;
            dashTimer = dashDuration;
            lastDashTime = Time.time;

            if (!isGrounded)
                airDashesUsed++;

            if (dashResetsAirJumps && !isGrounded)
                airJumpsUsed = 0;
        }

        public bool IsGrounded() => isGrounded;

        public bool CanJump()
        {
            return (isGrounded || coyoteTimer > 0) || 
                   (enableDoubleJump && airJumpsUsed < maxAirJumps);
        }

        public bool CanDash()
        {
            return enableDash && 
                   Time.time >= lastDashTime + dashCooldown && 
                   (isGrounded || airDashesUsed < maxAirDashes);
        }

        public Vector2 GetVelocity() => rb.velocity;

        public void SetVelocity(Vector2 velocity)
        {
            rb.velocity = velocity;
        }

        #endregion

        #region IDamageable Implementation

        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            healthSystem.TakeDamage(damage, damageSource);
        }

        public bool IsAlive() => healthSystem.IsAlive();

        public float GetHealthPercentage() => healthSystem.GetHealthPercentage();

        #endregion

        #region Public API

        /// <summary>
        /// Get access to the health system for external management.
        /// </summary>
        public HealthSystem GetHealthSystem() => healthSystem;

        /// <summary>
        /// Check if the player is currently dashing.
        /// </summary>
        public bool IsDashing() => isDashing;

        /// <summary>
        /// Check if the player is touching a wall.
        /// </summary>
        public bool IsTouchingWall() => isTouchingWall;

        /// <summary>
        /// Check if the player is facing right.
        /// </summary>
        public bool IsFacingRight() => isFacingRight;

        /// <summary>
        /// Force the player to face a specific direction.
        /// </summary>
        public void SetFacing(bool faceRight)
        {
            if (faceRight != isFacingRight)
                Flip();
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
            }

            if (wallCheckPoint != null)
            {
                Vector3 wallPos = wallCheckPoint.position;
                wallPos.x += (isFacingRight ? 1 : -1) * wallCheckSize.x * 0.5f;
                Gizmos.color = isTouchingWall ? Color.blue : Color.gray;
                Gizmos.DrawWireCube(wallPos, wallCheckSize);
            }
        }

        #endregion

        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
        }
    }
}