using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    public float damageAmount = 1f;

    [Header("Hit Feedback")]
    [SerializeField] private float hitstopDuration = 0.5f; // Duration of hitstop in seconds

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
                StartCoroutine(HitstopCoroutine());
            }
        }
    }

    private IEnumerator HitstopCoroutine()
    {
        // Only apply hitstop if the game is not paused
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
        {
            yield break;
        }

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        // Wait for real time, not affected by timescale
        yield return new WaitForSecondsRealtime(hitstopDuration);
        
        // Only restore timescale if the game is not paused
        if (PauseMenuManager.Instance == null || !PauseMenuManager.Instance.IsPaused)
        {
            Time.timeScale = originalTimeScale;
        }
    }
}