using UnityEngine;

namespace ElderCloak.Core.Interfaces
{
    /// <summary>
    /// Interface for moveable objects
    /// </summary>
    public interface IMoveable
    {
        /// <summary>
        /// Current velocity of the object
        /// </summary>
        Vector2 Velocity { get; }
        
        /// <summary>
        /// Whether the object is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Move the object in a direction
        /// </summary>
        /// <param name="direction">Direction to move</param>
        void Move(Vector2 direction);
        
        /// <summary>
        /// Stop all movement
        /// </summary>
        void StopMovement();
    }
}