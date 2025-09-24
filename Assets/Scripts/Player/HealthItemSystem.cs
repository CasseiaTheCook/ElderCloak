using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class HealthItemSystem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private int initialItemCount = 3;
    [Tooltip("Percentage of max health to restore on use.")]
    [SerializeField] private float healPercentage = 30f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemCounterText;

    private int currentItemCount;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("HealthItemSystem requires a PlayerHealth component on the same GameObject.", this);
        }
    }

    private void Start()
    {
        currentItemCount = initialItemCount;
        UpdateUI();
    }

    /// <summary>
    /// Called by the PlayerInput component when the "Heal" action is performed.
    /// </summary>
    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UseItem();
        }
    }

    private void UseItem()
    {
        // Can't use if no items left, player health is full, or playerHealth script is missing.
        if (currentItemCount <= 0 || playerHealth == null || playerHealth.IsAtMaxHealth)
        {
            // Optional: Play a "fail" sound effect here
            return;
        }

        currentItemCount--;
        playerHealth.HealPercentage(healPercentage);
        UpdateUI();
        // Optional: Play a "success" sound or particle effect here
    }

    private void UpdateUI()
    {
        if (itemCounterText != null)
        {
            itemCounterText.text = currentItemCount.ToString();
        }
    }
}
