using System;
using UnityEngine;
using ElderCloak.Interfaces;

namespace ElderCloak.Health
{
    /// <summary>
    /// Comprehensive health system implementation.
    /// Handles health tracking, damage processing, and death logic.
    /// Future-proof design with extensive event system for UI and gameplay integration.
    /// </summary>
    [System.Serializable]
    public class HealthSystem : IHealth, IDamageable
    {
        [Header("Health Configuration")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool canTakeDamage = true;
        [SerializeField] private bool regenerateHealth = false;
        [SerializeField] private float regenerationRate = 5f;
        [SerializeField] private float regenerationDelay = 3f;

        [Header("Damage Immunity")]
        [SerializeField] private float invulnerabilityDuration = 1f;
        [SerializeField] private bool isInvulnerable = false;

        // Events for health system
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<float, GameObject> OnDamageTaken;
        public event Action<float> OnHealthRestored;

        // Internal state tracking
        private float lastDamageTime;
        private bool isDead = false;
        private MonoBehaviour owner;

        /// <summary>
        /// Initialize the health system with an owner MonoBehaviour.
        /// </summary>
        /// <param name="owner">The MonoBehaviour that owns this health system</param>
        public void Initialize(MonoBehaviour owner)
        {
            this.owner = owner;
            currentHealth = maxHealth;
            isDead = false;
            isInvulnerable = false;
        }

        /// <summary>
        /// Update method to be called from the owner's Update method.
        /// Handles regeneration and invulnerability timing.
        /// </summary>
        public void Update()
        {
            HandleInvulnerability();
            HandleRegeneration();
        }

        #region IHealth Implementation

        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;

        public void SetMaxHealth(float maxHealth, bool adjustCurrent = false)
        {
            float previousMaxHealth = this.maxHealth;
            this.maxHealth = Mathf.Max(0, maxHealth);

            if (adjustCurrent && previousMaxHealth > 0)
            {
                float healthRatio = currentHealth / previousMaxHealth;
                currentHealth = this.maxHealth * healthRatio;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, this.maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, this.maxHealth);
        }

        public void RestoreHealth(float amount)
        {
            if (isDead || amount <= 0) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            
            if (currentHealth != previousHealth)
            {
                OnHealthRestored?.Invoke(amount);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        public void RestoreToFull()
        {
            RestoreHealth(maxHealth);
        }

        public bool IsAlive() => !isDead && currentHealth > 0;

        public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0f;

        #endregion

        #region IDamageable Implementation

        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            if (!CanTakeDamageNow() || damage <= 0) return;

            float actualDamage = Mathf.Min(damage, currentHealth);
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            
            lastDamageTime = Time.time;
            isInvulnerable = invulnerabilityDuration > 0;

            // Trigger events
            OnDamageTaken?.Invoke(actualDamage, damageSource);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check for death
            if (currentHealth <= 0 && !isDead)
            {
                HandleDeath();
            }
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Enable or disable the ability to take damage.
        /// </summary>
        /// <param name="canTakeDamage">Whether this object can take damage</param>
        public void SetCanTakeDamage(bool canTakeDamage)
        {
            this.canTakeDamage = canTakeDamage;
        }

        /// <summary>
        /// Enable or disable health regeneration.
        /// </summary>
        /// <param name="regenerate">Whether to regenerate health</param>
        /// <param name="rate">Regeneration rate per second</param>
        /// <param name="delay">Delay after damage before regeneration starts</param>
        public void SetRegeneration(bool regenerate, float rate = 5f, float delay = 3f)
        {
            regenerateHealth = regenerate;
            regenerationRate = rate;
            regenerationDelay = delay;
        }

        /// <summary>
        /// Manually set invulnerability state.
        /// </summary>
        /// <param name="invulnerable">Whether to be invulnerable</param>
        /// <param name="duration">Duration of invulnerability (0 for permanent until manually disabled)</param>
        public void SetInvulnerable(bool invulnerable, float duration = 0f)
        {
            isInvulnerable = invulnerable;
            if (invulnerable && duration > 0f)
            {
                if (owner != null)
                {
                    owner.StartCoroutine(InvulnerabilityCoroutine(duration));
                }
            }
        }

        /// <summary>
        /// Revive the character with specified health amount.
        /// </summary>
        /// <param name="healthAmount">Health to revive with (default: full health)</param>
        public void Revive(float healthAmount = -1f)
        {
            if (healthAmount < 0) healthAmount = maxHealth;
            
            isDead = false;
            currentHealth = Mathf.Min(healthAmount, maxHealth);
            canTakeDamage = true;
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if damage can be taken right now.
        /// </summary>
        private bool CanTakeDamageNow()
        {
            return canTakeDamage && !isDead && !isInvulnerable;
        }

        /// <summary>
        /// Handle the death sequence.
        /// </summary>
        private void HandleDeath()
        {
            isDead = true;
            canTakeDamage = false;
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Handle invulnerability timing.
        /// </summary>
        private void HandleInvulnerability()
        {
            if (isInvulnerable && invulnerabilityDuration > 0 && 
                Time.time >= lastDamageTime + invulnerabilityDuration)
            {
                isInvulnerable = false;
            }
        }

        /// <summary>
        /// Handle health regeneration logic.
        /// </summary>
        private void HandleRegeneration()
        {
            if (regenerateHealth && !isDead && currentHealth < maxHealth)
            {
                if (Time.time >= lastDamageTime + regenerationDelay)
                {
                    RestoreHealth(regenerationRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Coroutine for timed invulnerability.
        /// </summary>
        private System.Collections.IEnumerator InvulnerabilityCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }

        #endregion

        #region Inspector Debug Info
#if UNITY_EDITOR
        [Header("Debug Info (Runtime Only)")]
        [SerializeField, ReadOnly] private bool debugIsDead;
        [SerializeField, ReadOnly] private bool debugIsInvulnerable;
        [SerializeField, ReadOnly] private float debugHealthPercentage;

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                debugIsDead = isDead;
                debugIsInvulnerable = isInvulnerable;
                debugHealthPercentage = GetHealthPercentage();
            }
        }

        /// <summary>
        /// Custom attribute for read-only fields in inspector.
        /// </summary>
        public class ReadOnlyAttribute : PropertyAttribute { }
#endif
        #endregion
    }
}