using UnityEngine;
using System;
using System.Collections;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 30f; // Saniyede yenilenen stamina
    [SerializeField] private float staminaRegenDelay = 1.5f; // Yenilenmenin başlaması için bekleme süresi

    [Header("Dependencies")]
    [SerializeField] private SkillRegenSystem skillRegenSystem;

    private float _currentStamina;
    public float CurrentStamina => _currentStamina;

    private Coroutine _regenCoroutine;

    public event Action<float, float> OnStaminaChanged;

    private void Awake()
    {
        if (skillRegenSystem == null)
        {
            skillRegenSystem = GetComponent<SkillRegenSystem>();
        }
    }

    private void Start()
    {
        _currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }

    /// <summary>
    /// Checks if there is enough stamina available.
    /// </summary>
    public bool HasEnoughStamina(float amount)
    {
        // Allow action if at least 1 stamina is available
        return _currentStamina >= 1f;
    }

    /// <summary>
    /// Consumes a specified amount of stamina and starts the regeneration delay.
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        if (_currentStamina >= 1f)
        {
            // Consume the lesser of the requested amount or what's left
            float consumeAmount = Mathf.Min(amount, _currentStamina);
            _currentStamina -= consumeAmount;
            OnStaminaChanged?.Invoke(_currentStamina, maxStamina);

            // Mevcut bir yenilenme varsa durdur
            if (_regenCoroutine != null)
            {
                StopCoroutine(_regenCoroutine);
            }

            // Yenisini başlat
            _regenCoroutine = StartCoroutine(RegenerateStamina());
        }
    }

    private IEnumerator RegenerateStamina()
    {
        // Yenilenme başlamadan önce bekle
        yield return new WaitForSeconds(staminaRegenDelay);

        // Stamina tam dolana kadar yenile
        while (_currentStamina < maxStamina)
        {
            _currentStamina += staminaRegenRate * Time.deltaTime;
            _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
            OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
            yield return null; // Bir sonraki frame'e kadar bekle
        }
        _regenCoroutine = null;
    }
}