// Author: Copilot
// Version: 2.0
// Purpose: Manages player's health on the Root object while animations affect the child object.

using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 5;

    [Header("Damage Feedback")]
    public float iFrameDuration = 1.5f;
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
        if (isInvincible) return;

        currentHealth -= amount;
        StartCoroutine(HandleIFrames());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator HandleIFrames()
    {
        isInvincible = true;

        // Change the sprite color to indicate damage
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
        }

        yield return new WaitForSeconds(iFrameDuration);

        // Revert to the original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
    }
}