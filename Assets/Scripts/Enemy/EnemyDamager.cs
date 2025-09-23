using UnityEngine;
using ElderCloak.Core.Interfaces;

namespace ElderCloak.Enemy
{
    /// <summary>
    /// Basic enemy that damages players on contact
    /// </summary>
    public class EnemyDamager : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int contactDamage = 20;
        [SerializeField] private float damageCooldown = 1.0f;
        [SerializeField] private LayerMask playerLayerMask = 1;
        
        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 5f;
        
        // Track last damage time for each player to prevent spam damage
        private float lastDamageTime = -1f;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDamagePlayer(collision.collider);
        }
        
        private void TryDamagePlayer(Collider2D other)
        {
            // Check if it's a player and cooldown has passed
            if (Time.time - lastDamageTime < damageCooldown) return;
            
            // Check if object is on player layer
            if (((1 << other.gameObject.layer) & playerLayerMask) == 0) return;
            
            // Try to damage the player
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(contactDamage, transform);
                lastDamageTime = Time.time;
                
                // Apply knockback to player
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}