using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HealthTextUI : MonoBehaviour
{
    private TextMeshProUGUI healthText;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        healthText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Sahnedeki PlayerHealth component'ini bul
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Can değiştiğinde tetiklenecek olan event'e abone ol
            playerHealth.OnHealthChanged += UpdateHealthText;

            // Set the initial text value immediately without waiting for an event.
            UpdateHealthText(playerHealth.CurrentHealthForUI, playerHealth.maxHealth);
        }
        else
        {
            Debug.LogError("HealthTextUI, sahnede PlayerHealth bulamadı!", this);
        }
    }

    private void OnDestroy()
    {
        // Obje yok olduğunda event aboneliğini iptal et (hafıza sızıntılarını önler)
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthText;
        }
    }

    /// <summary>
    /// Can metnini günceller.
    /// </summary>
    /// <param name="currentHealth">Mevcut can.</param>
    /// <param name="maxHealth">Maksimum can.</param>
    private void UpdateHealthText(float currentHealth, float maxHealth)
    {
        // Küsuratlı sayıları yuvarlayarak daha temiz bir görüntü elde edelim
        healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }
}
