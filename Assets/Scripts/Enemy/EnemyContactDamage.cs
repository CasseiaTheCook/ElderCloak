using UnityEngine;

public class EnemyContactDamage : MonoBehaviour, IDamageable
{
    [Header("Damage Settings")]
    public int damage = 1;

    [Header("Damage Feedback")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;

    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
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
        StartCoroutine(HandleDamageFeedback());
    }

    private System.Collections.IEnumerator HandleDamageFeedback()
    {
        // Flash the sprite to indicate damage
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // Revert to the original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}