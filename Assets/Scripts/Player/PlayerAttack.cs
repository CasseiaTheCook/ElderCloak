using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;

    [Header("References")]
    [SerializeField] private GameObject attackHitbox; // Reference to the attack hitbox object

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
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

        // Detect enemies within the attack hitbox
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackHitbox.transform.position,
            attackHitbox.GetComponent<BoxCollider2D>().size,
            0,
            enemyLayer
        );

        foreach (Collider2D enemy in hits)
        {
            // Check if the enemy implements the IDamageable interface
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Apply damage
                damageable.TakeDamage(attackDamage);
            }

            // Apply knockback to the enemy
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyHealth.ApplyKnockback(knockbackDirection);
            }
        }
    }
}