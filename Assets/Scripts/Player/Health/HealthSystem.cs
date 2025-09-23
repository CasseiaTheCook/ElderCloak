using UnityEngine;
using UnityEngine.Events;
using ElderCloak.Core.Interfaces;

namespace ElderCloak.Player.Health
{
    /// <summary>
    /// Health system component that handles damage, healing, and death
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        
        [Header("Damage Settings")]
        [SerializeField] private float invulnerabilityTime = 0.5f;
        private float lastDamageTime = -1f;
        
        [Header("Events")]
        public UnityEvent<int, int> OnHealthChanged; // current, max
        public UnityEvent<int, Transform> OnDamageTaken; // damage, source
        public UnityEvent<int> OnHealed; // heal amount
        public UnityEvent OnDeath;
        public UnityEvent OnRevive;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        public bool IsInvulnerable => Time.time - lastDamageTime < invulnerabilityTime;
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void TakeDamage(int damage, Transform damageSource = null)
        {
            if (!IsAlive || IsInvulnerable) return;
            
            damage = Mathf.Max(0, damage);
            currentHealth = Mathf.Max(0, currentHealth - damage);
            lastDamageTime = Time.time;
            
            OnDamageTaken?.Invoke(damage, damageSource);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (!IsAlive)
            {
                OnDeath?.Invoke();
            }
        }
        
        public void Heal(int healAmount)
        {
            if (!IsAlive) return;
            
            healAmount = Mathf.Max(0, healAmount);
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            
            OnHealed?.Invoke(healAmount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void FullHeal()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void Revive(int healthAmount = -1)
        {
            if (IsAlive) return;
            
            currentHealth = healthAmount < 0 ? maxHealth : Mathf.Min(healthAmount, maxHealth);
            OnRevive?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}