using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.1f; // Duration of the knockback effect
    public float flashDuration = 0.1f; // Duration for the red flash effect

    private bool isKnockedBack = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        // Fetch the SpriteRenderer from the child object
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on child!");
        }
    }

    public void TakeDamage(int amount)
    {
        // Reduce health
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");

        // Trigger default knockback effect (optional)
        Vector2 defaultKnockbackDirection = Vector2.zero; // Default to no knockback
        StartCoroutine(ApplyKnockback(defaultKnockbackDirection));

        // Trigger red flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }

        // Check if the enemy is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died!");
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashRed()
    {
        // Change color to red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // Wait for the flash duration
        yield return new WaitForSeconds(flashDuration);

        // Revert to the original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // Assuming white is the default color
        }
    }

    private System.Collections.IEnumerator ApplyKnockback(Vector2 knockbackDirection)
    {
        isKnockedBack = true;

        // Calculate the knockback target position
        Vector3 originalPosition = transform.position;
        Vector3 knockbackPosition = originalPosition + (Vector3)knockbackDirection.normalized * knockbackForce;

        float elapsedTime = 0f;

        // Smoothly move the enemy to the knockback position
        while (elapsedTime < knockbackDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, knockbackPosition, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the enemy ends at the exact knockback position
        transform.position = knockbackPosition;

        isKnockedBack = false;
    }
}