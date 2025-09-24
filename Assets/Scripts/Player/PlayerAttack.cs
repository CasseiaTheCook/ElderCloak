using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private HealthRegenSystem healthRegenSystem;
    private Animator animator; // Reference to the Animator

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.2f;

    private Collider2D attackCollider;
    private bool isAttacking = false;

    private void Awake()
    {
        if (healthRegenSystem == null)
            healthRegenSystem = GetComponent<HealthRegenSystem>();

        if (attackHitbox != null)
            attackCollider = attackHitbox.GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(); // Get Animator from child if not assigned
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is not assigned on PlayerAttack script!");
            return;
        }

        isAttacking = true;

        // Trigger attack animation
        animator.SetTrigger("Attack"); // Correctly using Trigger for Attack
    }

    // Called via Animation Event or at the end of the attack sequence
    public void EndAttack()
    {
        isAttacking = false;
    }

    public void OnSuccessfulHit()
    {
        healthRegenSystem?.AddFill();
    }


}