using UnityEngine;
using ElderCloak.Combat;

namespace ElderCloak.Enemy
{
    /// <summary>
    /// Basic melee enemy that chases the player and attacks when in range
    /// Demonstrates how to extend the BaseEnemy class for specific enemy types
    /// </summary>
    public class BasicMeleeEnemy : BaseEnemy
    {
        [Header("Melee Enemy Settings")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float patrolSpeed = 1f;
        [SerializeField] private float patrolDistance = 3f;
        
        [Header("Patrol Settings")]
        [SerializeField] private bool enablePatrol = true;
        [SerializeField] private Transform[] patrolPoints;
        
        // Components
        private MeleeAttackSystem attackSystem;
        
        // State
        private Vector3 startPosition;
        private int currentPatrolIndex = 0;
        private float lastAttackTime;
        private bool isChasing;
        
        // Movement
        private Vector2 patrolDirection = Vector2.right;
        private float patrolTimer;
        
        /// <summary>
        /// Whether the enemy is currently chasing the player
        /// </summary>
        public bool IsChasing => isChasing;
        
        /// <summary>
        /// Whether the enemy can attack the player
        /// </summary>
        public bool CanAttackPlayer => Time.time >= lastAttackTime + attackCooldown;
        
        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            attackSystem = GetComponent<MeleeAttackSystem>();
            
            // Add MeleeAttackSystem if it doesn't exist
            if (attackSystem == null && canAttackPlayer)
            {
                attackSystem = gameObject.AddComponent<MeleeAttackSystem>();
            }
        }
        
        protected override void OnEnemyStart()
        {
            startPosition = transform.position;
            
            // Set up attack system
            if (attackSystem != null)
            {
                attackSystem.OnAttackStart.AddListener(OnAttackStart);
                attackSystem.OnTargetHit.AddListener(OnTargetHit);
            }
            
            // Initialize patrol
            if (enablePatrol && patrolPoints.Length == 0)
            {
                // Create simple back-and-forth patrol if no patrol points are set
                patrolDirection = Vector2.right;
            }
        }
        
        protected override void UpdateBehavior()
        {
            if (!IsAlive) return;
            
            if (isAggro)
            {
                UpdateChaseAndAttackBehavior();
            }
            else
            {
                UpdatePatrolBehavior();
            }
            
            UpdateFacing();
        }
        
        /// <summary>
        /// Handle chase and attack behavior when player is in aggro range
        /// </summary>
        private void UpdateChaseAndAttackBehavior()
        {
            isChasing = true;
            
            if (player == null) return;
            
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            // Attack if in range and can attack
            if (distanceToPlayer <= attackRange && CanAttackPlayer && canAttackPlayer)
            {
                TryAttackPlayer();
            }
        }
        
        /// <summary>
        /// Handle patrol behavior when player is not in aggro range
        /// </summary>
        private void UpdatePatrolBehavior()
        {
            isChasing = false;
            
            if (!enablePatrol) return;
            
            if (patrolPoints.Length > 0)
            {
                UpdatePatrolPointMovement();
            }
            else
            {
                UpdateSimplePatrol();
            }
        }
        
        /// <summary>
        /// Update movement between defined patrol points
        /// </summary>
        private void UpdatePatrolPointMovement()
        {
            if (currentPatrolIndex >= patrolPoints.Length) return;
            
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            if (targetPoint == null) return;
            
            float distanceToTarget = Vector2.Distance(transform.position, targetPoint.position);
            
            if (distanceToTarget < 0.5f)
            {
                // Reached patrol point, move to next one
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        
        /// <summary>
        /// Update simple back-and-forth patrol
        /// </summary>
        private void UpdateSimplePatrol()
        {
            float distanceFromStart = Vector2.Distance(transform.position, startPosition);
            
            // Check if we need to turn around
            if (distanceFromStart > patrolDistance)
            {
                patrolDirection = (startPosition - transform.position).normalized;
            }
            
            // Occasionally change direction for more natural movement
            patrolTimer += Time.deltaTime;
            if (patrolTimer > Random.Range(2f, 4f))
            {
                patrolTimer = 0f;
                patrolDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;
            }
        }
        
        protected override void UpdateMovement()
        {
            if (!IsAlive) return;
            
            Vector2 targetVelocity = Vector2.zero;
            
            if (isChasing && player != null)
            {
                // Chase player
                Vector2 directionToPlayer = (player.position - transform.position).normalized;
                targetVelocity = directionToPlayer * chaseSpeed;
            }
            else if (enablePatrol)
            {
                // Patrol movement
                if (patrolPoints.Length > 0 && currentPatrolIndex < patrolPoints.Length)
                {
                    Vector2 directionToPatrol = (patrolPoints[currentPatrolIndex].position - transform.position).normalized;
                    targetVelocity = directionToPatrol * patrolSpeed;
                }
                else
                {
                    targetVelocity = patrolDirection * patrolSpeed;
                }
            }
            
            // Apply movement
            rb.velocity = new Vector2(targetVelocity.x, rb.velocity.y);
        }
        
        /// <summary>
        /// Attempt to attack the player
        /// </summary>
        private void TryAttackPlayer()
        {
            if (attackSystem != null && attackSystem.CanAttack)
            {
                attackSystem.PerformAttack();
                lastAttackTime = Time.time;
            }
        }
        
        protected override void OnPlayerEnterAggro()
        {
            base.OnPlayerEnterAggro();
            // Could add audio/visual effects here
        }
        
        protected override void OnPlayerExitAggro()
        {
            base.OnPlayerExitAggro();
            isChasing = false;
        }
        
        protected override void OnEnemyDeath()
        {
            // Disable components
            if (enemyCollider != null)
                enemyCollider.enabled = false;
            
            if (rb != null)
                rb.velocity = Vector2.zero;
            
            // Disable this script
            enabled = false;
            
            // Could add death effects, drop items, etc.
            // Destroy(gameObject, 2f); // Destroy after 2 seconds
        }
        
        protected override void OnEnemyDamageTaken(float damage, GameObject source)
        {
            // React to damage - could add knockback, change color, etc.
            if (source != null && source.CompareTag("Player"))
            {
                // Become aggressive if attacked by player
                if (!isAggro)
                {
                    isAggro = true;
                    OnPlayerEnterAggro();
                }
            }
        }
        
        #region Attack Event Handlers
        private void OnAttackStart()
        {
            // Stop movement during attack
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        
        private void OnTargetHit(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                // Successfully hit the player
            }
        }
        #endregion
        
        #region Debug
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw patrol distance
            if (enablePatrol)
            {
                Gizmos.color = Color.green;
                Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
                Gizmos.DrawWireSphere(startPos, patrolDistance);
                
                // Draw patrol points
                if (patrolPoints != null)
                {
                    Gizmos.color = Color.cyan;
                    for (int i = 0; i < patrolPoints.Length; i++)
                    {
                        if (patrolPoints[i] != null)
                        {
                            Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);
                            
                            // Draw lines between patrol points
                            if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                            {
                                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}