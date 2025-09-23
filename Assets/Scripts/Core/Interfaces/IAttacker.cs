using UnityEngine;

namespace ElderCloak.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can attack
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// Damage dealt by this attacker
        /// </summary>
        int Damage { get; }
        
        /// <summary>
        /// Perform an attack
        /// </summary>
        void Attack();
        
        /// <summary>
        /// Check if the attacker can currently attack
        /// </summary>
        bool CanAttack();
    }
}