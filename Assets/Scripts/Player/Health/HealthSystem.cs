using UnityEngine;
using UnityEngine.Events;

namespace ElderCloak.Player.Health
{
    [System.Serializable]
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        
        [Header("Damage Settings")]
        [SerializeField] private float invulnerabilityTime = 1.5f;
        [SerializeField] private bool isInvulnerable = false;
        
        [Header("Events")]
        public UnityEvent<int> OnHealthChanged;
        public UnityEvent<int, int> OnDamageTaken; // damage amount, remaining health
        public UnityEvent OnDeath;
        public UnityEvent OnHeal;
        
        private float invulnerabilityTimer;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsInvulnerable => isInvulnerable;
        public float HealthPercentage => (float)currentHealth / maxHealth;
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        private void Update()
        {
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0)
                {
                    isInvulnerable = false;
                }
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (IsDead || IsInvulnerable)
                return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            OnHealthChanged?.Invoke(currentHealth);
            OnDamageTaken?.Invoke(damage, currentHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartInvulnerability();
            }
        }
        
        public void Heal(int healAmount)
        {
            if (IsDead)
                return;
            
            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
            
            if (currentHealth != oldHealth)
            {
                OnHealthChanged?.Invoke(currentHealth);
                OnHeal?.Invoke();
            }
        }
        
        public void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        private void StartInvulnerability()
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityTime;
        }
        
        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log("Player has died!");
        }
        
        public void Respawn()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            invulnerabilityTimer = 0;
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        // For debugging in inspector
        [ContextMenu("Take 10 Damage")]
        private void TestDamage()
        {
            TakeDamage(10);
        }
        
        [ContextMenu("Heal 10 Health")]
        private void TestHeal()
        {
            Heal(10);
        }
    }
}