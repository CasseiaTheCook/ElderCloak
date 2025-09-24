using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private void Awake()
    {
        // Find the PlayerAttack script on the parent object
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // First, check if the object we hit is tagged as an "Enemy"
        if (other.transform.parent.CompareTag("Enemy"))
        {
            // Then, check if it can be damaged
            IDamageable damageable = other.transform.parent.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // 1. Tell the player attack script that we hit something
                playerAttack?.OnSuccessfulHit();
                // 2. Deal damage to the enemy, passing the player's position for knockback
                Vector2 playerPosition = transform.parent.position;
                damageable.TakeDamage(1, playerPosition);
            }
        }
    }
}
