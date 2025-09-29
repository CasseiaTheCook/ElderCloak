using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float hitShakeDuration = 0.15f;
    [SerializeField] private float hitShakeMagnitude = 0.2f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Call this when you hit an enemy to shake the camera.
    /// </summary>
    public void HitShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition;
        }
        shakeCoroutine = StartCoroutine(Shake(hitShakeDuration, hitShakeMagnitude));
    }

    private System.Collections.IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 randomPoint = originalPosition + (Vector3)Random.insideUnitCircle * magnitude;
            transform.localPosition = randomPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}