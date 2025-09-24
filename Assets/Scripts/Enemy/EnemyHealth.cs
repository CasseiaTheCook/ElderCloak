using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    private int currentHealth;
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

    public void TakeDamage(int amount, Vector2 knockbackPosition)
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
            StartCoroutine(ApplyKnockback(knockbackPosition));
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

    private System.Collections.IEnumerator ApplyKnockback(Vector2 knockbackPosition)
    {
        isKnockedBack = true;

        // Determine knockback direction (away from the attacker)
        Vector3 knockbackDirection = (transform.position - (Vector3)knockbackPosition).normalized;
        Vector3 startPosition = transform.position;
        
        // Check if enemy is grounded to modify knockback behavior
        IGroundDetection groundDetection = GetComponent<IGroundDetection>();
        bool isGrounded = groundDetection != null && groundDetection.IsGrounded();
        
        Vector3 targetPosition;
        if (isGrounded)
        {
            // Only apply horizontal knockback if grounded
            knockbackDirection.y = 0;
            knockbackDirection = knockbackDirection.normalized;
            
            // Handle edge case where knockback is purely vertical (no horizontal component)
            if (knockbackDirection.magnitude < 0.1f)
            {
                // Use a random horizontal direction if no horizontal knockback exists
                knockbackDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f) > 0 ? 1f : -1f, 0, 0);
            }
            
            targetPosition = startPosition + knockbackDirection * knockbackDistance;
        }
        else
        {
            // Apply full knockback if airborne
            targetPosition = startPosition + knockbackDirection * knockbackDistance;
        }

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