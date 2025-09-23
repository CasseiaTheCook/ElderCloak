using UnityEngine;

namespace ElderCloak.Systems
{
    /// <summary>
    /// Camera controller for following the player with smooth movement and screen shake effects.
    /// Designed for 2D platformer games with configurable follow behavior.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        [Header("Smoothing")]
        [SerializeField] private float smoothTimeX = 0.3f;
        [SerializeField] private float smoothTimeY = 0.3f;
        [SerializeField] private float maxSpeedX = 10f;
        [SerializeField] private float maxSpeedY = 10f;

        [Header("Boundaries")]
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-10, -5);
        [SerializeField] private Vector2 maxBounds = new Vector2(10, 5);

        [Header("Look Ahead")]
        [SerializeField] private bool enableLookAhead = true;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 2f;

        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 1f;
        [SerializeField] private float shakeDecay = 5f;

        // Internal state
        private Vector3 targetPosition;
        private Vector2 velocity;
        private Vector3 lookAheadOffset;
        private Vector3 shakeOffset;
        private float currentShakeIntensity;

        // Component references
        private Camera cam;

        #region Unity Lifecycle

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("CameraController requires a Camera component!");
            }
        }

        private void Start()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            UpdateLookAhead();
            UpdateTargetPosition();
            UpdateCameraPosition();
            UpdateScreenShake();
        }

        #endregion

        #region Camera Movement

        /// <summary>
        /// Update the look-ahead offset based on target movement.
        /// </summary>
        private void UpdateLookAhead()
        {
            if (!enableLookAhead) return;

            // Get target's movement direction (requires Rigidbody2D on target)
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 targetVelocity = targetRb.velocity;
                Vector3 desiredLookAhead = Vector3.zero;

                if (Mathf.Abs(targetVelocity.x) > 0.1f)
                {
                    desiredLookAhead.x = Mathf.Sign(targetVelocity.x) * lookAheadDistance;
                }

                lookAheadOffset = Vector3.Lerp(lookAheadOffset, desiredLookAhead, 
                                              lookAheadSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update the target position for the camera.
        /// </summary>
        private void UpdateTargetPosition()
        {
            targetPosition = target.position + offset + lookAheadOffset;

            // Apply boundaries if enabled
            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
        }

        /// <summary>
        /// Update the actual camera position with smoothing.
        /// </summary>
        private void UpdateCameraPosition()
        {
            Vector3 currentPos = transform.position;
            Vector3 newPos = currentPos;

            // Smooth follow on X axis
            if (followX)
            {
                newPos.x = Mathf.SmoothDamp(currentPos.x, targetPosition.x, 
                                           ref velocity.x, smoothTimeX, maxSpeedX);
            }

            // Smooth follow on Y axis  
            if (followY)
            {
                newPos.y = Mathf.SmoothDamp(currentPos.y, targetPosition.y, 
                                           ref velocity.y, smoothTimeY, maxSpeedY);
            }

            // Maintain Z position
            newPos.z = targetPosition.z;

            // Apply screen shake offset
            newPos += shakeOffset;

            transform.position = newPos;
        }

        #endregion

        #region Screen Shake

        /// <summary>
        /// Update screen shake effect.
        /// </summary>
        private void UpdateScreenShake()
        {
            if (currentShakeIntensity > 0)
            {
                // Generate random shake offset
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentShakeIntensity,
                    Random.Range(-1f, 1f) * currentShakeIntensity,
                    0
                );

                // Decay shake intensity over time
                currentShakeIntensity -= shakeDecay * Time.deltaTime;
                currentShakeIntensity = Mathf.Max(0, currentShakeIntensity);
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }

        /// <summary>
        /// Trigger a screen shake effect.
        /// </summary>
        /// <param name="intensity">Shake intensity (0-1)</param>
        /// <param name="duration">Shake duration in seconds</param>
        public void Shake(float intensity = 1f, float duration = 0.5f)
        {
            currentShakeIntensity = intensity * shakeIntensity;
            
            // If we want a specific duration, we can calculate the required decay rate
            if (duration > 0)
            {
                shakeDecay = currentShakeIntensity / duration;
            }
        }

        /// <summary>
        /// Stop screen shake immediately.
        /// </summary>
        public void StopShake()
        {
            currentShakeIntensity = 0f;
            shakeOffset = Vector3.zero;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the camera target.
        /// </summary>
        /// <param name="newTarget">New target to follow</param>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                // Immediately move to target position
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Set camera boundaries.
        /// </summary>
        /// <param name="min">Minimum bounds</param>
        /// <param name="max">Maximum bounds</param>
        public void SetBoundaries(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBoundaries = true;
        }

        /// <summary>
        /// Disable camera boundaries.
        /// </summary>
        public void DisableBoundaries()
        {
            useBoundaries = false;
        }

        /// <summary>
        /// Set camera follow speed.
        /// </summary>
        /// <param name="speed">New follow speed</param>
        public void SetFollowSpeed(float speed)
        {
            followSpeed = speed;
        }

        /// <summary>
        /// Set camera offset from target.
        /// </summary>
        /// <param name="newOffset">New offset</param>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// Get the current target.
        /// </summary>
        /// <returns>Current target transform</returns>
        public Transform GetTarget()
        {
            return target;
        }

        /// <summary>
        /// Check if camera is currently shaking.
        /// </summary>
        /// <returns>True if shaking</returns>
        public bool IsShaking()
        {
            return currentShakeIntensity > 0;
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, 
                                           (minBounds.y + maxBounds.y) * 0.5f, 0);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, 
                                         maxBounds.y - minBounds.y, 1);
                Gizmos.DrawWireCube(center, size);
            }

            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
                
                // Draw look-ahead indicator
                if (enableLookAhead && Application.isPlaying)
                {
                    Gizmos.color = Color.blue;
                    Vector3 lookAheadPos = target.position + lookAheadOffset;
                    Gizmos.DrawWireSphere(lookAheadPos, 0.5f);
                }
            }
        }

        #endregion
    }
}