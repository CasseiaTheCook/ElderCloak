using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 5;

    [Header("Damage Feedback")]
    public float iFrameDuration = 1.5f; // Duration for invulnerability
    public Color damageColor = Color.red;

    [Header("Knockback Settings")]
    public float knockbackDistance = 1f; // Distance to knock back
    public float knockbackDuration = 0.2f; // Duration of the knockback effect

    private float currentHealth;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;
    private PlayerParry playerParry; // Reference to the parry script

    public event Action<float, float> OnHealthChanged;

    public bool IsAtMaxHealth => currentHealth >= maxHealth;
    public float CurrentHealthForUI => currentHealth; // Property to expose current health for UI

    private void Awake()
    {
        currentHealth = maxHealth;
        playerParry = GetComponent<PlayerParry>();

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

    private void Start()
    {
        // Trigger the event on start to set the initial health bar value
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount, Vector2 knockbackPosition)
    {
        // --- PARRY CHECK ---
        // If the player is currently in a parry window, block the damage.
        if (playerParry != null && playerParry.IsParrying)
        {
            playerParry.OnSuccessfulParry();
            // We can try to stun the attacker here in the future.
            // For now, just block the damage.
            return; 
        }

        if (isInvincible) return; // Ignore damage while invincible

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Trigger invincibility frames
        StartCoroutine(HandleIFrames());


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
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        // Optional: Add a healing visual/sound effect here
    }

    /// <summary>
    /// Heals the player by a percentage of their maximum health.
    /// </summary>
    /// <param name="percentage">The percentage to heal (e.g., 30 for 30%).</param>
    public void HealPercentage(float percentage)
    {
        float amountToHeal = maxHealth * (percentage / 100f);
        currentHealth += amountToHeal;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player healed for {amountToHeal} ({percentage}%). Current health: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        gameObject.SetActive(false); // Disable the player object
    }

    private System.Collections.IEnumerator HandleIFrames()
    {
        // Start invincibility with visual feedback
        SetInvincibility(true, true);

        // Wait for the duration of invincibility
        yield return new WaitForSeconds(iFrameDuration);

        // End invincibility and revert visuals
        SetInvincibility(false, true);
    }

    /// <summary>
    /// Sets the player's invincibility state.
    /// </summary>
    /// <param name="isInvincible">True to make the player invincible, false to make them vulnerable.</param>
    /// <param name="applyVisuals">If true, applies the damage color feedback.</param>
    public void SetInvincibility(bool isInvincible, bool applyVisuals = false)
    {
        this.isInvincible = isInvincible;
        Physics2D.IgnoreLayerCollision(3, 7, isInvincible); // Player = Layer 3, Enemy = Layer 7

        if (spriteRenderer != null)
        {
            spriteRenderer.color = (isInvincible && applyVisuals) ? damageColor : originalColor;
        }
    }
}