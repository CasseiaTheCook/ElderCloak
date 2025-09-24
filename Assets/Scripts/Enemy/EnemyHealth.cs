using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;

    [Header("Visual Settings")]
    public float flashDuration = 0.1f; // Duration for the red flash effect

    [Header("Knockback Settings")]
    public float knockbackDistance = 0.5f; // Distance to knock back
    public float knockbackDuration = 0.1f; // Duration of the knockback effect

    private bool isKnockedBack = false; // Flag to prevent overlapping knockbacks

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

    public void TakeDamage(float amount, Vector2 knockbackPosition)
    {
        // Reduce health
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");

        // Trigger red flash effect
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }

        // Trigger knockback effect
        if (!isKnockedBack)
        {
            // Update knockback from player's position instead of the hitbox
            Vector2 playerPosition = FindFirstObjectByType<PlayerMovement>().transform.position;
            StartCoroutine(ApplyKnockback(playerPosition));
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

    private System.Collections.IEnumerator ApplyKnockback(Vector2 playerPosition)
    {
        isKnockedBack = true;

        // Determine knockback direction (away from the player)
        Vector3 knockbackDirection = (transform.position - (Vector3)playerPosition).normalized;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + knockbackDirection * knockbackDistance;

        float elapsedTime = 0f;

        // Move towards the target position smoothly over the knockback duration
        while (elapsedTime < knockbackDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is set
        transform.position = targetPosition;

        yield return new WaitForSeconds(flashDuration);

        // Reset the knockback flag
        isKnockedBack = false;
    }
}