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
        // Bar referansı varsa, tam dolu değilse ve 0'dan büyükse barı zamanla azalt
        if (skillRegenUI != null && !skillRegenUI.IsFull && skillRegenUI.CurrentFillAmount > 0)
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
        skillRegenUI?.AddFill(maxFillAmount);
    }

    /// <summary>
    /// Consumes the entire skill bar.
    /// </summary>
    public void Use()
    {
        skillRegenUI?.Use();
    }
}