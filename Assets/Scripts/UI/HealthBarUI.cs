using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class HealthBarUI : MonoBehaviour
{
    [Header("Main Bar")]
    [Tooltip("How quickly the health bar animates to the target value.")]
    [SerializeField] private float animationSpeed = 0.5f;

    [Header("Delayed Bar (Damage Feedback)")]
    [SerializeField] private Image delayedHealthBarImage;
    [SerializeField] private float delayBeforeDrain = 0.75f;
    [SerializeField] private float drainSpeed = 0.25f;

    private Image healthBarImage;
    private PlayerHealth playerHealth;

    private Coroutine healthAnimationCoroutine;
    private Coroutine delayedHealthAnimationCoroutine;

    private void Awake()
    {
        healthBarImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Find the PlayerHealth component in the scene
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Subscribe to the health changed event
            playerHealth.OnHealthChanged += UpdateHealthBar;

            // Set the initial health bar value instantly without animation
            healthBarImage.fillAmount = playerHealth.CurrentHealthForUI / playerHealth.maxHealth;
            if (delayedHealthBarImage != null)
            {
                delayedHealthBarImage.fillAmount = healthBarImage.fillAmount;
            }
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
        float targetFill = (maxHealth > 0) ? currentHealth / maxHealth : 0;

        // Stop any existing main bar animation
        if (healthAnimationCoroutine != null)
        {
            StopCoroutine(healthAnimationCoroutine);
        }

        // Check if we are taking damage or healing
        if (targetFill < healthBarImage.fillAmount)
        {
            // --- TAKING DAMAGE ---
            // Main bar animates down quickly
            healthAnimationCoroutine = StartCoroutine(AnimateBar(healthBarImage, targetFill, animationSpeed));

            // Stop any existing delayed bar animation
            if (delayedHealthAnimationCoroutine != null)
            {
                StopCoroutine(delayedHealthAnimationCoroutine);
            }
            // Delayed bar waits and then animates down slowly
            delayedHealthAnimationCoroutine = StartCoroutine(AnimateBar(delayedHealthBarImage, targetFill, drainSpeed, delayBeforeDrain));
        }
        else
        {
            // --- HEALING ---
            // Delayed bar snaps to the new health value immediately
            if (delayedHealthBarImage != null)
            {
                delayedHealthBarImage.fillAmount = targetFill;
            }
            // Main bar animates up
            healthAnimationCoroutine = StartCoroutine(AnimateBar(healthBarImage, targetFill, animationSpeed));
        }
    }

    private IEnumerator AnimateBar(Image barImage, float targetFill, float speed, float delay = 0f)
    {
        if (barImage == null) yield break;

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        float currentFill = barImage.fillAmount;

        // Loop until the current fill amount is very close to the target
        while (!Mathf.Approximately(currentFill, targetFill))
        {
            // Move the current fill amount towards the target
            currentFill = Mathf.MoveTowards(currentFill, targetFill, speed * Time.deltaTime);
            barImage.fillAmount = currentFill;
            yield return null; // Wait for the next frame
        }

        // Ensure it's exactly at the target value in the end
        barImage.fillAmount = targetFill;
    }
}
