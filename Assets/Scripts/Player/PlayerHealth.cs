using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 5;

    [Header("Damage Feedback")]
    public float iFrameDuration = 1.5f; // Duration for invulnerability
    public Color damageColor = Color.red;

    [Header("Knockback Settings")]
    public float knockbackDistance = 1f; // Distance to knock back
    public float knockbackDuration = 0.2f; // Duration of the knockback effect

    private int currentHealth;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;
    private bool isKnockedBack = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        // Fetch the SpriteRenderer from the child object
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found! Ensure the child object has a SpriteRenderer.");
        }
    }

    public void TakeDamage(int amount, Vector2 knockbackPosition)
    {
        if (isInvincible) return; // Ignore damage while invincible

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}");

        // Trigger invincibility frames
        StartCoroutine(HandleIFrames());

        // Trigger knockback effect
        if (!isKnockedBack)
        {
            StartCoroutine(ApplyKnockback(knockbackPosition));
        }

        // Check if the player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player healed for {amount}. Current health: {currentHealth}");
        // Optional: Add a healing visual/sound effect here
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        gameObject.SetActive(false); // Disable the player object
    }

    private System.Collections.IEnumerator HandleIFrames()
    {
        isInvincible = true;

        // Ignore collisions between the player and enemies immediately
        Physics2D.IgnoreLayerCollision(3, 7, true); // Player = Layer 3, Enemy = Layer 7

        // Change the sprite color to indicate invincibility
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
        }

        // Wait for the duration of invincibility
        yield return new WaitForSeconds(iFrameDuration);

        // Re-enable collisions between the player and enemies
        Physics2D.IgnoreLayerCollision(3, 7, false);

        // Revert the sprite color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
    }

    private System.Collections.IEnumerator ApplyKnockback(Vector2 knockbackPosition)
    {
        isKnockedBack = true;

        // Determine knockback direction (away from the attacker)
        Vector3 knockbackDirection = (transform.position - (Vector3)knockbackPosition).normalized;
        Vector3 startPosition = transform.position;
        
        // Check if player is grounded to modify knockback behavior
        IGroundDetection groundDetection = GetComponent<IGroundDetection>();
        bool isGrounded = groundDetection != null && groundDetection.IsGrounded();
        
        Vector3 targetPosition;
        if (isGrounded)
        {
            // Only apply horizontal knockback if grounded
            knockbackDirection.y = 0;
            knockbackDirection = knockbackDirection.normalized;
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

        yield return new WaitForSeconds(0.1f);

        // Reset the knockback flag
        isKnockedBack = false;
    }
}