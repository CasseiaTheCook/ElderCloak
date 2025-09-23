using UnityEngine;
using ElderCloak.Interfaces;
using ElderCloak.Health;
using ElderCloak.Combat;

namespace ElderCloak.Enemy
{
    /// <summary>
    /// Basic enemy controller with health, damage handling, and AI behavior.
    /// Serves as a base class for more specific enemy types.
    /// Demonstrates integration with the health and combat systems.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BasicEnemyController : MonoBehaviour, IDamageable, IAttackable
    {
        [Header("Enemy Configuration")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private LayerMask playerLayerMask = -1;

        [Header("AI Behavior")]
        [SerializeField] private EnemyBehaviorType behaviorType = EnemyBehaviorType.Patrol;
        [SerializeField] private float patrolDistance = 4f;
        [SerializeField] private float waitTime = 2f;
        [SerializeField] private bool canFly = false;

        [Header("Combat")]
        [SerializeField] private bool canAttack = true;
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Death Behavior")]
        [SerializeField] private float deathDuration = 1f;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private bool destroyOnDeath = true;

        // Component references
        private Rigidbody2D rb;
        private Collider2D col;
        private HealthSystem healthSystem;
        private MeleeAttackSystem attackSystem;

        // AI state
        private Transform target;
        private Vector2 startPosition;
        private bool movingRight = true;
        private float waitTimer;
        private bool isWaiting;

        // Combat state
        private float lastAttackTime;

        // State tracking
        private bool isDead = false;
        private bool isFacingRight = true;

        public enum EnemyBehaviorType
        {
            Idle,
            Patrol,
            Chase,
            Guard
        }

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            
            // Initialize health system
            healthSystem = new HealthSystem();
            healthSystem.Initialize(this);
            
            // Subscribe to health events
            healthSystem.OnDeath += HandleDeath;
            healthSystem.OnDamageTaken += HandleDamageTaken;

            // Initialize attack system if enemy can attack
            if (canAttack)
            {
                attackSystem = new MeleeAttackSystem();
                attackSystem.Initialize(this);
                attackSystem.OnTargetHit += HandleTargetHit;
            }

            startPosition = transform.position;
        }

        private void Update()
        {
            if (isDead) return;

            healthSystem.Update();
            
            if (attackSystem != null)
                attackSystem.Update();

            UpdateAI();
        }

        private void FixedUpdate()
        {
            if (isDead) return;
            
            UpdateMovement();
        }

        #endregion

        #region AI Behavior

        /// <summary>
        /// Update AI behavior based on current behavior type.
        /// </summary>
        private void UpdateAI()
        {
            // Find player target
            FindPlayerTarget();

            switch (behaviorType)
            {
                case EnemyBehaviorType.Idle:
                    HandleIdleBehavior();
                    break;
                case EnemyBehaviorType.Patrol:
                    HandlePatrolBehavior();
                    break;
                case EnemyBehaviorType.Chase:
                    HandleChaseBehavior();
                    break;
                case EnemyBehaviorType.Guard:
                    HandleGuardBehavior();
                    break;
            }

            // Handle combat
            if (canAttack && target != null)
            {
                HandleCombat();
            }
        }

        /// <summary>
        /// Find the player target within detection range.
        /// </summary>
        private void FindPlayerTarget()
        {
            Collider2D playerCollider = Physics2D.OverlapCircle(
                transform.position, 
                detectionRange, 
                playerLayerMask
            );

            if (playerCollider != null)
            {
                target = playerCollider.transform;
            }
            else if (Vector2.Distance(transform.position, target != null ? target.position : startPosition) > detectionRange * 1.5f)
            {
                target = null;
            }
        }

        private void HandleIdleBehavior()
        {
            // Do nothing, just stand there
        }

        private void HandlePatrolBehavior()
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    isWaiting = false;
                    movingRight = !movingRight;
                }
                return;
            }

            float distanceFromStart = transform.position.x - startPosition.x;
            
            if (movingRight && distanceFromStart >= patrolDistance)
            {
                isWaiting = true;
                waitTimer = waitTime;
            }
            else if (!movingRight && distanceFromStart <= -patrolDistance)
            {
                isWaiting = true;
                waitTimer = waitTime;
            }
        }

        private void HandleChaseBehavior()
        {
            if (target != null)
            {
                Vector2 direction = (target.position - transform.position).normalized;
                movingRight = direction.x > 0;
            }
        }

        private void HandleGuardBehavior()
        {
            if (target != null)
            {
                // Face the target but don't move
                Vector2 direction = target.position - transform.position;
                movingRight = direction.x > 0;
            }
            else
            {
                // Return to start position if far away
                if (Vector2.Distance(transform.position, startPosition) > 1f)
                {
                    Vector2 direction = (startPosition - (Vector2)transform.position).normalized;
                    movingRight = direction.x > 0;
                }
            }
        }

        #endregion

        #region Movement

        /// <summary>
        /// Update movement based on AI decisions.
        /// </summary>
        private void UpdateMovement()
        {
            Vector2 movement = Vector2.zero;

            // Determine movement direction based on behavior
            switch (behaviorType)
            {
                case EnemyBehaviorType.Patrol:
                    if (!isWaiting)
                        movement.x = movingRight ? 1 : -1;
                    break;

                case EnemyBehaviorType.Chase:
                    if (target != null)
                    {
                        Vector2 direction = (target.position - transform.position).normalized;
                        movement = direction;
                        
                        // Don't move if too close (within attack range)
                        if (Vector2.Distance(transform.position, target.position) <= attackRange * 0.8f)
                            movement = Vector2.zero;
                    }
                    break;

                case EnemyBehaviorType.Guard:
                    if (target == null && Vector2.Distance(transform.position, startPosition) > 1f)
                    {
                        Vector2 direction = (startPosition - (Vector2)transform.position).normalized;
                        movement = direction;
                    }
                    break;
            }

            // Apply movement
            if (!canFly)
            {
                // Ground-based movement
                rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
            }
            else
            {
                // Flying movement
                rb.velocity = movement * moveSpeed;
            }

            // Handle sprite flipping
            if (movement.x > 0 && !isFacingRight) Flip();
            else if (movement.x < 0 && isFacingRight) Flip();
        }

        /// <summary>
        /// Flip the enemy sprite horizontally.
        /// </summary>
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }

        #endregion

        #region Combat

        /// <summary>
        /// Handle combat behavior when target is in range.
        /// </summary>
        private void HandleCombat()
        {
            if (target == null || !canAttack) return;

            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Vector2 attackDirection = (target.position - transform.position).normalized;
                Attack(attackDirection);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle death event from health system.
        /// </summary>
        private void HandleDeath()
        {
            if (isDead) return;

            isDead = true;
            
            // Disable collider and rigidbody
            col.enabled = false;
            rb.simulated = false;

            // Spawn death effects
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Handle destruction
            if (destroyOnDeath)
            {
                Destroy(gameObject, deathDuration);
            }

            Debug.Log($"{name} has died!");
        }

        /// <summary>
        /// Handle damage taken event.
        /// </summary>
        private void HandleDamageTaken(float damage, GameObject source)
        {
            Debug.Log($"{name} took {damage} damage from {(source != null ? source.name : "unknown source")}");
            
            // Switch to chase behavior when damaged if not already aggressive
            if (behaviorType == EnemyBehaviorType.Idle || behaviorType == EnemyBehaviorType.Patrol)
            {
                behaviorType = EnemyBehaviorType.Chase;
            }
        }

        /// <summary>
        /// Handle when this enemy hits a target.
        /// </summary>
        private void HandleTargetHit(GameObject target)
        {
            Debug.Log($"{name} hit {target.name}");
        }

        #endregion

        #region IDamageable Implementation

        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            if (isDead) return;
            healthSystem.TakeDamage(damage, damageSource);
        }

        public bool IsAlive() => healthSystem.IsAlive();

        public float GetHealthPercentage() => healthSystem.GetHealthPercentage();

        #endregion

        #region IAttackable Implementation

        public void Attack(Vector2 direction)
        {
            if (attackSystem == null || !CanAttack()) return;

            lastAttackTime = Time.time;
            attackSystem.Attack(direction);
        }

        public bool CanAttack()
        {
            if (isDead || !canAttack || attackSystem == null) return false;
            return Time.time >= lastAttackTime + attackCooldown;
        }

        public float GetAttackDamage() => attackDamage;
        public float GetAttackRange() => attackRange;

        #endregion

        #region Public API

        /// <summary>
        /// Get the health system for external access.
        /// </summary>
        public HealthSystem GetHealthSystem() => healthSystem;

        /// <summary>
        /// Set the enemy's behavior type.
        /// </summary>
        public void SetBehaviorType(EnemyBehaviorType newBehavior)
        {
            behaviorType = newBehavior;
        }

        /// <summary>
        /// Get the current target.
        /// </summary>
        public Transform GetTarget() => target;

        /// <summary>
        /// Manually set a target.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Check if the enemy is dead.
        /// </summary>
        public bool IsDead() => isDead;

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw patrol area
            if (behaviorType == EnemyBehaviorType.Patrol)
            {
                Vector3 patrolCenter = Application.isPlaying ? startPosition : transform.position;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(patrolCenter, new Vector3(patrolDistance * 2, 1, 1));
            }

            // Draw line to target
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        #endregion
    }
}