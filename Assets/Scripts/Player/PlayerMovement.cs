// Author: Copilot
// Version: 1.2
// Purpose: Handles player movement, including walking, jumping, and dashing.

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Dash Settings")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;

    private int jumpsLeft;
    private bool isDashing;
    private float dashTime;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ResetJumpCount();
    }

    private void Update()
    {
        HandleDashTimer();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("Move action triggered");
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && jumpsLeft > 0)
        {
            Debug.Log("Jump action triggered");
            Jump();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing)
        {
            Debug.Log("Dash action triggered");
            Dash();
        }
    }

    private void ResetJumpCount()
    {
        jumpsLeft = maxJumps;
    }

    private void HandleDashTimer()
    {
        if (isDashing)
        {
            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                EndDash();
            }
        }
    }

    private void ApplyMovement()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsLeft--;
    }

    private void Dash()
    {
        isDashing = true;
        dashTime = dashDuration;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(moveInput.x * dashForce, 0);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 3f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            ResetJumpCount();
        }
    }
}