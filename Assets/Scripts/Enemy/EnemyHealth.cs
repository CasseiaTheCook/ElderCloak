using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float flashDuration = 0.1f; // Duration for the red flash effect

    private void Awake()
    {
        currentHealth = maxHealth;

        // Fetch the SpriteRenderer from the parent object
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {

        }
    }

    public void TakeDamage(int amount)
    {
        // Reduce health
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with the player's attack hitbox
        if (collision.CompareTag("HitBox"))
        {
            // Apply damage
            TakeDamage(1);

            // Apply knockback
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
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
}