using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class StaminaBarUI : MonoBehaviour
{
    [Header("Delayed Bar (Consumption Feedback)")]
    [SerializeField] private Image delayedStaminaBarImage;
    [SerializeField] private float delayBeforeDrain = 0.5f;
    [SerializeField] private float drainSpeed = 1f;

    [Header("Low Stamina Flash")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashSpeed = 2f;

    private Image staminaBarImage;
    private PlayerStamina playerStamina;
    private Color originalColor;

    private Coroutine delayedStaminaAnimationCoroutine;

    private void Awake()
    {
        staminaBarImage = GetComponent<Image>();
        originalColor = staminaBarImage.color;
    }

    private void Start()
    {
        playerStamina = FindFirstObjectByType<PlayerStamina>();
        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged += UpdateStaminaBar;
            playerStamina.OnLowStaminaStateChanged += HandleLowStaminaFlash;
            // Başlangıç değerini ayarla
            float initialFill = playerStamina.CurrentStamina / 100f; // Assume 100 max at start
            staminaBarImage.fillAmount = initialFill;
            if (delayedStaminaBarImage != null) {
                delayedStaminaBarImage.fillAmount = initialFill;
            }
        }
        else
        {
            Debug.LogError("StaminaBarUI, sahnede PlayerStamina bulamadı!", this);
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged -= UpdateStaminaBar;
            playerStamina.OnLowStaminaStateChanged -= HandleLowStaminaFlash;
        }
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        float targetFill = (maxStamina > 0) ? currentStamina / maxStamina : 0;

        // Check if stamina is being consumed or regenerated
        if (targetFill < staminaBarImage.fillAmount)
        {
            // --- CONSUMPTION ---
            // Main bar snaps down instantly for responsiveness
            staminaBarImage.fillAmount = targetFill;

            // Stop any existing delayed bar animation
            if (delayedStaminaAnimationCoroutine != null)
            {
                StopCoroutine(delayedStaminaAnimationCoroutine);
            }
            // Delayed bar waits and then animates down
            delayedStaminaAnimationCoroutine = StartCoroutine(AnimateDelayedBar(targetFill));
        }
        else
        {
            // --- REGENERATION ---
            // Both bars move up together smoothly
            staminaBarImage.fillAmount = targetFill;
            if (delayedStaminaBarImage != null)
            {
                delayedStaminaBarImage.fillAmount = targetFill;
            }
        }
    }

    private IEnumerator AnimateDelayedBar(float targetFill)
    {
        if (delayedStaminaBarImage == null) yield break;

        yield return new WaitForSeconds(delayBeforeDrain);

        while (delayedStaminaBarImage.fillAmount > targetFill)
        {
            delayedStaminaBarImage.fillAmount = Mathf.MoveTowards(delayedStaminaBarImage.fillAmount, targetFill, drainSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void HandleLowStaminaFlash(bool isLow)
    {
        if (isLow)
        {
            StartCoroutine(FlashRoutine());
        }
        else
        {
            StopAllCoroutines(); // Stop flashing and delayed bar animations
            staminaBarImage.color = originalColor;
        }
    }

    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1) / 2; // Oscillates between 0 and 1
            staminaBarImage.color = Color.Lerp(originalColor, flashColor, alpha);
            yield return null;
        }
    }
}
