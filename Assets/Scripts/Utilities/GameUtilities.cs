using UnityEngine;

namespace ElderCloak.Utilities
{
    /// <summary>
    /// Collection of utility functions for the ElderCloak character controller system.
    /// Provides common helper methods for physics, math, and game object operations.
    /// </summary>
    public static class GameUtilities
    {
        #region Physics Utilities

        /// <summary>
        /// Check if a point is within a cone-shaped area.
        /// Useful for cone-based attack detection or AI vision cones.
        /// </summary>
        /// <param name="origin">The origin point of the cone</param>
        /// <param name="direction">The direction the cone is pointing</param>
        /// <param name="point">The point to test</param>
        /// <param name="range">The maximum range of the cone</param>
        /// <param name="angle">The total angle of the cone in degrees</param>
        /// <returns>True if the point is within the cone</returns>
        public static bool IsPointInCone(Vector2 origin, Vector2 direction, Vector2 point, float range, float angle)
        {
            Vector2 toPoint = point - origin;
            float distance = toPoint.magnitude;
            
            // Check if within range
            if (distance > range) return false;
            
            // Check if within angle
            float angleToPoint = Vector2.Angle(direction, toPoint);
            return angleToPoint <= angle * 0.5f;
        }

        /// <summary>
        /// Calculate knockback force based on impact direction and strength.
        /// </summary>
        /// <param name="impactPoint">Point of impact</param>
        /// <param name="targetPoint">Target position</param>
        /// <param name="force">Base knockback force</param>
        /// <param name="upwardBias">Additional upward force (0-1)</param>
        /// <returns>Knockback force vector</returns>
        public static Vector2 CalculateKnockback(Vector2 impactPoint, Vector2 targetPoint, float force, float upwardBias = 0.3f)
        {
            Vector2 direction = (targetPoint - impactPoint).normalized;
            direction.y += upwardBias;
            return direction.normalized * force;
        }

        /// <summary>
        /// Check if a position is within camera view.
        /// </summary>
        /// <param name="position">World position to check</param>
        /// <param name="camera">Camera to check against (null for main camera)</param>
        /// <param name="margin">Extra margin around screen bounds</param>
        /// <returns>True if position is visible</returns>
        public static bool IsPositionInCameraView(Vector3 position, Camera camera = null, float margin = 0f)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return false;

            Vector3 viewportPoint = camera.WorldToViewportPoint(position);
            return viewportPoint.x >= -margin && viewportPoint.x <= 1f + margin &&
                   viewportPoint.y >= -margin && viewportPoint.y <= 1f + margin &&
                   viewportPoint.z > 0;
        }

        #endregion

        #region Math Utilities

        /// <summary>
        /// Remap a value from one range to another.
        /// </summary>
        /// <param name="value">Value to remap</param>
        /// <param name="fromMin">Source range minimum</param>
        /// <param name="fromMax">Source range maximum</param>
        /// <param name="toMin">Target range minimum</param>
        /// <param name="toMax">Target range maximum</param>
        /// <returns>Remapped value</returns>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Apply smoothing to a value over time using exponential decay.
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="smoothing">Smoothing factor (higher = slower)</param>
        /// <param name="deltaTime">Time delta</param>
        /// <returns>Smoothed value</returns>
        public static float SmoothDamp(float current, float target, float smoothing, float deltaTime)
        {
            return Mathf.Lerp(current, target, 1f - Mathf.Exp(-smoothing * deltaTime));
        }

        /// <summary>
        /// Calculate a parabolic trajectory point.
        /// Useful for projectile arcs or jump prediction.
        /// </summary>
        /// <param name="startPoint">Starting position</param>
        /// <param name="endPoint">Target position</param>
        /// <param name="height">Arc height</param>
        /// <param name="t">Time parameter (0-1)</param>
        /// <returns>Position along the arc</returns>
        public static Vector2 CalculateParabolicPoint(Vector2 startPoint, Vector2 endPoint, float height, float t)
        {
            Vector2 linearPoint = Vector2.Lerp(startPoint, endPoint, t);
            float parabolicOffset = 4 * height * t * (1 - t);
            return new Vector2(linearPoint.x, linearPoint.y + parabolicOffset);
        }

        #endregion

        #region GameObject Utilities

        /// <summary>
        /// Find the closest object from a list of transforms.
        /// </summary>
        /// <param name="reference">Reference position</param>
        /// <param name="targets">List of target transforms</param>
        /// <returns>Closest transform or null if none found</returns>
        public static Transform FindClosest(Vector3 reference, Transform[] targets)
        {
            if (targets == null || targets.Length == 0) return null;

            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (Transform target in targets)
            {
                if (target == null) continue;

                float distance = Vector3.Distance(reference, target.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get all objects within a radius that implement a specific interface.
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="center">Center position</param>
        /// <param name="radius">Search radius</param>
        /// <param name="layerMask">Layer mask for filtering</param>
        /// <returns>Array of objects implementing the interface</returns>
        public static T[] FindObjectsInRadius<T>(Vector2 center, float radius, LayerMask layerMask = -1) where T : class
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius, layerMask);
            System.Collections.Generic.List<T> results = new System.Collections.Generic.List<T>();

            foreach (Collider2D col in colliders)
            {
                T component = col.GetComponent<T>();
                if (component != null && !results.Contains(component))
                {
                    results.Add(component);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Safely destroy a GameObject with optional delay.
        /// </summary>
        /// <param name="gameObject">GameObject to destroy</param>
        /// <param name="delay">Delay before destruction</param>
        public static void SafeDestroy(GameObject gameObject, float delay = 0f)
        {
            if (gameObject == null) return;

            if (Application.isPlaying)
            {
                if (delay > 0)
                    Object.Destroy(gameObject, delay);
                else
                    Object.Destroy(gameObject);
            }
            else
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        #endregion

        #region Animation Utilities

        /// <summary>
        /// Create an animation curve for bouncing motion.
        /// </summary>
        /// <param name="bounces">Number of bounces</param>
        /// <param name="decay">Bounce decay factor</param>
        /// <returns>Animation curve</returns>
        public static AnimationCurve CreateBounceCurve(int bounces = 3, float decay = 0.6f)
        {
            AnimationCurve curve = new AnimationCurve();
            
            // Start point
            curve.AddKey(0f, 0f);
            
            // Add bounce points
            for (int i = 1; i <= bounces; i++)
            {
                float time = (float)i / bounces;
                float height = Mathf.Pow(decay, i - 1);
                
                // Up point
                curve.AddKey(time - 0.05f, height);
                // Down point
                curve.AddKey(time, 0f);
            }
            
            // Smooth the curve
            for (int i = 0; i < curve.keys.Length; i++)
            {
                curve.SmoothTangents(i, 0.3f);
            }
            
            return curve;
        }

        /// <summary>
        /// Evaluate an easing function.
        /// </summary>
        /// <param name="t">Time parameter (0-1)</param>
        /// <param name="easingType">Type of easing to apply</param>
        /// <returns>Eased value</returns>
        public static float EaseInOut(float t, EasingType easingType = EasingType.Quadratic)
        {
            switch (easingType)
            {
                case EasingType.Linear:
                    return t;
                case EasingType.Quadratic:
                    return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                case EasingType.Cubic:
                    return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
                case EasingType.Sine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
                default:
                    return t;
            }
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Draw a debug arc in the scene view.
        /// </summary>
        /// <param name="center">Center of the arc</param>
        /// <param name="direction">Direction of the arc</param>
        /// <param name="angle">Total angle in degrees</param>
        /// <param name="radius">Radius of the arc</param>
        /// <param name="color">Color to draw with</param>
        /// <param name="segments">Number of segments</param>
        public static void DrawDebugArc(Vector3 center, Vector3 direction, float angle, float radius, 
                                       Color color, int segments = 20)
        {
#if UNITY_EDITOR
            float startAngle = -angle * 0.5f;
            float angleStep = angle / segments;
            
            Vector3 prevPoint = center + Quaternion.AngleAxis(startAngle, Vector3.forward) * direction * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                Vector3 currentPoint = center + Quaternion.AngleAxis(currentAngle, Vector3.forward) * direction * radius;
                
                Debug.DrawLine(prevPoint, currentPoint, color);
                prevPoint = currentPoint;
            }
            
            // Draw center lines
            Debug.DrawLine(center, center + Quaternion.AngleAxis(startAngle, Vector3.forward) * direction * radius, color);
            Debug.DrawLine(center, center + Quaternion.AngleAxis(startAngle + angle, Vector3.forward) * direction * radius, color);
#endif
        }

        #endregion

        #region Enums

        public enum EasingType
        {
            Linear,
            Quadratic,
            Cubic,
            Sine
        }

        #endregion
    }
}