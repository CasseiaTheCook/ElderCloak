using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;

    private int currentHealth;

    private void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        // Reduce health
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");

        // Check if the enemy is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 knockbackDirection)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died!");

        // Play death animation or effects (optional)
        // Example: Animator.SetTrigger("Die");

        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}