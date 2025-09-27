using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    public float damageAmount = 1f;

    [Header("Hit Feedback")]
    [SerializeField] private float hitstopDuration = 0.5f; // Duration of hitstop in seconds

    private static bool isHitstopActive = false; // To prevent multiple hitstops

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
                damageable.TakeDamage(damageAmount, playerPosition);
                // 3. Shake the camera on hit
                CameraShaker.Instance?.HitShake();
                // 4. Hitstop for better hit feeling
                if (!isHitstopActive)
                {
                    StartCoroutine(HitstopCoroutine());
                }
            }
        }
    }

    private IEnumerator HitstopCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        isHitstopActive = true;
        Time.timeScale = 0f;
        // Wait for real time, not affected by timescale
        yield return new WaitForSecondsRealtime(hitstopDuration);
        Time.timeScale = originalTimeScale;
        isHitstopActive = false;
    }
}