using UnityEngine;
using UnityEngine.InputSystem;

namespace ElderCloak.Player.Input
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private InputAction attackAction;
        
        // Input Values
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool DashPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        
        private void Awake()
        {
            SetupInputActions();
        }
        
        private void SetupInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("Input Actions asset is not assigned!");
                return;
            }
            
            playerMap = inputActions.FindActionMap("Player");
            
            moveAction = playerMap.FindAction("Move");
            jumpAction = playerMap.FindAction("Jump");
            dashAction = playerMap.FindAction("Dash");
            attackAction = playerMap.FindAction("Attack");
            
            // Bind callbacks
            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;
            dashAction.performed += OnDashPerformed;
            attackAction.performed += OnAttackPerformed;
        }
        
        private void Update()
        {
            MoveInput = moveAction.ReadValue<Vector2>();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            JumpPressed = true;
            JumpHeld = true;
        }
        
        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            JumpHeld = false;
        }
        
        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            DashPressed = true;
        }
        
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed = true;
        }
        
        private void LateUpdate()
        {
            // Reset one-frame inputs
            JumpPressed = false;
            DashPressed = false;
            AttackPressed = false;
        }
        
        private void OnEnable()
        {
            inputActions?.Enable();
        }
        
        private void OnDisable()
        {
            inputActions?.Disable();
        }
        
        private void OnDestroy()
        {
            // Unbind callbacks
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
                jumpAction.canceled -= OnJumpCanceled;
            }
            if (dashAction != null)
                dashAction.performed -= OnDashPerformed;
            if (attackAction != null)
                attackAction.performed -= OnAttackPerformed;
        }
    }
}