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
    private bool canJump = true;

    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashStaminaCost = 25f;
    [Tooltip("If true, the player becomes invincible during the dash. This is controlled by SkillRegenSystem.")]
    [SerializeField] public bool canShadowDash = false;
    private bool canDash = true; // Only for cooldown/exhausted state

    // NEW: Ability unlock variables
    public bool hasDashAbility = false;
    public bool hasDoubleJumpAbility = false;
    public bool hasRunAbility = true;

    private int jumpsLeft;
    private bool isDashing;
    private bool isDashOnCooldown = false;
    private float dashTimeRemaining;
    private Vector2 dashDirection;
    private Vector2 moveInput;
    private bool isFacingRight = true;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerAttack playerAttack;
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;

    private SaveDataManager saveManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
        playerHealth = GetComponent<PlayerHealth>();
        playerStamina = GetComponent<PlayerStamina>();
        saveManager = FindFirstObjectByType<SaveDataManager>();
        if (saveManager != null && saveManager.currentSave != null)
        {
            // Dash yeteneði
            hasDashAbility = saveManager.currentSave.canDash;
            // Çift zýplama yeteneði
            hasDoubleJumpAbility = saveManager.currentSave.canDoubleJump;
            maxJumps = hasDoubleJumpAbility ? 2 : 1;
            // Koþma yeteneði
            hasRunAbility = saveManager.currentSave.canRun;
            currentMoveSpeed = hasRunAbility ? moveSpeed : 0f;
        }
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
        UpdateAnimations();
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
        // Only allow jumping if you have jump ability
        if (context.performed && jumpsLeft > 0 && canJump)
        {
            Jump();
        }

        if (context.canceled && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        // Only allow dash if player has unlocked dash ability
        if (context.performed && hasDashAbility && !isDashing && canDash && !isDashOnCooldown && playerStamina.HasEnoughStamina(1f))
        {
            Dash();
        }
    }

    private void Flip()
    {
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
        float staminaToConsume = Mathf.Min(dashStaminaCost, playerStamina.CurrentStamina);
        playerStamina.ConsumeStamina(staminaToConsume);

        if (canShadowDash)
        {
            playerHealth?.SetInvincibility(true);
        }

        isDashOnCooldown = true;
        isDashing = true;
        dashTimeRemaining = dashDuration;

        float horizontalDirection = moveInput.x;
        if (Mathf.Approximately(horizontalDirection, 0f))
        {
            horizontalDirection = isFacingRight ? 1f : -1f;
        }
        dashDirection = new Vector2(horizontalDirection, 0).normalized;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            float dashSpeed = dashDistance / dashDuration;
            Vector3 dashMovement = dashDirection * dashSpeed * Time.deltaTime;
            transform.position += dashMovement;
            dashTimeRemaining -= Time.deltaTime;

            if (dashTimeRemaining <= 0)
            {
                EndDash();
            }
        }
    }

    private void EndDash()
    {
        if (canShadowDash)
        {
            playerHealth?.SetInvincibility(false);
        }

        isDashing = false;
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
        animator.SetBool("isWalking", Mathf.Abs(moveInput.x) > 0);
        animator.SetBool("isJumping", !IsGrounded());
    }

    private bool IsGrounded()
    {
        return Mathf.Abs(rb.linearVelocity.y) < 0.01f;
    }

    private void SetExhaustedState(bool isExhausted)
    {
        // Only disables dash on exhaustion/cooldown, NOT ability unlock!
        canDash = !isExhausted;
    }
}