using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ElderCloak.Input
{
    /// <summary>
    /// Centralized input handler using Unity's new Input System.
    /// Provides a clean interface between input actions and game systems.
    /// Future-proof design that can easily adapt to new input methods.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInputHandler", menuName = "ElderCloak/Input/Player Input Handler")]
    public class PlayerInputHandler : ScriptableObject, InputSystem_Actions.IPlayerActions
    {
        // Input events for movement and actions
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnJumpStarted;
        public event Action OnJumpCanceled;
        public event Action OnAttackStarted;
        public event Action OnDashStarted;
        public event Action OnSprintStarted;
        public event Action OnSprintCanceled;
        public event Action OnCrouchStarted;
        public event Action OnCrouchCanceled;
        public event Action OnInteractStarted;
        public event Action OnInteractCanceled;

        [Header("Input Settings")]
        [SerializeField] private bool enableInput = true;
        [SerializeField] private float moveDeadzone = 0.1f;
        [SerializeField] private float lookDeadzone = 0.1f;

        // Current input states
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprintHeld { get; private set; }
        public bool IsCrouchHeld { get; private set; }
        public bool IsInteractHeld { get; private set; }

        private InputSystem_Actions inputActions;

        /// <summary>
        /// Initialize the input handler with the input actions asset.
        /// </summary>
        /// <param name="inputActions">The input actions asset to use</param>
        public void Initialize(InputSystem_Actions inputActions)
        {
            this.inputActions = inputActions;
            if (inputActions != null)
            {
                inputActions.Player.SetCallbacks(this);
            }
        }

        /// <summary>
        /// Enable input processing.
        /// </summary>
        public void EnableInput()
        {
            enableInput = true;
            inputActions?.Player.Enable();
        }

        /// <summary>
        /// Disable input processing.
        /// </summary>
        public void DisableInput()
        {
            enableInput = false;
            inputActions?.Player.Disable();
        }

        /// <summary>
        /// Toggle input processing on/off.
        /// </summary>
        public void ToggleInput()
        {
            if (enableInput) DisableInput();
            else EnableInput();
        }

        #region IPlayerActions Implementation

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            Vector2 input = context.ReadValue<Vector2>();
            
            // Apply deadzone
            if (input.magnitude < moveDeadzone)
                input = Vector2.zero;

            MoveInput = input;
            OnMove?.Invoke(input);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            Vector2 input = context.ReadValue<Vector2>();
            
            // Apply deadzone
            if (input.magnitude < lookDeadzone)
                input = Vector2.zero;

            LookInput = input;
            OnLook?.Invoke(input);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                OnJumpStarted?.Invoke();
            }
            else if (context.canceled)
            {
                OnJumpCanceled?.Invoke();
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                OnAttackStarted?.Invoke();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                OnDashStarted?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                IsSprintHeld = true;
                OnSprintStarted?.Invoke();
            }
            else if (context.canceled)
            {
                IsSprintHeld = false;
                OnSprintCanceled?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                IsCrouchHeld = true;
                OnCrouchStarted?.Invoke();
            }
            else if (context.canceled)
            {
                IsCrouchHeld = false;
                OnCrouchCanceled?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            if (context.started)
            {
                IsInteractHeld = true;
                OnInteractStarted?.Invoke();
            }
            else if (context.canceled)
            {
                IsInteractHeld = false;
                OnInteractCanceled?.Invoke();
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            // Implementation for previous action if needed
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            // Implementation for next action if needed
        }

        #endregion

        /// <summary>
        /// Get the current movement direction as a normalized vector.
        /// </summary>
        /// <returns>Normalized movement direction</returns>
        public Vector2 GetMovementDirection()
        {
            return MoveInput.normalized;
        }

        /// <summary>
        /// Get the raw movement input magnitude.
        /// </summary>
        /// <returns>Movement input magnitude (0-1)</returns>
        public float GetMovementMagnitude()
        {
            return MoveInput.magnitude;
        }

        /// <summary>
        /// Check if movement input is being provided.
        /// </summary>
        /// <returns>True if there's movement input</returns>
        public bool IsMoving()
        {
            return MoveInput.magnitude > moveDeadzone;
        }

        /// <summary>
        /// Get horizontal movement input (-1 to 1).
        /// </summary>
        /// <returns>Horizontal input value</returns>
        public float GetHorizontalInput()
        {
            return MoveInput.x;
        }

        /// <summary>
        /// Get vertical movement input (-1 to 1).
        /// </summary>
        /// <returns>Vertical input value</returns>
        public float GetVerticalInput()
        {
            return MoveInput.y;
        }

        /// <summary>
        /// Update deadzone settings at runtime.
        /// </summary>
        /// <param name="moveDeadzone">Movement input deadzone</param>
        /// <param name="lookDeadzone">Look input deadzone</param>
        public void UpdateDeadzones(float moveDeadzone, float lookDeadzone)
        {
            this.moveDeadzone = Mathf.Clamp01(moveDeadzone);
            this.lookDeadzone = Mathf.Clamp01(lookDeadzone);
        }

        private void OnDestroy()
        {
            inputActions?.Player.RemoveCallbacks(this);
        }
    }
}