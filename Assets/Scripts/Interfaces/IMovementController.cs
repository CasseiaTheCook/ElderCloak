using UnityEngine;

namespace ElderCloak.Interfaces
{
    /// <summary>
    /// Interface for character movement controllers.
    /// Provides standardized movement capabilities for different character types.
    /// </summary>
    public interface IMovementController
    {
        /// <summary>
        /// Move the character in the specified direction.
        /// </summary>
        /// <param name="direction">Movement direction (normalized)</param>
        void Move(Vector2 direction);

        /// <summary>
        /// Make the character jump.
        /// </summary>
        void Jump();

        /// <summary>
        /// Make the character dash in a direction.
        /// </summary>
        /// <param name="direction">Dash direction</param>
        void Dash(Vector2 direction);

        /// <summary>
        /// Check if the character is currently on the ground.
        /// </summary>
        /// <returns>True if grounded, false otherwise</returns>
        bool IsGrounded();

        /// <summary>
        /// Check if the character can currently jump.
        /// </summary>
        /// <returns>True if can jump, false otherwise</returns>
        bool CanJump();

        /// <summary>
        /// Check if the character can currently dash.
        /// </summary>
        /// <returns>True if can dash, false otherwise</returns>
        bool CanDash();

        /// <summary>
        /// Get the current velocity of the character.
        /// </summary>
        /// <returns>Current velocity vector</returns>
        Vector2 GetVelocity();

        /// <summary>
        /// Set the character's velocity directly.
        /// </summary>
        /// <param name="velocity">New velocity vector</param>
        void SetVelocity(Vector2 velocity);
    }
}