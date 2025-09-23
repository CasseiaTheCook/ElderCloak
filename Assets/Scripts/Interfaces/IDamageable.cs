using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for objects that can take damage
    /// Provides a contract for damage handling across different game entities
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this object
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="damageSource">The source of the damage (optional)</param>
        void TakeDamage(float damage, GameObject damageSource = null);
        
        /// <summary>
        /// Check if the object is currently able to take damage
        /// </summary>
        /// <returns>True if the object can take damage, false otherwise</returns>
        bool CanTakeDamage();
    }
}