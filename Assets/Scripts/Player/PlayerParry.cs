using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry Settings")]
    [SerializeField] private float parryStaminaCost = 30f;
    [SerializeField] private float parryDuration = 0.3f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private Color parrySuccessColor = Color.yellow;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SkillRegenSystem skillRegenSystem;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private PlayerStamina playerStamina;

    private bool isParrying = false;
    private bool canParry = true;
    private Color originalColor;

    public bool IsParrying => isParrying;

    private void Awake()
    {
        // Referansları otomatik olarak bul
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (skillRegenSystem == null) skillRegenSystem = GetComponent<SkillRegenSystem>();
        if (playerSpriteRenderer == null) playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (playerStamina == null) playerStamina = GetComponent<PlayerStamina>();

        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        // Parry yapabilmek için yeterli stamina var mı, bekleme süresinde değil mi ve bitkin değil mi?
        if (context.performed && canParry && playerStamina.HasEnoughStamina(1))
        {
            // Staminayı tüket
            playerStamina.ConsumeStamina(parryStaminaCost);
            // Parry işlemini başlat
            StartCoroutine(ParryWindow());
        }
    }

    private IEnumerator ParryWindow()
    {
        canParry = false;       // Parry'yi bekleme durumuna al
        isParrying = true;      // Parry durumunu aktif et

        yield return new WaitForSeconds(parryDuration);

        isParrying = false;     // Parry süresi bitti

        // Cooldown süresi kadar bekle ve tekrar parry yapmaya izin ver
        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }

    /// <summary>
    /// Called by PlayerHealth when damage is successfully parried.
    /// </summary>
    public void OnSuccessfulParry()
    {
        Debug.Log("Parry Successful!");

        // Yetenek barını doldur
        skillRegenSystem?.FillToMax(); // Fill the bar completely

        // Görsel geri bildirim için rengi anlık değiştir
        StartCoroutine(FlashParryColor());
    }

    private IEnumerator FlashParryColor()
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = parrySuccessColor;
            yield return new WaitForSeconds(0.15f);
            playerSpriteRenderer.color = originalColor;
        }
    }


    private void SetExhaustedState(bool isExhausted)
    {
        // When exhausted, player cannot parry. This overrides the cooldown.
        if (isExhausted)
        {
            canParry = false;
        }
        else
        {
            // IMPORTANT FIX: When no longer exhausted, allow parrying again.
            // The regular cooldown from ParryWindow will still apply if it's active.
            // This just removes the exhaustion lock.
            canParry = true;
        }
    }
}
