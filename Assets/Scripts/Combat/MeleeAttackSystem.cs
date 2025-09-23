using UnityEngine;
using UnityEngine.Events;
using ElderCloak.Interfaces;

namespace ElderCloak.Combat
{
    /// <summary>
    /// Handles melee attack mechanics for both players and enemies
    /// Provides a flexible system for different attack types and hit detection
    /// </summary>
    public class MeleeAttackSystem : MonoBehaviour, IAttackable
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackDuration = 0.3f;
        [SerializeField] private LayerMask targetLayerMask = -1;
        
        [Header("Hit Detection")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private Vector2 attackBoxSize = new Vector2(1f, 1f);
        [SerializeField] private bool useCircleHitbox = false;
        
        [Header("Knockback")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onAttackStart = new UnityEvent();
        [SerializeField] private UnityEvent onAttackHit = new UnityEvent();
        [SerializeField] private UnityEvent onAttackEnd = new UnityEvent();
        [SerializeField] private UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();
        
        // State tracking
        private float lastAttackTime;
        private float attackTimer;
        private bool isCurrentlyAttacking;
        private Collider2D[] hitTargets = new Collider2D[10]; // Reusable array for hit detection
        
        // Components
        private SpriteRenderer spriteRenderer;
        
        #region IAttackable Implementation
        public bool CanAttack => Time.time >= lastAttackTime + attackCooldown && !isCurrentlyAttacking;
        public bool IsAttacking => isCurrentlyAttacking;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        #endregion
        
        /// <summary>
        /// Event callbacks for external systems
        /// </summary>
        public UnityEvent OnAttackStart => onAttackStart;
        public UnityEvent OnAttackHit => onAttackHit;
        public UnityEvent OnAttackEnd => onAttackEnd;
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;
        
        /// <summary>
        /// Current attack cooldown remaining
        /// </summary>
        public float AttackCooldownRemaining => Mathf.Max(0f, (lastAttackTime + attackCooldown) - Time.time);
        
        /// <summary>
        /// Whether the attack hitbox is currently active
        /// </summary>
        public bool IsHitboxActive => isCurrentlyAttacking && attackTimer > 0f;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Set up attack point if not assigned
            if (attackPoint == null)
            {
                GameObject attackPointGO = new GameObject("AttackPoint");
                attackPointGO.transform.SetParent(transform);
                attackPointGO.transform.localPosition = new Vector3(attackRange * 0.7f, 0, 0);
                attackPoint = attackPointGO.transform;
            }
        }
        
        private void Update()
        {
            UpdateAttackState();
        }
        
        /// <summary>
        /// Update attack timing and state
        /// </summary>
        private void UpdateAttackState()
        {
            if (isCurrentlyAttacking)
            {
                attackTimer -= Time.deltaTime;
                
                if (attackTimer <= 0f)
                {
                    EndAttack();
                }
            }
        }
        
        #region IAttackable Implementation
        public void PerformAttack()
        {
            if (!CanAttack) return;
            
            StartAttack();
        }
        
        public void CancelAttack()
        {
            if (isCurrentlyAttacking)
            {
                EndAttack();
            }
        }
        #endregion
        
        /// <summary>
        /// Start an attack sequence
        /// </summary>
        private void StartAttack()
        {
            isCurrentlyAttacking = true;
            lastAttackTime = Time.time;
            attackTimer = attackDuration;
            
            // Update attack point position based on facing direction
            UpdateAttackPointPosition();
            
            // Perform hit detection immediately
            DetectHits();
            
            onAttackStart?.Invoke();
        }
        
        /// <summary>
        /// End the current attack
        /// </summary>
        private void EndAttack()
        {
            isCurrentlyAttacking = false;
            attackTimer = 0f;
            
            onAttackEnd?.Invoke();
        }
        
        /// <summary>
        /// Update attack point position based on character facing direction
        /// </summary>
        private void UpdateAttackPointPosition()
        {
            if (spriteRenderer != null)
            {
                float direction = spriteRenderer.flipX ? -1f : 1f;
                Vector3 localPos = attackPoint.localPosition;
                localPos.x = Mathf.Abs(localPos.x) * direction;
                attackPoint.localPosition = localPos;
            }
        }
        
        /// <summary>
        /// Detect and process hits against targets
        /// </summary>
        private void DetectHits()
        {
            int hitCount;
            
            if (useCircleHitbox)
            {
                hitCount = Physics2D.OverlapCircleNonAlloc(
                    attackPoint.position,
                    attackRange,
                    hitTargets,
                    targetLayerMask
                );
            }
            else
            {
                hitCount = Physics2D.OverlapBoxNonAlloc(
                    attackPoint.position,
                    attackBoxSize,
                    0f,
                    hitTargets,
                    targetLayerMask
                );
            }
            
            // Process all hit targets
            for (int i = 0; i < hitCount; i++)
            {
                ProcessHit(hitTargets[i]);
            }
            
            // Trigger hit event if we hit anything
            if (hitCount > 0)
            {
                onAttackHit?.Invoke();
            }
        }
        
        /// <summary>
        /// Process a hit against a specific target
        /// </summary>
        /// <param name="target">The collider that was hit</param>
        private void ProcessHit(Collider2D target)
        {
            // Don't hit ourselves
            if (target.gameObject == gameObject) return;
            
            GameObject targetObject = target.gameObject;
            
            // Apply damage if target is damageable
            IDamageable damageable = targetObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, gameObject);
            }
            
            // Apply knockback if target has a Rigidbody2D
            ApplyKnockback(targetObject);
            
            // Trigger target hit event
            onTargetHit?.Invoke(targetObject);
        }
        
        /// <summary>
        /// Apply knockback to a target
        /// </summary>
        /// <param name="target">Target to apply knockback to</param>
        private void ApplyKnockback(GameObject target)
        {
            if (knockbackForce <= 0f) return;
            
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb == null) return;
            
            // Calculate knockback direction
            Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
            
            // Apply knockback force
            targetRb.velocity = new Vector2(knockbackDirection.x * knockbackForce, targetRb.velocity.y);
            
            // Optionally, start a coroutine to reduce knockback over time
            if (knockbackDuration > 0f)
            {
                StartCoroutine(ReduceKnockbackOverTime(targetRb, knockbackDuration));
            }
        }
        
        /// <summary>
        /// Gradually reduce knockback effect over time
        /// </summary>
        private System.Collections.IEnumerator ReduceKnockbackOverTime(Rigidbody2D targetRb, float duration)
        {
            float timer = duration;
            Vector2 initialVelocity = targetRb.velocity;
            
            while (timer > 0f && targetRb != null)
            {
                timer -= Time.deltaTime;
                float t = timer / duration;
                
                // Reduce horizontal knockback velocity over time
                targetRb.velocity = new Vector2(
                    Mathf.Lerp(0f, initialVelocity.x, t),
                    targetRb.velocity.y
                );
                
                yield return null;
            }
        }
        
        #region Public Configuration Methods
        /// <summary>
        /// Set the attack damage
        /// </summary>
        /// <param name="damage">New damage value</param>
        public void SetAttackDamage(float damage)
        {
            attackDamage = Mathf.Max(0f, damage);
        }
        
        /// <summary>
        /// Set the attack range
        /// </summary>
        /// <param name="range">New range value</param>
        public void SetAttackRange(float range)
        {
            attackRange = Mathf.Max(0f, range);
        }
        
        /// <summary>
        /// Set the attack cooldown
        /// </summary>
        /// <param name="cooldown">New cooldown value in seconds</param>
        public void SetAttackCooldown(float cooldown)
        {
            attackCooldown = Mathf.Max(0f, cooldown);
        }
        
        /// <summary>
        /// Set the target layer mask for hit detection
        /// </summary>
        /// <param name="layerMask">New layer mask</param>
        public void SetTargetLayerMask(LayerMask layerMask)
        {
            targetLayerMask = layerMask;
        }
        #endregion
        
        #region Debug Visualization
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            
            // Draw attack range
            Gizmos.color = IsHitboxActive ? Color.red : Color.yellow;
            
            if (useCircleHitbox)
            {
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
                Gizmos.matrix = Matrix4x4.identity;
            }
            
            // Draw attack point
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, 0.1f);
        }
        #endregion
    }
}