using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for objects that can perform attacks.
    /// This provides a standardized way to handle attack behavior across different entities.
    /// </summary>
    public interface IAttackable
    {
        /// <summary>
        /// Perform an attack action.
        /// </summary>
        /// <param name="direction">The direction of the attack</param>
        void Attack(Vector2 direction);

        /// <summary>
        /// Check if this object can currently attack.
        /// </summary>
        /// <returns>True if can attack, false otherwise</returns>
        bool CanAttack();

        /// <summary>
        /// Get the attack damage value.
        /// </summary>
        /// <returns>The damage this attack deals</returns>
        float GetAttackDamage();

        /// <summary>
        /// Get the attack range.
        /// </summary>
        /// <returns>The range of this attack</returns>
        float GetAttackRange();
    }
}