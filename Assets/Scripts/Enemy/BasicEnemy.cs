using UnityEngine;
using ElderCloak.Core.Interfaces;

namespace ElderCloak.Enemy
{
    /// <summary>
    /// Basic enemy with health that can be damaged by player attacks
    /// </summary>
    [RequireComponent(typeof(EnemyDamager))]
    public class BasicEnemy : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 50;
        [SerializeField] private int currentHealth;
        
        [Header("Death Settings")]
        [SerializeField] private float deathDelay = 1f;
        
        // Properties
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        // Components
        private SpriteRenderer spriteRenderer;
        private Collider2D enemyCollider;
        private EnemyDamager damager;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            spriteRenderer = GetComponent<SpriteRenderer>();
            enemyCollider = GetComponent<Collider2D>();
            damager = GetComponent<EnemyDamager>();
        }
        
        public void TakeDamage(int damage, Transform damageSource = null)
        {
            if (!IsAlive) return;
            
            damage = Mathf.Max(0, damage);
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // Visual feedback for taking damage
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashRed());
            }
            
            Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}/{maxHealth}");
            
            if (!IsAlive)
            {
                Die();
            }
        }
        
        public void Heal(int healAmount)
        {
            if (!IsAlive) return;
            
            healAmount = Mathf.Max(0, healAmount);
            currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        }
        
        private void Die()
        {
            Debug.Log($"{gameObject.name} has died!");
            
            // Disable damager so it can't hurt player anymore
            if (damager != null)
            {
                damager.enabled = false;
            }
            
            // Disable collider
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
            
            // Optional death effects
            if (spriteRenderer != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                Destroy(gameObject, deathDelay);
            }
        }
        
        private System.Collections.IEnumerator FlashRed()
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            Color originalColor = spriteRenderer.color;
            float fadeTime = deathDelay;
            float elapsedTime = 0;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}