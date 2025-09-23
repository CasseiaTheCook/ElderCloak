using UnityEngine;
using ElderCloak.Player.Health;

namespace ElderCloak.Enemies
{
    /// <summary>
    /// Simple enemy that damages the player on contact and can be attacked by the player
    /// </summary>
    public class EnemyDamager : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private int damageAmount = 15;
        [SerializeField] private float damageInterval = 1f; // Cooldown between damage instances
        [SerializeField] private int enemyHealth = 50;
        
        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 8f;
        
        private float lastDamageTime;
        private HealthSystem enemyHealthSystem;
        
        private void Awake()
        {
            // Add health system to this enemy
            enemyHealthSystem = gameObject.AddComponent<HealthSystem>();
            enemyHealthSystem.SetMaxHealth(enemyHealth);
            enemyHealthSystem.OnDeath.AddListener(OnEnemyDeath);
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Check if enough time has passed since last damage
                if (Time.time - lastDamageTime >= damageInterval)
                {
                    HealthSystem playerHealth = other.GetComponent<HealthSystem>();
                    if (playerHealth != null && !playerHealth.IsInvulnerable)
                    {
                        playerHealth.TakeDamage(damageAmount);
                        lastDamageTime = Time.time;
                        
                        // Apply knockback to player
                        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                        }
                        
                        Debug.Log($"Enemy dealt {damageAmount} damage to player!");
                    }
                }
            }
        }
        
        private void OnEnemyDeath()
        {
            Debug.Log("Enemy destroyed!");
            // You can add death effects here (particles, sound, etc.)
            Destroy(gameObject);
        }
        
        // Visual feedback when enemy takes damage
        private void Start()
        {
            enemyHealthSystem.OnDamageTaken.AddListener((damage, remaining) =>
            {
                Debug.Log($"Enemy took {damage} damage! Remaining health: {remaining}");
                // You can add visual effects here (flash red, etc.)
            });
        }
    }
}