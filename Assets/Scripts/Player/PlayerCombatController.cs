using UnityEngine;
using ElderCloak.Combat;
using ElderCloak.Input;
using ElderCloak.Interfaces;

namespace ElderCloak.Player
{
    /// <summary>
    /// Player combat controller that integrates melee attacks with player movement.
    /// Handles attack input, direction, and coordination with the PlayerController.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombatController : MonoBehaviour, IAttackable
    {
        [Header("Combat System")]
        [SerializeField] private MeleeAttackSystem attackSystem = new MeleeAttackSystem();
        
        [Header("Attack Direction Settings")]
        [SerializeField] private bool useMovementDirection = true;
        [SerializeField] private bool allowUpwardAttacks = true;
        [SerializeField] private bool allowDownwardAttacks = true;
        [SerializeField] private float upwardAttackThreshold = 0.7f;
        [SerializeField] private float downwardAttackThreshold = -0.7f;

        // Component references
        private PlayerController playerController;
        private PlayerInputHandler inputHandler;

        // State tracking
        private Vector2 lastAttackDirection = Vector2.right;

        #region Unity Lifecycle

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            attackSystem.Initialize(this);

            // Subscribe to attack system events
            attackSystem.OnAttackStarted += HandleAttackStarted;
            attackSystem.OnAttackEnded += HandleAttackEnded;
            attackSystem.OnTargetHit += HandleTargetHit;
            attackSystem.OnComboIncreased += HandleComboIncreased;
        }

        private void Start()
        {
            SetupInputHandling();
        }

        private void Update()
        {
            attackSystem.Update();
        }

        #endregion

        #region Input Setup

        /// <summary>
        /// Set up input handling for combat actions.
        /// </summary>
        private void SetupInputHandling()
        {
            // Try to get input handler from player controller
            var inputHandlerComponent = FindObjectOfType<MonoBehaviour>() as PlayerInputHandler;
            if (inputHandlerComponent != null)
            {
                SetInputHandler(inputHandlerComponent);
            }
        }

        /// <summary>
        /// Set input handler manually.
        /// </summary>
        /// <param name="handler">The input handler to use</param>
        public void SetInputHandler(PlayerInputHandler handler)
        {
            if (inputHandler != null)
                inputHandler.OnAttackStarted -= HandleAttackInput;
                
            inputHandler = handler;
            
            if (inputHandler != null)
                inputHandler.OnAttackStarted += HandleAttackInput;
        }

        private void HandleAttackInput()
        {
            Vector2 attackDirection = GetAttackDirection();
            Attack(attackDirection);
        }

        #endregion

        #region Attack Direction Logic

        /// <summary>
        /// Determine the attack direction based on input and settings.
        /// </summary>
        /// <returns>The direction to attack in</returns>
        private Vector2 GetAttackDirection()
        {
            Vector2 attackDir = Vector2.right;

            if (useMovementDirection && inputHandler != null)
            {
                Vector2 moveInput = inputHandler.MoveInput;
                
                // Check for vertical attacks first
                if (allowUpwardAttacks && moveInput.y >= upwardAttackThreshold)
                {
                    attackDir = Vector2.up;
                }
                else if (allowDownwardAttacks && moveInput.y <= downwardAttackThreshold && !playerController.IsGrounded())
                {
                    attackDir = Vector2.down;
                }
                else if (Mathf.Abs(moveInput.x) > 0.1f)
                {
                    // Horizontal attack based on input
                    attackDir = moveInput.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    // No movement input, use facing direction
                    attackDir = playerController.IsFacingRight() ? Vector2.right : Vector2.left;
                }
            }
            else
            {
                // Use facing direction
                attackDir = playerController.IsFacingRight() ? Vector2.right : Vector2.left;
            }

            lastAttackDirection = attackDir;
            return attackDir;
        }

        #endregion

        #region Attack System Event Handlers

        /// <summary>
        /// Handle when an attack starts.
        /// </summary>
        private void HandleAttackStarted(Vector2 direction)
        {
            // Could add attack animations, sound effects, etc.
            Debug.Log($"Player started attack in direction: {direction}");
        }

        /// <summary>
        /// Handle when an attack ends.
        /// </summary>
        private void HandleAttackEnded()
        {
            // Could reset attack animations, etc.
            Debug.Log("Player attack ended");
        }

        /// <summary>
        /// Handle when an attack hits a target.
        /// </summary>
        private void HandleTargetHit(GameObject target)
        {
            Debug.Log($"Player hit target: {target.name}");
            // Could add screen shake, particle effects, etc.
        }

        /// <summary>
        /// Handle combo progression.
        /// </summary>
        private void HandleComboIncreased(int comboCount)
        {
            Debug.Log($"Player combo: {comboCount}");
            // Could add combo UI updates, special effects, etc.
        }

        #endregion

        #region IAttackable Implementation

        public void Attack(Vector2 direction)
        {
            // Ensure we're not in a state where attacking should be disabled
            if (!CanAttack()) return;

            attackSystem.Attack(direction);
        }

        public bool CanAttack()
        {
            // Add any player-specific attack restrictions here
            if (!playerController.IsAlive()) return false;
            if (playerController.IsDashing()) return false; // Can't attack while dashing
            
            return attackSystem.CanAttack();
        }

        public float GetAttackDamage() => attackSystem.GetAttackDamage();
        public float GetAttackRange() => attackSystem.GetAttackRange();

        #endregion

        #region Public API

        /// <summary>
        /// Get the current combo count.
        /// </summary>
        /// <returns>Current combo count</returns>
        public int GetComboCount()
        {
            return attackSystem.GetCurrentCombo();
        }

        /// <summary>
        /// Reset the combo counter.
        /// </summary>
        public void ResetCombo()
        {
            attackSystem.ResetCombo();
        }

        /// <summary>
        /// Check if currently performing an attack.
        /// </summary>
        /// <returns>True if attacking</returns>
        public bool IsAttacking()
        {
            return attackSystem.IsAttacking();
        }

        /// <summary>
        /// Get the direction of the last attack.
        /// </summary>
        /// <returns>Last attack direction</returns>
        public Vector2 GetLastAttackDirection()
        {
            return lastAttackDirection;
        }

        /// <summary>
        /// Get direct access to the attack system for advanced configuration.
        /// </summary>
        /// <returns>The melee attack system</returns>
        public MeleeAttackSystem GetAttackSystem()
        {
            return attackSystem;
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Vector2 direction = GetAttackDirection();
                attackSystem.DrawDebugGizmos(direction);
            }
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (attackSystem != null)
            {
                attackSystem.OnAttackStarted -= HandleAttackStarted;
                attackSystem.OnAttackEnded -= HandleAttackEnded;
                attackSystem.OnTargetHit -= HandleTargetHit;
                attackSystem.OnComboIncreased -= HandleComboIncreased;
            }

            if (inputHandler != null)
            {
                inputHandler.OnAttackStarted -= HandleAttackInput;
            }
        }
    }
}