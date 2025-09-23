using UnityEngine;
using ElderCloak.Interfaces;

namespace ElderCloak.Enemy
{
    /// <summary>
    /// Abstract base class for all enemy types
    /// Provides common functionality and enforces a consistent structure for enemies
    /// </summary>
    public abstract class BaseEnemy : MonoBehaviour, IDamageable
    {
        [Header("Base Enemy Settings")]
        [SerializeField] protected float health = 50f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float detectionRange = 5f;
        [SerializeField] protected LayerMask playerLayerMask = 1;
        
        [Header("Behavior Settings")]
        [SerializeField] protected bool canAttackPlayer = true;
        [SerializeField] protected float aggroRange = 3f;
        [SerializeField] protected float deAggroRange = 8f;
        
        // Components (protected for derived classes)
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected Collider2D enemyCollider;
        protected HealthComponent healthComponent;
        
        // State tracking
        protected Transform player;
        protected bool isPlayerInRange;
        protected bool isAggro;
        protected bool isAlive = true;
        
        /// <summary>
        /// Current enemy health
        /// </summary>
        public float Health => healthComponent != null ? healthComponent.CurrentHealth : health;
        
        /// <summary>
        /// Whether the enemy is alive
        /// </summary>
        public bool IsAlive => isAlive && (healthComponent == null || healthComponent.IsAlive);
        
        /// <summary>
        /// Whether the player is within detection range
        /// </summary>
        public bool IsPlayerInRange => isPlayerInRange;
        
        /// <summary>
        /// Whether the enemy is currently aggressive toward the player
        /// </summary>
        public bool IsAggro => isAggro;
        
        /// <summary>
        /// Reference to the player transform
        /// </summary>
        public Transform Player => player;
        
        protected virtual void Awake()
        {
            InitializeComponents();
            SetupHealthComponent();
        }
        
        protected virtual void Start()
        {
            FindPlayer();
            OnEnemyStart();
        }
        
        protected virtual void Update()
        {
            if (!IsAlive) return;
            
            UpdatePlayerDetection();
            UpdateBehavior();
        }
        
        protected virtual void FixedUpdate()
        {
            if (!IsAlive) return;
            
            UpdateMovement();
        }
        
        /// <summary>
        /// Initialize required components
        /// </summary>
        protected virtual void InitializeComponents()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            enemyCollider = GetComponent<Collider2D>();
            healthComponent = GetComponent<HealthComponent>();
            
            // Create Rigidbody2D if it doesn't exist
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
                rb.freezeRotation = true;
            }
        }
        
        /// <summary>
        /// Set up the health component if it exists
        /// </summary>
        protected virtual void SetupHealthComponent()
        {
            if (healthComponent != null)
            {
                healthComponent.OnDeath.AddListener(OnDeath);
                healthComponent.OnDamageTaken.AddListener(OnDamageTaken);
            }
        }
        
        /// <summary>
        /// Find the player in the scene
        /// </summary>
        protected virtual void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        
        /// <summary>
        /// Update player detection and aggro state
        /// </summary>
        protected virtual void UpdatePlayerDetection()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            // Check if player is in detection range
            isPlayerInRange = distanceToPlayer <= detectionRange;
            
            // Update aggro state
            if (!isAggro && distanceToPlayer <= aggroRange)
            {
                isAggro = true;
                OnPlayerEnterAggro();
            }
            else if (isAggro && distanceToPlayer > deAggroRange)
            {
                isAggro = false;
                OnPlayerExitAggro();
            }
        }
        
        /// <summary>
        /// Update sprite facing direction based on movement or player position
        /// </summary>
        protected virtual void UpdateFacing()
        {
            if (spriteRenderer == null) return;
            
            Vector2 facingDirection = GetFacingDirection();
            
            if (Mathf.Abs(facingDirection.x) > 0.1f)
            {
                spriteRenderer.flipX = facingDirection.x < 0;
            }
        }
        
        /// <summary>
        /// Get the direction the enemy should face
        /// </summary>
        /// <returns>The facing direction vector</returns>
        protected virtual Vector2 GetFacingDirection()
        {
            if (isAggro && player != null)
            {
                return (player.position - transform.position).normalized;
            }
            
            return rb.velocity.normalized;
        }
        
        #region IDamageable Implementation
        public virtual void TakeDamage(float damage, GameObject damageSource = null)
        {
            if (!IsAlive) return;
            
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage, damageSource);
            }
            else
            {
                // Fallback health system
                health = Mathf.Max(0f, health - damage);
                if (health <= 0f)
                {
                    OnDeath();
                }
            }
        }
        
        public virtual bool CanTakeDamage()
        {
            return IsAlive && (healthComponent == null || healthComponent.CanTakeDamage());
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// Called when the enemy dies
        /// </summary>
        protected virtual void OnDeath()
        {
            isAlive = false;
            OnEnemyDeath();
        }
        
        /// <summary>
        /// Called when the enemy takes damage
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        /// <param name="source">Source of the damage</param>
        protected virtual void OnDamageTaken(float damage, GameObject source)
        {
            OnEnemyDamageTaken(damage, source);
        }
        
        /// <summary>
        /// Called when player enters aggro range
        /// </summary>
        protected virtual void OnPlayerEnterAggro()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when player exits aggro range
        /// </summary>
        protected virtual void OnPlayerExitAggro()
        {
            // Override in derived classes
        }
        #endregion
        
        #region Abstract Methods (Must be implemented by derived classes)
        /// <summary>
        /// Called during Start - implement enemy-specific initialization
        /// </summary>
        protected abstract void OnEnemyStart();
        
        /// <summary>
        /// Update enemy behavior - implement AI logic here
        /// </summary>
        protected abstract void UpdateBehavior();
        
        /// <summary>
        /// Update enemy movement - implement movement logic here
        /// </summary>
        protected abstract void UpdateMovement();
        
        /// <summary>
        /// Called when enemy dies - implement death behavior
        /// </summary>
        protected abstract void OnEnemyDeath();
        
        /// <summary>
        /// Called when enemy takes damage - implement damage reaction
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        /// <param name="source">Source of the damage</param>
        protected abstract void OnEnemyDamageTaken(float damage, GameObject source);
        #endregion
        
        #region Debug Visualization
        protected virtual void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw aggro range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            
            // Draw de-aggro range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, deAggroRange);
        }
        #endregion
    }
}