using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthBarUI : MonoBehaviour
{
    private Image healthBarImage;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        healthBarImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Find the PlayerHealth component in the scene
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Subscribe to the health changed event
            playerHealth.OnHealthChanged += UpdateHealthBar;
        }
        else
        {
            Debug.LogError("HealthBarUI could not find PlayerHealth in the scene!", this);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    /// <summary>
    /// Updates the fill amount of the health bar image.
    /// </summary>
    /// <param name="currentHealth">Player's current health.</param>
    /// <param name="maxHealth">Player's maximum health.</param>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0)
        {
            // Calculate the fill amount (a value between 0 and 1)
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }
}
