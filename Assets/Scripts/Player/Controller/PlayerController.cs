using UnityEngine;
using ElderCloak.Player.Input;
using ElderCloak.Player.Movement;
using ElderCloak.Player.Combat;
using ElderCloak.Player.Health;

namespace ElderCloak.Player.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float accelerationTime = 0.1f;
        [SerializeField] private float decelerationTime = 0.1f;
        
        [Header("Transform References")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform attackPoint;
        
        [Header("Systems")]
        [SerializeField] private DoubleJumpSystem doubleJumpSystem;
        [SerializeField] private DashSystem dashSystem;
        [SerializeField] private MeleeAttackSystem meleeAttackSystem;
        
        // Components
        private Rigidbody2D rb;
        private PlayerInputHandler inputHandler;
        private HealthSystem healthSystem;
        
        // Movement variables
        private float velocityXSmoothing;
        private bool facingRight = true;
        
        // Properties
        public bool IsGrounded => doubleJumpSystem.IsGrounded;
        public bool IsDashing => dashSystem.IsDashing;
        public bool IsAttacking => meleeAttackSystem.IsAttacking;
        public Vector2 FacingDirection => facingRight ? Vector2.right : Vector2.left;
        
        private void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody2D>();
            inputHandler = GetComponent<PlayerInputHandler>();
            healthSystem = GetComponent<HealthSystem>();
            
            // Validate references
            ValidateReferences();
            
            // Initialize systems
            InitializeSystems();
        }
        
        private void ValidateReferences()
        {
            if (groundCheck == null)
            {
                // Create ground check if not assigned
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
                groundCheck = groundCheckObj.transform;
                Debug.LogWarning("Ground check was not assigned. Created automatically.");
            }
            
            if (attackPoint == null)
            {
                // Create attack point if not assigned
                GameObject attackPointObj = new GameObject("AttackPoint");
                attackPointObj.transform.SetParent(transform);
                attackPointObj.transform.localPosition = new Vector3(1f, 0, 0);
                attackPoint = attackPointObj.transform;
                Debug.LogWarning("Attack point was not assigned. Created automatically.");
            }
        }
        
        private void InitializeSystems()
        {
            doubleJumpSystem.Initialize(rb, groundCheck);
            dashSystem.Initialize(rb);
            meleeAttackSystem.Initialize(attackPoint, this);
            
            // Subscribe to health events
            healthSystem.OnDeath.AddListener(OnPlayerDeath);
        }
        
        private void Update()
        {
            // Update systems
            doubleJumpSystem.Update();
            dashSystem.Update();
            meleeAttackSystem.Update();
            
            // Handle input
            HandleInput();
            
            // Update dash system with ground state
            dashSystem.OnGroundedChanged(IsGrounded);
        }
        
        private void FixedUpdate()
        {
            // Handle movement (only if not dashing)
            if (!IsDashing)
            {
                HandleMovement();
            }
        }
        
        private void HandleInput()
        {
            // Jump input
            if (inputHandler.JumpPressed)
            {
                doubleJumpSystem.RequestJump();
            }
            
            // Try to jump if requested
            doubleJumpSystem.TryJump();
            
            // Dash input
            if (inputHandler.DashPressed)
            {
                dashSystem.TryDash(inputHandler.MoveInput, FacingDirection);
            }
            
            // Attack input
            if (inputHandler.AttackPressed)
            {
                meleeAttackSystem.TryAttack();
            }
        }
        
        private void HandleMovement()
        {
            float targetVelocityX = inputHandler.MoveInput.x * moveSpeed;
            
            // Apply acceleration/deceleration
            float smoothTime = Mathf.Abs(targetVelocityX) > 0.1f ? accelerationTime : decelerationTime;
            float newVelocityX = Mathf.SmoothDamp(rb.linearVelocity.x, targetVelocityX, 
                                                  ref velocityXSmoothing, smoothTime);
            
            // Apply velocity
            rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
            
            // Handle sprite flipping
            HandleSpriteFlipping();
        }
        
        private void HandleSpriteFlipping()
        {
            if (inputHandler.MoveInput.x > 0.1f && !facingRight)
            {
                Flip();
            }
            else if (inputHandler.MoveInput.x < -0.1f && facingRight)
            {
                Flip();
            }
        }
        
        private void Flip()
        {
            facingRight = !facingRight;
            transform.Rotate(0, 180, 0);
            
            // Update attack point position
            Vector3 attackPos = attackPoint.localPosition;
            attackPos.x *= -1;
            attackPoint.localPosition = attackPos;
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("Player Controller: Player has died!");
            // You can add death handling here (disable controls, play animation, etc.)
            
            // Example: Disable this component
            enabled = false;
            
            // You could also trigger a respawn after a delay
            Invoke(nameof(Respawn), 2f);
        }
        
        private void Respawn()
        {
            // Reset all systems
            doubleJumpSystem.ResetJumps();
            dashSystem.ResetDashes();
            meleeAttackSystem.ResetCooldown();
            
            // Reset health
            healthSystem.Respawn();
            
            // Re-enable controller
            enabled = true;
            
            Debug.Log("Player respawned!");
        }
        
        // Damage trigger (for enemy collisions)
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Hazard"))
            {
                // Take damage from enemy collision
                healthSystem.TakeDamage(10); // Default damage
                
                // Optional: Apply knockback
                Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;
                rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            }
        }
        
        // For debugging and visualization
        private void OnDrawGizmos()
        {
            doubleJumpSystem.DrawGizmos();
            meleeAttackSystem.DrawGizmos();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (healthSystem != null)
            {
                healthSystem.OnDeath.RemoveListener(OnPlayerDeath);
            }
        }
    }
}