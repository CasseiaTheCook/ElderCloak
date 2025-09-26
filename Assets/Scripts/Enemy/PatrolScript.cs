using UnityEngine;

public class PatrolScript : MonoBehaviour
{
    public Transform pointA; // First point for patrol
    public Transform pointB; // Second point for patrol
    private Transform player; // Player's Transform
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float aggroHorizontalRange = 5f; // Horizontal aggro range
    public float aggroVerticalRange = 2f;   // Vertical aggro range
    public float chaseHorizontalRange = 10f; // Horizontal chase range
    public float chaseVerticalRange = 4f;   // Vertical chase range
    public float idleTime = 3f; // Wait time in idle state
    public float chaseTimeout = 5f; // Maximum chase time

    private Vector3 targetPoint;
    private bool isChasing = false;
    private bool isIdle = false;
    private bool isPaused = false; // Flag to indicate if AI is paused for an attack
    private float idleTimer = 0f;
    private float chaseTimer = 0f;

    void Start()
    {
        targetPoint = new Vector3(pointA.position.x, transform.position.y, transform.position.z); // Initial patrol target
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // If the AI is paused, do nothing
        if (isPaused)
        {
            return;
        }

        float distanceToPlayerX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceToPlayerY = Mathf.Abs(transform.position.y - player.position.y);

        if (isChasing)
        {
            ChasePlayer(distanceToPlayerX, distanceToPlayerY);
        }
        else if (isIdle)
        {
            HandleIdleState(distanceToPlayerX, distanceToPlayerY);
        }
        else
        {
            Patrol(distanceToPlayerX, distanceToPlayerY);
        }
    }

    void Patrol(float distanceToPlayerX, float distanceToPlayerY)
    {
        // Start chasing only if the player enters the aggro box
        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            isChasing = true;
            chaseTimer = 0f; // Reset chase timer
            return;
        }

        // Move between patrol points on the x-axis
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.MoveTowards(transform.position.x, targetPoint.x, patrolSpeed * Time.deltaTime);
        transform.position = newPosition;

        // If reached patrol border, go idle
        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            bool atBorder = targetPoint.x == pointA.position.x || targetPoint.x == pointB.position.x;
            if (atBorder)
            {
                isIdle = true;
                idleTimer = 0f;
                return;
            }

            // Switch to the other point upon reaching the target
            targetPoint = targetPoint.x == pointA.position.x
                ? new Vector3(pointB.position.x, transform.position.y, transform.position.z)
                : new Vector3(pointA.position.x, transform.position.y, transform.position.z);
        }
    }

    void ChasePlayer(float distanceToPlayerX, float distanceToPlayerY)
    {
        // Stop chasing if the player leaves the chase box
        if (distanceToPlayerX > chaseHorizontalRange || distanceToPlayerY > chaseVerticalRange)
        {
            isChasing = false;
            targetPoint = ClosestPatrolPoint();
            chaseTimer = 0f; // Reset chase timer
            return;
        }

        // If the player is in the aggro box, reset the chase timer
        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            chaseTimer = 0f; // Reset timer since the player is close
        }
        else
        {
            // Increment the chase timer if the player is only in the chase box
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeout)
            {
                // Stop chasing after the timeout if the player doesn't enter the aggro box
                isChasing = false;
                chaseTimer = 0f; // Reset chase timer
                return;
            }
        }

        // Move toward the player's position only on the x-axis, clamped to patrol area
        float patrolMinX = Mathf.Min(pointA.position.x, pointB.position.x);
        float patrolMaxX = Mathf.Max(pointA.position.x, pointB.position.x);

        Vector3 newPosition = transform.position;
        float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
        newPosition.x = Mathf.Clamp(targetX, patrolMinX, patrolMaxX); // Clamp x to patrol area
        transform.position = newPosition;
    }

    void HandleIdleState(float distanceToPlayerX, float distanceToPlayerY)
    {
        // Resume chasing if the player enters the aggro box
        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            isIdle = false;
            isChasing = true;
            chaseTimer = 0f; // Reset chase timer
            return;
        }

        // Return to patrol after a certain time
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            isIdle = false;

            // After idling, set the next patrol target to the opposite border
            targetPoint = targetPoint.x == pointA.position.x
                ? new Vector3(pointB.position.x, transform.position.y, transform.position.z)
                : new Vector3(pointA.position.x, transform.position.y, transform.position.z);
        }
    }

    Vector3 ClosestPatrolPoint()
    {
        // Find the closest patrol point
        float distanceToA = Mathf.Abs(transform.position.x - pointA.position.x);
        float distanceToB = Mathf.Abs(transform.position.x - pointB.position.x);
        return distanceToA < distanceToB
            ? new Vector3(pointA.position.x, transform.position.y, transform.position.z)
            : new Vector3(pointB.position.x, transform.position.y, transform.position.z);
    }

    void OnDrawGizmosSelected()
    {
        // Draw aggro box
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(aggroHorizontalRange * 2, aggroVerticalRange * 2, 0f));

        // Draw chase box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(chaseHorizontalRange * 2, chaseVerticalRange * 2, 0f));

        if (pointA != null && pointB != null)
        {
            // Draw patrol area
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(pointA.position.x, transform.position.y, transform.position.z),
                new Vector3(pointB.position.x, transform.position.y, transform.position.z)
            );

            // Draw patrol points
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pointA.position, 0.15f);
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }
    }

    public void PauseAI() { isPaused = true; }
    public void ResumeAI() { isPaused = false; }
}