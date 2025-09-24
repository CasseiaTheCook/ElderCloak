using UnityEngine;

public class EnemyGroundDetection : MonoBehaviour, IGroundDetection
{
    [Header("Ground Detection Settings")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayerMask = 1; // Default to Layer 0 (Default layer)
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsGrounded()
    {
        // Use both velocity check and raycast for more accurate detection
        bool velocityGrounded = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        
        // Perform a short raycast downward to check for ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask);
        bool raycastGrounded = hit.collider != null;
        
        return velocityGrounded || raycastGrounded;
    }

    // Optional: Draw gizmo in scene view to visualize ground detection
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}