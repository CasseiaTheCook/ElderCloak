using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry Settings")]
    [SerializeField] private float parryDuration = 0.3f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private Color parrySuccessColor = Color.yellow;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SkillRegenSystem skillRegenSystem;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

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

        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (context.performed && canParry)
        {
            StartCoroutine(ParryWindow());
        }
    }

    private IEnumerator ParryWindow()
    {
        canParry = false;       // Cooldown'ı başlat
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
}
