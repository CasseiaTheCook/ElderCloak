using UnityEngine;
using ElderCloak.Input;
using ElderCloak.Player;

namespace ElderCloak.Systems
{
    /// <summary>
    /// Main input manager that coordinates input handling across all systems.
    /// Creates and manages the input actions asset and distributes input to relevant controllers.
    /// This provides a centralized point for input management and future extensibility.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private bool enableInputOnStart = true;
        [SerializeField] private bool persistBetweenScenes = true;

        [Header("Connected Controllers")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerCombatController playerCombatController;

        // Input system reference
        private InputSystem_Actions inputActions;

        // Singleton pattern for global access
        public static InputManager Instance { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                
                if (persistBetweenScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeInputSystem();
        }

        private void Start()
        {
            SetupControllerConnections();
            
            if (enableInputOnStart)
            {
                EnableInput();
            }
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            CleanupInputSystem();
        }

        #endregion

        #region Input System Setup

        /// <summary>
        /// Initialize the input system and input handler.
        /// </summary>
        private void InitializeInputSystem()
        {
            // Create input actions instance
            inputActions = new InputSystem_Actions();

            // Create input handler if not assigned
            if (inputHandler == null)
            {
                inputHandler = ScriptableObject.CreateInstance<PlayerInputHandler>();
            }

            // Initialize the input handler with our input actions
            inputHandler.Initialize(inputActions);
        }

        /// <summary>
        /// Set up connections between input handler and controllers.
        /// </summary>
        private void SetupControllerConnections()
        {
            // Find controllers if not assigned
            if (playerController == null)
                playerController = FindObjectOfType<PlayerController>();
            
            if (playerCombatController == null)
                playerCombatController = FindObjectOfType<PlayerCombatController>();

            // Connect input handler to controllers
            if (playerController != null)
            {
                playerController.SetInputHandler(inputHandler);
            }

            if (playerCombatController != null)
            {
                playerCombatController.SetInputHandler(inputHandler);
            }
        }

        /// <summary>
        /// Clean up input system resources.
        /// </summary>
        private void CleanupInputSystem()
        {
            if (inputActions != null)
            {
                inputActions.Dispose();
                inputActions = null;
            }
        }

        #endregion

        #region Input Control

        /// <summary>
        /// Enable input processing.
        /// </summary>
        public void EnableInput()
        {
            if (inputHandler != null)
            {
                inputHandler.EnableInput();
            }
        }

        /// <summary>
        /// Disable input processing.
        /// </summary>
        public void DisableInput()
        {
            if (inputHandler != null)
            {
                inputHandler.DisableInput();
            }
        }

        /// <summary>
        /// Toggle input processing on/off.
        /// </summary>
        public void ToggleInput()
        {
            if (inputHandler != null)
            {
                inputHandler.ToggleInput();
            }
        }

        /// <summary>
        /// Check if input is currently enabled.
        /// </summary>
        /// <returns>True if input is enabled</returns>
        public bool IsInputEnabled()
        {
            return inputActions != null && inputActions.Player.enabled;
        }

        #endregion

        #region Controller Management

        /// <summary>
        /// Register a player controller with the input system.
        /// </summary>
        /// <param name="controller">The player controller to register</param>
        public void RegisterPlayerController(PlayerController controller)
        {
            if (controller != null && inputHandler != null)
            {
                playerController = controller;
                controller.SetInputHandler(inputHandler);
            }
        }

        /// <summary>
        /// Register a player combat controller with the input system.
        /// </summary>
        /// <param name="controller">The combat controller to register</param>
        public void RegisterCombatController(PlayerCombatController controller)
        {
            if (controller != null && inputHandler != null)
            {
                playerCombatController = controller;
                controller.SetInputHandler(inputHandler);
            }
        }

        /// <summary>
        /// Unregister a player controller.
        /// </summary>
        public void UnregisterPlayerController()
        {
            if (playerController != null)
            {
                playerController.SetInputHandler(null);
                playerController = null;
            }
        }

        /// <summary>
        /// Unregister a combat controller.
        /// </summary>
        public void UnregisterCombatController()
        {
            if (playerCombatController != null)
            {
                playerCombatController.SetInputHandler(null);
                playerCombatController = null;
            }
        }

        #endregion

        #region Input State Access

        /// <summary>
        /// Get the current movement input.
        /// </summary>
        /// <returns>Movement input vector</returns>
        public Vector2 GetMovementInput()
        {
            return inputHandler?.MoveInput ?? Vector2.zero;
        }

        /// <summary>
        /// Get the current look input.
        /// </summary>
        /// <returns>Look input vector</returns>
        public Vector2 GetLookInput()
        {
            return inputHandler?.LookInput ?? Vector2.zero;
        }

        /// <summary>
        /// Check if sprint is being held.
        /// </summary>
        /// <returns>True if sprint is held</returns>
        public bool IsSprintHeld()
        {
            return inputHandler?.IsSprintHeld ?? false;
        }

        /// <summary>
        /// Check if crouch is being held.
        /// </summary>
        /// <returns>True if crouch is held</returns>
        public bool IsCrouchHeld()
        {
            return inputHandler?.IsCrouchHeld ?? false;
        }

        /// <summary>
        /// Check if interact is being held.
        /// </summary>
        /// <returns>True if interact is held</returns>
        public bool IsInteractHeld()
        {
            return inputHandler?.IsInteractHeld ?? false;
        }

        #endregion

        #region Input Settings

        /// <summary>
        /// Update input deadzone settings.
        /// </summary>
        /// <param name="moveDeadzone">Movement deadzone (0-1)</param>
        /// <param name="lookDeadzone">Look deadzone (0-1)</param>
        public void UpdateInputSettings(float moveDeadzone, float lookDeadzone)
        {
            if (inputHandler != null)
            {
                inputHandler.UpdateDeadzones(moveDeadzone, lookDeadzone);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get the input handler for direct access.
        /// </summary>
        /// <returns>The player input handler</returns>
        public PlayerInputHandler GetInputHandler()
        {
            return inputHandler;
        }

        /// <summary>
        /// Get the input actions asset for direct access.
        /// </summary>
        /// <returns>The input actions asset</returns>
        public InputSystem_Actions GetInputActions()
        {
            return inputActions;
        }

        /// <summary>
        /// Get the current player controller.
        /// </summary>
        /// <returns>The player controller</returns>
        public PlayerController GetPlayerController()
        {
            return playerController;
        }

        /// <summary>
        /// Get the current combat controller.
        /// </summary>
        /// <returns>The combat controller</returns>
        public PlayerCombatController GetCombatController()
        {
            return playerCombatController;
        }

        #endregion

        #region Debug Info

#if UNITY_EDITOR
        [Header("Debug Info (Runtime Only)")]
        [SerializeField, ReadOnly] private bool debugInputEnabled;
        [SerializeField, ReadOnly] private Vector2 debugMovementInput;
        [SerializeField, ReadOnly] private bool debugSprintHeld;
        [SerializeField, ReadOnly] private bool debugCrouchHeld;

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                debugInputEnabled = IsInputEnabled();
                debugMovementInput = GetMovementInput();
                debugSprintHeld = IsSprintHeld();
                debugCrouchHeld = IsCrouchHeld();
            }
        }

        /// <summary>
        /// Custom attribute for read-only fields in inspector.
        /// </summary>
        public class ReadOnlyAttribute : PropertyAttribute { }
#endif

        #endregion
    }
}