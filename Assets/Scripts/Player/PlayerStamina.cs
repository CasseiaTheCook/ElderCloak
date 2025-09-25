using UnityEngine;
using System;
using System.Collections;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [Tooltip("Stamina bu değerin altına düştüğünde bitkinlik durumu başlar (yüzde olarak).")]
    [Range(0, 100)]
    [SerializeField] private float lowStaminaThreshold = 30f;
    [SerializeField] private float staminaRegenRate = 30f; // Saniyede yenilenen stamina
    [SerializeField] private float staminaRegenDelay = 1.5f; // Yenilenmenin başlaması için bekleme süresi

    [Header("Dependencies")]
    [SerializeField] private SkillRegenSystem skillRegenSystem;

    private float _currentStamina;
    public float CurrentStamina => _currentStamina;

    private Coroutine _regenCoroutine;
    private bool _isLowOnStamina = false;

    public event Action<float, float> OnStaminaChanged;
    public event Action<bool> OnLowStaminaStateChanged;

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
        CheckLowStaminaState();
    }

    /// <summary>
    /// Checks if there is enough stamina available.
    /// </summary>
    public bool HasEnoughStamina(float amount)
    {
        return _currentStamina >= amount;
    }

    /// <summary>
    /// Consumes a specified amount of stamina and starts the regeneration delay.
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        // Eğer yetenek barı doluysa, stamina tüketme.
        if (skillRegenSystem != null && skillRegenSystem.IsFull())
        {
            return;
        }

        if (_currentStamina >= amount)
        {
            _currentStamina -= amount;
            CheckLowStaminaState();
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
            CheckLowStaminaState();
            OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
            yield return null; // Bir sonraki frame'e kadar bekle
        }
        _regenCoroutine = null;
    }

    private void CheckLowStaminaState()
    {
        bool isCurrentlyLow = (_currentStamina / maxStamina * 100f) < lowStaminaThreshold;

        // Sadece durum değiştiğinde event'i tetikle
        if (isCurrentlyLow != _isLowOnStamina)
        {
            _isLowOnStamina = isCurrentlyLow;
            OnLowStaminaStateChanged?.Invoke(_isLowOnStamina);
            Debug.Log($"Low Stamina State: {_isLowOnStamina}");
        }
    }
}
