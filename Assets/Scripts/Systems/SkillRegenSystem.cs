using UnityEngine;

public class SkillRegenSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillRegenUI skillRegenUI;

    [Header("Full Bar Abilities")]
    [SerializeField] private PlayerMovement playerMovement; // Reference to control dash abilities

    [Header("Bar Settings")]
    [SerializeField] private float maxFillAmount = 100f;
    [SerializeField] private float fillPerHit = 25f;
    [SerializeField] private float fillDecayRate = 5f; // Barın saniyede azalma miktarı
    [SerializeField] private float decayDelay = 1f; // Delay before decay starts after filling

    private float decayDelayTimer = 0f;

    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (skillRegenUI != null)
        {
            skillRegenUI.Initialize(maxFillAmount);
        }
    }

    private void Update()
    {
        // Update the decay delay timer
        if (decayDelayTimer > 0f)
        {
            decayDelayTimer -= Time.deltaTime;
        }

        // Bar referansı varsa, tam dolu değilse ve 0'dan büyükse barı zamanla azalt
        // Only decay if the delay timer has expired
        if (skillRegenUI != null && !skillRegenUI.IsFull && skillRegenUI.CurrentFillAmount > 0 && decayDelayTimer <= 0f)
        {
            skillRegenUI.AddFill(-fillDecayRate * Time.deltaTime);
        }

        // Update shadow dash availability based on whether the bar is full
        if (playerMovement != null)
        {
            playerMovement.canShadowDash = IsFull();
        }
    }

    /// <summary>
    /// Düşmana vurulduğunda barı doldurur.
    /// Bu metot, düşmana vuruş yapan script'ten çağrılmalıdır.
    /// </summary>
    public void AddFill()
    {
        if (skillRegenUI != null)
        {
            skillRegenUI.AddFill(fillPerHit);
            decayDelayTimer = decayDelay; // Reset the decay delay timer
        }
    }

    /// <summary>
    /// Returns true if the skill bar is full.
    /// </summary>
    public bool IsFull()
    {
        return skillRegenUI != null && skillRegenUI.IsFull;
    }

    /// <summary>
    /// Fills the skill bar to its maximum value.
    /// </summary>
    public void FillToMax()
    {
        // Pass a large number to ensure it fills completely, AddFill in UI will clamp it.
        if (skillRegenUI != null)
        {
            skillRegenUI.AddFill(maxFillAmount);
            decayDelayTimer = decayDelay; // Reset the decay delay timer
        }
    }

    /// <summary>
    /// Consumes the entire skill bar.
    /// </summary>
    public void Use()
    {
        skillRegenUI?.Use();
    }
}