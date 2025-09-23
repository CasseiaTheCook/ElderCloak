using UnityEngine;
using UnityEngine.InputSystem;
using ElderCloak.Combat;
using ElderCloak.Interfaces;

namespace ElderCloak.Player
{
    /// <summary>
    /// Handles player combat input and integrates with the melee attack system
    /// Works in conjunction with PlayerController for movement and MeleeAttackSystem for attacks
    /// </summary>
    [RequireComponent(typeof(MeleeAttackSystem))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private bool enableCombatDuringMovement = true;
        [SerializeField] private bool stopMovementDuringAttack = false;
        
        // Components
        private MeleeAttackSystem attackSystem;
        private PlayerController playerController;
        
        // Input state
        private bool attackPressed;
        
        /// <summary>
        /// Reference to the melee attack system
        /// </summary>
        public MeleeAttackSystem AttackSystem => attackSystem;
        
        /// <summary>
        /// Whether the player can currently attack
        /// </summary>
        public bool CanAttack => attackSystem.CanAttack && (enableCombatDuringMovement || CanAttackBasedOnMovement());
        
        private void Awake()
        {
            // Get components
            attackSystem = GetComponent<MeleeAttackSystem>();
            playerController = GetComponent<PlayerController>();
        }
        
        private void Update()
        {
            HandleCombatInput();
            HandleCombatLogic();
        }
        
        /// <summary>
        /// Handle combat-related input
        /// </summary>
        private void HandleCombatInput()
        {
            // Read attack input (Left Mouse Button or Enter key)
            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Return))
                attackPressed = true;
        }
        
        /// <summary>
        /// Handle combat logic and interactions
        /// </summary>
        private void HandleCombatLogic()
        {
            // Process attack input
            if (attackPressed && CanAttack)
            {
                PerformAttack();
            }
            
            attackPressed = false;
        }
        
        /// <summary>
        /// Execute an attack
        /// </summary>
        private void PerformAttack()
        {
            attackSystem.PerformAttack();
        }
        
        /// <summary>
        /// Determine if we can attack based on movement state
        /// </summary>
        /// <returns>True if movement state allows attacking</returns>
        private bool CanAttackBasedOnMovement()
        {
            if (playerController == null) return true;
            
            // Don't attack during dash (unless specifically allowed)
            if (playerController.IsDashing)
                return false;
            
            return true;
        }
        
        #region Input Event Handlers (for Unity Events)
        /// <summary>
        /// Handle attack input through Unity Events
        /// </summary>
        /// <param name="context">Input action context</param>
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                attackPressed = true;
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Force cancel the current attack
        /// </summary>
        public void CancelAttack()
        {
            attackSystem.CancelAttack();
        }
        
        /// <summary>
        /// Set whether combat is enabled during movement
        /// </summary>
        /// <param name="enabled">Whether to enable combat during movement</param>
        public void SetCombatDuringMovementEnabled(bool enabled)
        {
            enableCombatDuringMovement = enabled;
        }
        
        /// <summary>
        /// Set whether movement should stop during attacks
        /// </summary>
        /// <param name="stopMovement">Whether to stop movement during attacks</param>
        public void SetStopMovementDuringAttack(bool stopMovement)
        {
            stopMovementDuringAttack = stopMovement;
        }
        #endregion
    }
}