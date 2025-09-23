using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 5;

    [Header("Damage Feedback")]
    public float iFrameDuration = 1.5f; // Duration for invulnerability
    public Color damageColor = Color.red;

    private int currentHealth;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;

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

    public void TakeDamage(int amount)
    {
        if (isInvincible) return; // Ignore damage while invincible

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}");

        // Trigger invincibility frames
        StartCoroutine(HandleIFrames());

        // Check if the player is dead
        if (currentHealth <= 0)
        {
            Die();
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If invincible, ignore push behavior by canceling physics force
        if (isInvincible && collision.gameObject.layer == 7) // Enemy Layer
        {
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = Vector2.zero; // Stop enemy movement caused by collision
            }
        }
    }
}