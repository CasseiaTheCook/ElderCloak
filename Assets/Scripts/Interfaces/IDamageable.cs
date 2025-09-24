// Author: Copilot
// Version: 1.0
// Purpose: Interface for objects that can take damage.



using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount, Vector2 position);
}