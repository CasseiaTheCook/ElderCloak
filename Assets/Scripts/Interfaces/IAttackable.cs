using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for objects that can perform attacks
    /// Provides a contract for attack behavior across different game entities
    /// </summary>
    public interface IAttackable
    {
        /// <summary>
        /// Whether the object can currently attack
        /// </summary>
        bool CanAttack { get; }
        
        /// <summary>
        /// Whether the object is currently in an attack animation/state
        /// </summary>
        bool IsAttacking { get; }
        
        /// <summary>
        /// The damage this object deals with its attack
        /// </summary>
        float AttackDamage { get; }
        
        /// <summary>
        /// The range of the attack
        /// </summary>
        float AttackRange { get; }
        
        /// <summary>
        /// Execute an attack
        /// </summary>
        void PerformAttack();
        
        /// <summary>
        /// Cancel the current attack if possible
        /// </summary>
        void CancelAttack();
    }
}