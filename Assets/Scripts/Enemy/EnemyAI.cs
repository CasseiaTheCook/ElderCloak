using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform pointA; // First point for patrol
    public Transform pointB; // Second point for patrol
    private Transform player; // Player's Transform
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float aggroRange = 5f; // Level 1: Start chasing
    public float chaseRange = 10f; // Level 2: Continue chasing
    public float idleTime = 3f; // Wait time in idle state
    public float chaseTimeout = 5f; // Maximum chase time

    private Vector3 targetPoint;
    private bool isChasing = false;
    private bool isIdle = false;
    private float idleTimer = 0f;
    private float chaseTimer = 0f;

    void Start()
    {
        targetPoint = new Vector3(pointA.position.x, transform.position.y, transform.position.z); // Initial patrol target
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isChasing)
        {
            ChasePlayer(distanceToPlayer);
        }
        else if (isIdle)
        {
            HandleIdleState(distanceToPlayer);
        }
        else
        {
            Patrol(distanceToPlayer);
        }
    }

    void Patrol(float distanceToPlayer)
    {
        // Start chasing only if the player enters aggro range
        if (distanceToPlayer <= aggroRange)
        {
            isChasing = true;
            chaseTimer = 0f; // Reset chase timer
            return;
        }

        // Move between patrol points on the x-axis
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.MoveTowards(transform.position.x, targetPoint.x, patrolSpeed * Time.deltaTime);
        transform.position = newPosition;

        // If reached patrol border and player is out of chase range, go idle
        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            bool atBorder = targetPoint.x == pointA.position.x || targetPoint.x == pointB.position.x;
            if (atBorder && distanceToPlayer > chaseRange)
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

    void ChasePlayer(float distanceToPlayer)
    {
        // Stop chasing if the player leaves the chase range
        if (distanceToPlayer > chaseRange)
        {
            isChasing = false;
            targetPoint = ClosestPatrolPoint();
            chaseTimer = 0f; // Reset chase timer
            return;
        }

        // If the player is in the aggro range, reset the chase timer
        if (distanceToPlayer <= aggroRange)
        {
            chaseTimer = 0f; // Reset timer since the player is close
        }
        else
        {
            // Increment the chase timer if the player is only in the chase range
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeout)
            {
                // Stop chasing after the timeout if the player doesn't enter the aggro range
                isChasing = false;
                isIdle = true;
                idleTimer = 0f;
                chaseTimer = 0f; // Reset chase timer
                return;
            }
        }

        // Move toward the player's position only on the x-axis
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
        transform.position = newPosition;
    }

    void HandleIdleState(float distanceToPlayer)
    {
        // Resume chasing if the player enters the aggro range
        if (distanceToPlayer <= aggroRange)
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

        // Draw aggro range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Draw chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}