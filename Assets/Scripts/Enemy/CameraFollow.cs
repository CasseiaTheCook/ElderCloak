using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [HideInInspector] public Transform target;          // The object to follow (e.g., player)
    public float smoothSpeed = 0.125f; // Smoothing factor (lower is smoother/slower)
    public Vector3 offset;            // Offset from target (set in Inspector or code)

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("PlayerSprite").transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        // Force the camera's Z position to stay fixed (2D follow)
        desiredPosition.z = transform.position.z;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}