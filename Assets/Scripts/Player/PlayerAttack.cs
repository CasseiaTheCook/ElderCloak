using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject attackHitbox; // Reference to the attack hitbox object

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.2f; // Duration for which the hitbox is active

    private bool isAttacking = false;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning("AttackHitbox is not assigned!");
            return;
        }

        // Activate the hitbox collider
        isAttacking = true;
        attackHitbox.GetComponent<Collider2D>().enabled = true;

        // Deactivate the hitbox after a short duration
        Invoke(nameof(ResetAttack), attackDuration);
    }

    private void ResetAttack()
    {
        attackHitbox.GetComponent<Collider2D>().enabled = false;
        isAttacking = false;
    }
}