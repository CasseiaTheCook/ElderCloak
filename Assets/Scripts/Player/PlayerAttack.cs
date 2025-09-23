using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug Settings")]
    [SerializeField] private bool showAttackRangeGizmo = true;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // Detect enemies within the attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

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

    private void OnDrawGizmosSelected()
    {
        // Visualize the attack range in the editor
        if (showAttackRangeGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}