using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform pointA; // First point for patrol
    public Transform pointB; // Second point for patrol
    private Transform player; // Player's Transform
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float aggroRange = 5f; // Aggro range (narrow field of view)
    public float chaseBoundaryRange = 10f; // Chase boundary
    public float idleTime = 3f; // Wait time in idle state
    private Vector3 targetPoint;
    private bool isChasing = false;
    private bool isIdle = false;
    private float idleTimer = 0f;

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
        Debug.Log("Patrol State");

        // Start chasing if the player enters the aggro range
        if (distanceToPlayer <= aggroRange)
        {
            isChasing = true;
            Debug.Log("Transition to Chase State");
            return;
        }

        // Move between points only on the x-axis
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.MoveTowards(transform.position.x, targetPoint.x, patrolSpeed * Time.deltaTime);
        transform.position = newPosition;

        // Switch to the other point upon reaching the target
        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            targetPoint = targetPoint.x == pointA.position.x
                ? new Vector3(pointB.position.x, transform.position.y, transform.position.z)
                : new Vector3(pointA.position.x, transform.position.y, transform.position.z);
        }
    }

    void ChasePlayer(float distanceToPlayer)
    {
        Debug.Log("Chase State");

        // Return to patrol if the player exits the chase boundary or aggro range
        if (distanceToPlayer > chaseBoundaryRange)
        {
            isChasing = false;
            Debug.Log("Transition to Patrol State");
            targetPoint = ClosestPatrolPoint(); // Return to the closest patrol point
            return;
        }

        // Move quickly toward the player only on the x-axis
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
        transform.position = newPosition;
    }

    void HandleIdleState(float distanceToPlayer)
    {
        Debug.Log("Idle State");

        // Resume chasing if the player returns to the patrol area
        if (distanceToPlayer <= chaseBoundaryRange && IsPlayerInPatrolArea())
        {
            isIdle = false;
            isChasing = true;
            Debug.Log("Transition to Chase State");
            return;
        }

        // Return to patrol after a certain time
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            isIdle = false;
            Debug.Log("Transition to Patrol State");
            targetPoint = ClosestPatrolPoint();
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

    bool IsPlayerInPatrolArea()
    {
        // Check if the player is within the patrol area
        float distanceToA = Mathf.Abs(player.position.x - pointA.position.x);
        float distanceToB = Mathf.Abs(player.position.x - pointB.position.x);
        return distanceToA <= chaseBoundaryRange || distanceToB <= chaseBoundaryRange;
    }
}