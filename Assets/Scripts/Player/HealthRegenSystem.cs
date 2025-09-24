using UnityEngine;
using UnityEngine.InputSystem;

public class HealthRegenSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthRegenUI healthRegenUI;

    [Header("Health Regen Settings")]
    [SerializeField] private float maxFillAmount = 100f;
    [SerializeField] private float fillPerHit = 25f;
    [SerializeField] private int healAmount = 1;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        // Initialize the UI with the max fill value
        if (healthRegenUI != null)
        {
            healthRegenUI.Initialize(maxFillAmount);
        }
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Check if the required components are assigned.
            if (healthRegenUI == null)
            {
                
                return;
            }
            if (playerHealth == null)
            {
              
                return;
            }

            // Check if the bar is full and attempt to heal.
            if (healthRegenUI.IsFull)
            {
                Debug.Log("SUCCESS: Bar is full. Healing player.");
                playerHealth.Heal(healAmount);
                healthRegenUI.Use();
            }
            else
            {
                Debug.Log("INFO: Heal failed because the bar is not full.");
            }
        }
    }

    /// <summary>
    /// Called by PlayerAttack when an enemy is hit.
    /// </summary>
    public void AddFill()
    {
        if (healthRegenUI != null)
        {
            healthRegenUI.AddFill(fillPerHit);
        }
    }
}