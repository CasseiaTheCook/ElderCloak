using UnityEngine;

namespace ElderCloak.Player.Movement
{
    [System.Serializable]
    public class DashSystem
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private int maxDashes = 1;
        
        [Header("Dash Physics")]
        [SerializeField] private bool resetVelocityOnDash = true;
        [SerializeField] private bool disableGravityDuringDash = true;
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        private int dashCount;
        private float dashTimer;
        private float cooldownTimer;
        private Vector2 dashDirection;
        private float originalGravityScale;
        
        private Rigidbody2D rb;
        private bool wasGrounded;
        
        public bool IsDashing { get; private set; }
        public bool CanDash => dashCount < maxDashes && cooldownTimer <= 0 && !IsDashing;
        public int RemainingDashes => Mathf.Max(0, maxDashes - dashCount);
        public float CooldownProgress => 1f - (cooldownTimer / dashCooldown);
        
        public void Initialize(Rigidbody2D rigidbody)
        {
            rb = rigidbody;
            originalGravityScale = rb.gravityScale;
            dashCount = 0;
            cooldownTimer = 0;
        }
        
        public void Update()
        {
            UpdateTimers();
            UpdateDash();
        }
        
        private void UpdateTimers()
        {
            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;
            
            if (IsDashing)
                dashTimer -= Time.deltaTime;
        }
        
        private void UpdateDash()
        {
            if (IsDashing)
            {
                if (dashTimer <= 0)
                {
                    EndDash();
                }
                else
                {
                    float progress = 1f - (dashTimer / dashDuration);
                    float curveValue = dashCurve.Evaluate(progress);
                    Vector2 dashVelocity = dashDirection * dashForce * curveValue;
                    rb.linearVelocity = dashVelocity;
                }
            }
        }
        
        public bool TryDash(Vector2 inputDirection, Vector2 playerFacingDirection)
        {
            if (!CanDash)
                return false;
            
            // Determine dash direction
            Vector2 direction = GetDashDirection(inputDirection, playerFacingDirection);
            
            StartDash(direction);
            return true;
        }
        
        private Vector2 GetDashDirection(Vector2 inputDirection, Vector2 facingDirection)
        {
            // If there's input, dash in input direction
            if (inputDirection.magnitude > 0.1f)
            {
                return inputDirection.normalized;
            }
            
            // If no input, dash in facing direction
            return facingDirection.normalized;
        }
        
        private void StartDash(Vector2 direction)
        {
            IsDashing = true;
            dashDirection = direction;
            dashTimer = dashDuration;
            dashCount++;
            cooldownTimer = dashCooldown;
            
            if (resetVelocityOnDash)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            if (disableGravityDuringDash)
            {
                rb.gravityScale = 0f;
            }
            
            Debug.Log($"Started dash in direction: {direction}. Remaining dashes: {RemainingDashes}");
        }
        
        private void EndDash()
        {
            IsDashing = false;
            
            if (disableGravityDuringDash)
            {
                rb.gravityScale = originalGravityScale;
            }
            
            // Reduce velocity after dash to prevent flying off
            rb.linearVelocity *= 0.5f;
            
            Debug.Log("Dash ended");
        }
        
        public void OnGroundedChanged(bool isGrounded)
        {
            if (isGrounded && !wasGrounded)
            {
                // Reset dashes when landing
                ResetDashes();
            }
            wasGrounded = isGrounded;
        }
        
        public void ResetDashes()
        {
            dashCount = 0;
            cooldownTimer = 0;
        }
        
        public void ForceCancelDash()
        {
            if (IsDashing)
            {
                EndDash();
            }
        }
    }
}