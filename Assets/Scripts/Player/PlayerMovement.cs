using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
   
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [HideInInspector] public float currentMoveSpeed;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    private bool canJump=true;

    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [Tooltip("If true, the player becomes invincible during the dash. This is controlled by SkillRegenSystem.")]
    [SerializeField] public bool canShadowDash = false;
    public bool canDash = true;

    private int jumpsLeft;
    private bool isDashing;
    private bool isDashOnCooldown = false;
    private float dashTimeRemaining;
    private Vector2 dashDirection;
    private Vector2 moveInput;
    private bool isFacingRight = true;

    private Rigidbody2D rb;
    private Animator animator; // Reference to the Animator
    private PlayerAttack playerAttack; // Reference to PlayerAttack script
    private PlayerHealth playerHealth; // Reference to PlayerHealth script

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>(); // Get Animator from child
        playerAttack = GetComponent<PlayerAttack>();   // Get reference to PlayerAttack script
        playerHealth = GetComponent<PlayerHealth>();   // Get reference to PlayerHealth script
    }

    private void Start()
    {
        ResetJumpCount();
        currentMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        HandleDash();
        Flip();
        UpdateAnimations(); // Update walking and jumping animations
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            ApplyMovement();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpsLeft > 0 && canJump)
        {
            Jump();
        }

        if (context.canceled && rb.linearVelocity.y > 0)
        {
            // Stop upward velocity instantly when jump button is released
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && canDash && !isDashOnCooldown)
        {
            Dash();
        }
    }

    private void Flip()
    {
        // If the player is attacking, don't allow flipping
        if (playerAttack != null && playerAttack.IsAttacking)
            return;

        if (moveInput.x == 0)
            return;

        bool shouldFaceRight = moveInput.x > 0;
        if (isFacingRight != shouldFaceRight)
        {
            isFacingRight = shouldFaceRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void ResetJumpCount()
    {
        jumpsLeft = maxJumps;
    }

    private void ApplyMovement()
    {
  
        rb.linearVelocity = new Vector2(moveInput.x * currentMoveSpeed, rb.linearVelocity.y);
    }

    public void DecreasedMovement()
    {
        currentMoveSpeed *= 0.5f;
        Debug.Log(currentMoveSpeed);
        canDash = false;
        canJump = false;
           
    }

    public void ResetMovement()
    {
            currentMoveSpeed = moveSpeed;
            canDash = true;
            canJump = true;
            jumpForce = 12f;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsLeft--;
    }

    private void Dash()
    {
        // Make the player invincible at the start of the dash if enabled
        if (canShadowDash)
        {
            playerHealth?.SetInvincibility(true);
        }

        isDashOnCooldown = true;
        isDashing = true;
        dashTimeRemaining = dashDuration;

        // Set dash direction based on move input, or facing direction if there's no input.
        float horizontalDirection = moveInput.x;
        if (Mathf.Approximately(horizontalDirection, 0f))
        {
            horizontalDirection = isFacingRight ? 1f : -1f;
        }
        dashDirection = new Vector2(horizontalDirection, 0).normalized;

        // Lock the Rigidbody's y-axis to prevent any vertical movement
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            // Calculate dash distance per frame
            float dashSpeed = dashDistance / dashDuration;
            Vector3 dashMovement = dashDirection * dashSpeed * Time.deltaTime;

            // Move the player directly using the Transform component
            transform.position += dashMovement;

            // Decrease the remaining dash time
            dashTimeRemaining -= Time.deltaTime;

            if (dashTimeRemaining <= 0)
            {
                EndDash();
            }
        }
    }

    private void EndDash()
    {
        // Make the player vulnerable again at the end of the dash if enabled
        if (canShadowDash)
        {
            playerHealth?.SetInvincibility(false);
        }

        isDashing = false;

        // Unlock the Rigidbody's y-axis to restore normal vertical movement
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        StartCoroutine(DashCooldownRoutine());
    }

    private System.Collections.IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        isDashOnCooldown = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            ResetJumpCount();
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        // Set isWalking based on horizontal movement
        animator.SetBool("isWalking", Mathf.Abs(moveInput.x) > 0);

        // Set isJumping based on vertical velocity
        animator.SetBool("isJumping", !IsGrounded());
    }

    private bool IsGrounded()
    {
        // Check if the player is grounded based on velocity
        return Mathf.Abs(rb.linearVelocity.y) < 0.01f;
    }
}