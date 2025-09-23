using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for objects that can take damage.
    /// This provides a standardized way to handle damage across all game entities.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this object.
        /// </summary>
        /// <param name="damage">The amount of damage to apply</param>
        /// <param name="damageSource">The source of the damage (optional)</param>
        void TakeDamage(float damage, GameObject damageSource = null);

        /// <summary>
        /// Check if this object is currently alive/active.
        /// </summary>
        /// <returns>True if alive, false if dead/destroyed</returns>
        bool IsAlive();

        /// <summary>
        /// Get the current health percentage (0-1).
        /// </summary>
        /// <returns>Health percentage as a float between 0 and 1</returns>
        float GetHealthPercentage();
    }
}