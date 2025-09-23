using UnityEngine;
using ElderCloak.Player.Health;
using ElderCloak.Player.Movement;
using ElderCloak.Player.Combat;

namespace ElderCloak.Player
{
    /// <summary>
    /// Main player controller that coordinates all player systems
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private PlayerController2D movement;
        [SerializeField] private MeleeAttack meleeAttack;
        
        [Header("Death Settings")]
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private Vector3 respawnPosition;
        
        private bool isDead;
        
        private void Awake()
        {
            // Auto-find components if not assigned
            if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
            if (movement == null) movement = GetComponent<PlayerController2D>();
            if (meleeAttack == null) meleeAttack = GetComponent<MeleeAttack>();
            
            // Store initial position as respawn point
            respawnPosition = transform.position;
        }
        
        private void Start()
        {
            // Subscribe to health system events
            if (healthSystem != null)
            {
                healthSystem.OnDeath.AddListener(HandlePlayerDeath);
                healthSystem.OnRevive.AddListener(HandlePlayerRevive);
                healthSystem.OnDamageTaken.AddListener(HandleDamageTaken);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (healthSystem != null)
            {
                healthSystem.OnDeath.RemoveListener(HandlePlayerDeath);
                healthSystem.OnRevive.RemoveListener(HandlePlayerRevive);
                healthSystem.OnDamageTaken.RemoveListener(HandleDamageTaken);
            }
        }
        
        private void HandlePlayerDeath()
        {
            isDead = true;
            
            // Stop movement
            if (movement != null)
            {
                movement.StopMovement();
                movement.enabled = false;
            }
            
            // Disable attacks
            if (meleeAttack != null)
            {
                meleeAttack.enabled = false;
            }
            
            // Optional: Play death animation, sound effects, etc.
            Debug.Log("Player has died!");
            
            // Respawn after delay
            Invoke(nameof(RespawnPlayer), respawnDelay);
        }
        
        private void HandlePlayerRevive()
        {
            isDead = false;
            
            // Re-enable movement
            if (movement != null)
            {
                movement.enabled = true;
            }
            
            // Re-enable attacks
            if (meleeAttack != null)
            {
                meleeAttack.enabled = true;
            }
            
            Debug.Log("Player has been revived!");
        }
        
        private void HandleDamageTaken(int damage, Transform damageSource)
        {
            // Optional: Screen shake, damage effects, sound, etc.
            Debug.Log($"Player took {damage} damage from {(damageSource ? damageSource.name : "unknown source")}!");
        }
        
        private void RespawnPlayer()
        {
            if (!isDead) return;
            
            // Reset position
            transform.position = respawnPosition;
            
            // Reset velocity
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            // Revive with full health
            if (healthSystem != null)
            {
                healthSystem.Revive();
            }
        }
        
        public void SetRespawnPoint(Vector3 newRespawnPoint)
        {
            respawnPosition = newRespawnPoint;
        }
    }
}