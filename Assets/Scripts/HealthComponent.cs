using UnityEngine;
using UnityEngine.Events;
using ElderCloak.Interfaces;

namespace ElderCloak.Health
{
    /// <summary>
    /// Base health component that can be used by both players and enemies
    /// Implements the IHealth interface for standardized health management
    /// </summary>
    public class HealthComponent : MonoBehaviour, IHealth, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool invulnerable = false;
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<float, float> onHealthChanged = new UnityEvent<float, float>();
        [SerializeField] private UnityEvent onDeath = new UnityEvent();
        [SerializeField] private UnityEvent<float, GameObject> onDamageTaken = new UnityEvent<float, GameObject>();
        [SerializeField] private UnityEvent<float> onHealed = new UnityEvent<float>();
        
        private float invulnerabilityTimer = 0f;
        
        #region IHealth Implementation
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f;
        public UnityEvent<float, float> OnHealthChanged => onHealthChanged;
        public UnityEvent OnDeath => onDeath;
        #endregion
        
        /// <summary>
        /// Additional events for more detailed health monitoring
        /// </summary>
        public UnityEvent<float, GameObject> OnDamageTaken => onDamageTaken;
        public UnityEvent<float> OnHealed => onHealed;
        
        /// <summary>
        /// Whether the object is currently invulnerable to damage
        /// </summary>
        public bool IsInvulnerable => invulnerable || invulnerabilityTimer > 0f;
        
        private void Awake()
        {
            // Initialize current health to max health if not set
            if (currentHealth <= 0f)
            {
                currentHealth = maxHealth;
            }
        }
        
        private void Update()
        {
            // Handle invulnerability timer
            if (invulnerabilityTimer > 0f)
            {
                invulnerabilityTimer -= Time.deltaTime;
            }
        }
        
        #region IDamageable Implementation
        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            if (!CanTakeDamage() || damage <= 0f)
                return;
            
            float actualDamage = Mathf.Min(damage, currentHealth);
            currentHealth = Mathf.Max(0f, currentHealth - actualDamage);
            
            // Trigger invulnerability period
            if (invulnerabilityDuration > 0f)
            {
                invulnerabilityTimer = invulnerabilityDuration;
            }
            
            // Trigger events
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            onDamageTaken?.Invoke(actualDamage, damageSource);
            
            // Check for death
            if (!IsAlive)
            {
                Die();
            }
        }
        
        public bool CanTakeDamage()
        {
            return IsAlive && !IsInvulnerable;
        }
        #endregion
        
        #region IHealth Implementation
        public void Heal(float healAmount)
        {
            if (!IsAlive || healAmount <= 0f)
                return;
            
            float actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
            currentHealth = Mathf.Min(maxHealth, currentHealth + actualHeal);
            
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            onHealed?.Invoke(actualHeal);
        }
        
        public void RestoreFullHealth()
        {
            if (!IsAlive)
                return;
            
            float healAmount = maxHealth - currentHealth;
            currentHealth = maxHealth;
            
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            if (healAmount > 0f)
            {
                onHealed?.Invoke(healAmount);
            }
        }
        
        public void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = false)
        {
            if (newMaxHealth <= 0f)
                return;
            
            if (adjustCurrentHealth && maxHealth > 0f)
            {
                // Scale current health proportionally
                float healthRatio = currentHealth / maxHealth;
                currentHealth = newMaxHealth * healthRatio;
            }
            else if (currentHealth > newMaxHealth)
            {
                // Clamp current health to new max
                currentHealth = newMaxHealth;
            }
            
            maxHealth = newMaxHealth;
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        #endregion
        
        /// <summary>
        /// Handle death logic
        /// </summary>
        private void Die()
        {
            onDeath?.Invoke();
        }
        
        /// <summary>
        /// Set invulnerability state
        /// </summary>
        /// <param name="isInvulnerable">Whether to be invulnerable</param>
        public void SetInvulnerable(bool isInvulnerable)
        {
            invulnerable = isInvulnerable;
        }
        
        /// <summary>
        /// Apply temporary invulnerability
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        public void ApplyTemporaryInvulnerability(float duration)
        {
            invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, duration);
        }
        
        /// <summary>
        /// Force set current health (for initialization or special cases)
        /// </summary>
        /// <param name="newHealth">New health value</param>
        public void SetCurrentHealth(float newHealth)
        {
            currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (!IsAlive)
            {
                Die();
            }
        }
    }
}