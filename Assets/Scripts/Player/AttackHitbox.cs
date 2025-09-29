using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    public float damageAmount = 1f;

    [Header("Hit Feedback")]
    [SerializeField] private float hitstopDuration = 0.5f; // Duration of hitstop in seconds

    private Coroutine hitstopCoroutine;
    private float hitstopTimer = 0f;

    private void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.parent.CompareTag("Enemy"))
        {
            IDamageable damageable = other.transform.parent.GetComponent<IDamageable>();
            if (damageable != null)
            {
                playerAttack?.OnSuccessfulHit();
                Vector2 playerPosition = transform.parent.position;
                damageable.TakeDamage(damageAmount, playerPosition);
                CameraShaker.Instance?.HitShake();
                StartHitstop();
            }
        }
    }

    private void StartHitstop()
    {
        // Eðer hitstop zaten aktifse, kalan süreyi uzat
        if (hitstopCoroutine != null)
        {
            hitstopTimer = Mathf.Max(hitstopTimer, hitstopDuration);
        }
        else
        {
            hitstopTimer = hitstopDuration;
            hitstopCoroutine = StartCoroutine(HitstopCoroutine());
        }
    }

    private IEnumerator HitstopCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        while (hitstopTimer > 0f)
        {
            yield return null;
            hitstopTimer -= Time.unscaledDeltaTime;
        }
        Time.timeScale = originalTimeScale;
        hitstopCoroutine = null;
    }
}