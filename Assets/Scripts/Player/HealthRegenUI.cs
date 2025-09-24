using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthRegenUI : MonoBehaviour
{
    private Image itemImage;
    private float currentFill = 0f;
    private float maxFill = 100f; // This will be set by the PlayerAttack script

    public bool IsFull => currentFill >= maxFill;

    void Awake()
    {
        itemImage = GetComponent<Image>();
        if (itemImage.type != Image.Type.Filled)
        {
            Debug.LogWarning($"{gameObject.name}'s Image component is not set to 'Filled'. Please change it in the Inspector.", this);
            itemImage.type = Image.Type.Filled;
        }
        UpdateUI();
    }

    /// <summary>
    /// Called by the Player to set the maximum fill capacity.
    /// </summary>
    public void Initialize(float maxFillValue)
    {
        maxFill = maxFillValue;
        UpdateUI();
    }

    /// <summary>
    /// Increases the fill amount and updates the UI.
    /// </summary>
    public void AddFill(float amount)
    {
        if (currentFill >= maxFill) return; // Already full

        currentFill += amount;
        if (currentFill > maxFill)
        {
            currentFill = maxFill;
        }

        Debug.Log($"UI filled by {amount}. Current: {currentFill}/{maxFill}");
        UpdateUI();
    }
    
    /// <summary>
    /// Resets the fill amount after the item is used.
    /// </summary>
    public void Use()
    {
        currentFill = 0f;
        UpdateUI();
        Debug.Log("Health Regen Item used and emptied.");
    }

    private void UpdateUI()
    {
        if (maxFill > 0)
        {
            itemImage.fillAmount = currentFill / maxFill;
        }
    }
}
