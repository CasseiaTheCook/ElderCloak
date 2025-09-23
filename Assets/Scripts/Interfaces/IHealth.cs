using System;
using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for health system management.
    /// Provides comprehensive health tracking and event handling.
    /// </summary>
    public interface IHealth
    {
        /// <summary>
        /// Event triggered when health changes.
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Event triggered when the entity dies.
        /// </summary>
        event Action OnDeath;

        /// <summary>
        /// Event triggered when damage is taken.
        /// Parameters: (damageAmount, damageSource)
        /// </summary>
        event Action<float, GameObject> OnDamageTaken;

        /// <summary>
        /// Event triggered when health is restored.
        /// Parameters: (healAmount)
        /// </summary>
        event Action<float> OnHealthRestored;

        /// <summary>
        /// Get the current health value.
        /// </summary>
        /// <returns>Current health</returns>
        float GetCurrentHealth();

        /// <summary>
        /// Get the maximum health value.
        /// </summary>
        /// <returns>Maximum health</returns>
        float GetMaxHealth();

        /// <summary>
        /// Set the maximum health and optionally adjust current health.
        /// </summary>
        /// <param name="maxHealth">New maximum health value</param>
        /// <param name="adjustCurrent">Whether to adjust current health proportionally</param>
        void SetMaxHealth(float maxHealth, bool adjustCurrent = false);

        /// <summary>
        /// Restore health by a specific amount.
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        void RestoreHealth(float amount);

        /// <summary>
        /// Restore health to maximum.
        /// </summary>
        void RestoreToFull();

        /// <summary>
        /// Check if the entity is currently alive.
        /// </summary>
        /// <returns>True if alive, false if dead</returns>
        bool IsAlive();

        /// <summary>
        /// Get health as a percentage (0-1).
        /// </summary>
        /// <returns>Health percentage</returns>
        float GetHealthPercentage();
    }
}