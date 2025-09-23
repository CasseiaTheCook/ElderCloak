using UnityEngine;
using UnityEngine.Events;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for objects with health systems
    /// Provides a standardized health management contract
    /// </summary>
    public interface IHealth
    {
        /// <summary>
        /// Current health value
        /// </summary>
        float CurrentHealth { get; }
        
        /// <summary>
        /// Maximum health value
        /// </summary>
        float MaxHealth { get; }
        
        /// <summary>
        /// Whether the object is currently alive (health > 0)
        /// </summary>
        bool IsAlive { get; }
        
        /// <summary>
        /// Event triggered when health changes
        /// Parameters: currentHealth, maxHealth
        /// </summary>
        UnityEvent<float, float> OnHealthChanged { get; }
        
        /// <summary>
        /// Event triggered when the object dies
        /// </summary>
        UnityEvent OnDeath { get; }
        
        /// <summary>
        /// Heal the object by the specified amount
        /// </summary>
        /// <param name="healAmount">Amount to heal</param>
        void Heal(float healAmount);
        
        /// <summary>
        /// Set the object's health to maximum
        /// </summary>
        void RestoreFullHealth();
        
        /// <summary>
        /// Set the maximum health and optionally adjust current health
        /// </summary>
        /// <param name="newMaxHealth">New maximum health value</param>
        /// <param name="adjustCurrentHealth">Whether to scale current health proportionally</param>
        void SetMaxHealth(float newMaxHealth, bool adjustCurrentHealth = false);
    }
}