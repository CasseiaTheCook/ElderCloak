using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using ElderCloak.Core.Interfaces;
using System.Collections;

namespace ElderCloak.Player.Combat
{
    /// <summary>
    /// Melee attack component for close-range punch attacks
    /// </summary>
    public class MeleeAttack : MonoBehaviour, IAttacker
    {
        [Header("Attack Settings")]
        [SerializeField] private int attackDamage = 25;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackDuration = 0.2f;
        
        [Header("Attack Area")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private Vector2 attackAreaSize = new Vector2(1.2f, 1.0f);
        [SerializeField] private LayerMask enemyLayerMask = 1;
        
        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private float knockbackDuration = 0.3f;
        
        [Header("Events")]
        public UnityEvent OnAttackStart;
        public UnityEvent OnAttackHit;
        public UnityEvent OnAttackEnd;
        
        // Components
        private PlayerInput playerInput;
        private Animator animator;
        
        // State
        private bool isAttacking;
        private float lastAttackTime = -1f;
        private bool attackInputPressed;
        
        // Properties
        public int Damage => attackDamage;
        public bool IsAttacking => isAttacking;
        
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            animator = GetComponent<Animator>();
            
            // Create attack point if not assigned
            if (attackPoint == null)
            {
                GameObject point = new GameObject("AttackPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0.8f, 0, 0);
                attackPoint = point.transform;
            }
        }
        
        private void Update()
        {
            HandleAttackInput();
        }
        
        private void HandleAttackInput()
        {
            if (attackInputPressed && CanAttack())
            {
                Attack();
                attackInputPressed = false;
            }
        }
        
        public void Attack()
        {
            if (!CanAttack()) return;
            
            StartCoroutine(AttackSequence());
        }
        
        private IEnumerator AttackSequence()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            
            OnAttackStart?.Invoke();
            
            // Play attack animation if available
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // Wait a bit before applying damage (for animation timing)
            yield return new WaitForSeconds(attackDuration * 0.3f);
            
            // Perform the actual attack
            PerformAttack();
            
            // Wait for attack to finish
            yield return new WaitForSeconds(attackDuration * 0.7f);
            
            isAttacking = false;
            OnAttackEnd?.Invoke();
        }
        
        private void PerformAttack()
        {
            // Adjust attack point position based on facing direction
            Vector3 attackPosition = attackPoint.position;
            if (transform.localScale.x < 0)
            {
                attackPosition = transform.position + new Vector3(-attackRange * 0.8f, attackPoint.localPosition.y, 0);
            }
            
            // Find all enemies in attack range
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPosition, attackAreaSize, 0f, enemyLayerMask);
            
            bool hitSomething = false;
            
            foreach (Collider2D enemy in hitEnemies)
            {
                // Don't hit ourselves
                if (enemy.transform == transform) continue;
                
                hitSomething = true;
                
                // Apply damage
                IDamageable damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage, transform);
                }
                
                // Apply knockback
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
            }
            
            if (hitSomething)
            {
                OnAttackHit?.Invoke();
            }
        }
        
        public bool CanAttack()
        {
            return !isAttacking && Time.time - lastAttackTime >= attackCooldown;
        }
        
        // Input System callback
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                attackInputPressed = true;
            }
        }
        
        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = isAttacking ? Color.red : Color.yellow;
                
                Vector3 attackPosition = attackPoint.position;
                if (transform.localScale.x < 0)
                {
                    attackPosition = transform.position + new Vector3(-attackRange * 0.8f, attackPoint.localPosition.y, 0);
                }
                
                Gizmos.DrawWireCube(attackPosition, attackAreaSize);
            }
        }
    }
}