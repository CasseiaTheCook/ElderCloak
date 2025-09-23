using UnityEngine;
using System.Collections;

namespace ElderCloak.Player.Combat
{
    [System.Serializable]
    public class MeleeAttackSystem
    {
        [Header("Attack Settings")]
        [SerializeField] private int attackDamage = 25;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private LayerMask enemyLayer = 1 << 6; // Assuming enemies are on layer 6
        
        [Header("Attack Animation")]
        [SerializeField] private float attackDuration = 0.3f;
        [SerializeField] private AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;
        
        private float cooldownTimer;
        private bool isAttacking;
        private float attackTimer;
        
        private Transform attackPoint;
        private MonoBehaviour owner; // For coroutines
        
        public bool CanAttack => cooldownTimer <= 0 && !isAttacking;
        public bool IsAttacking => isAttacking;
        public float CooldownProgress => 1f - (cooldownTimer / attackCooldown);
        
        public void Initialize(Transform attackPointTransform, MonoBehaviour ownerMono)
        {
            attackPoint = attackPointTransform;
            owner = ownerMono;
            cooldownTimer = 0;
        }
        
        public void Update()
        {
            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;
            
            if (isAttacking)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    EndAttack();
                }
            }
        }
        
        public bool TryAttack()
        {
            if (!CanAttack)
                return false;
            
            StartAttack();
            return true;
        }
        
        private void StartAttack()
        {
            isAttacking = true;
            attackTimer = attackDuration;
            cooldownTimer = attackCooldown;
            
            // Perform the attack
            PerformAttack();
            
            Debug.Log("Melee attack started!");
        }
        
        private void PerformAttack()
        {
            // Find all enemies in range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
            
            foreach (Collider2D enemy in hitEnemies)
            {
                // Try to get health component
                var enemyHealth = enemy.GetComponent<ElderCloak.Player.Health.HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                    Debug.Log($"Hit enemy {enemy.name} for {attackDamage} damage!");
                }
                
                // Apply knockback
                ApplyKnockback(enemy);
            }
            
            // Visual feedback (could trigger particle effects, screen shake, etc.)
            if (hitEnemies.Length > 0)
            {
                // You can add screen shake or other effects here
                Debug.Log($"Hit {hitEnemies.Length} enemies!");
            }
        }
        
        private void ApplyKnockback(Collider2D target)
        {
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockbackDirection = (target.transform.position - attackPoint.position).normalized;
                targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                
                // Optional: Start coroutine to reduce velocity after knockback duration
                if (owner != null)
                {
                    owner.StartCoroutine(ReduceKnockbackAfterDelay(targetRb, knockbackDuration));
                }
            }
        }
        
        private IEnumerator ReduceKnockbackAfterDelay(Rigidbody2D targetRb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (targetRb != null)
            {
                targetRb.linearVelocity *= 0.5f; // Reduce velocity by half
            }
        }
        
        private void EndAttack()
        {
            isAttacking = false;
            Debug.Log("Melee attack ended!");
        }
        
        // For debugging and visualization
        public void DrawGizmos()
        {
            if (attackPoint != null)
            {
                Color gizmoColor = isAttacking ? Color.red : Color.yellow;
                gizmoColor.a = 0.3f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            }
        }
        
        // Reset cooldown (useful for special abilities or power-ups)
        public void ResetCooldown()
        {
            cooldownTimer = 0;
        }
    }
}