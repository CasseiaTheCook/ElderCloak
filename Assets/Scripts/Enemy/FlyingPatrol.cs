using Pathfinding; // Important: Needed for AIPath and AIDestinationSetter
using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    [HideInInspector] public Transform player;
    public float aggroHorizontalRange = 5f;
    public float aggroVerticalRange = 2f;
    public float chaseHorizontalRange = 10f;
    public float chaseVerticalRange = 4f;
    public float idleTime = 3f;
    public float chaseTimeout = 5f;

    private Transform targetPoint;
    private bool isChasing = false;
    private bool isIdle = false;
    private bool isPaused = false;
    private float idleTimer = 0f;
    private float chaseTimer = 0f;

    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;

    void Start()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        targetPoint = pointA;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        destinationSetter.target = targetPoint;
    }

    void Update()
    {
        if (isPaused) return;

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
        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            isChasing = true;
            chaseTimer = 0f;
            destinationSetter.target = player;
            Debug.Log("FlyingEnemyAI: Switching to CHASING state from PATROL (player entered aggro range).");
            return;
        }

        // Move between patrol points
        destinationSetter.target = targetPoint;

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            bool atBorder = targetPoint == pointA || targetPoint == pointB;
            if (atBorder)
            {
                isIdle = true;
                idleTimer = 0f;
                aiPath.canMove = false; // Stop moving during idle
                Debug.Log("FlyingEnemyAI: Switching to IDLE state (reached patrol border).");
                return;
            }
            // Switch patrol point
            targetPoint = targetPoint == pointA ? pointB : pointA;
            destinationSetter.target = targetPoint;
        }
    }

    void ChasePlayer(float distanceToPlayerX, float distanceToPlayerY)
    {
        if (distanceToPlayerX > chaseHorizontalRange || distanceToPlayerY > chaseVerticalRange)
        {
            isChasing = false;
            // Resume patrol to closest point
            targetPoint = ClosestPatrolPoint();
            destinationSetter.target = targetPoint;
            chaseTimer = 0f;
            Debug.Log("FlyingEnemyAI: Stopping CHASE (player left chase range). Switching to PATROL.");
            return;
        }

        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            chaseTimer = 0f;
        }
        else
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeout)
            {
                isChasing = false;
                chaseTimer = 0f;
                targetPoint = ClosestPatrolPoint();
                destinationSetter.target = targetPoint;
                Debug.Log("FlyingEnemyAI: Stopping CHASE (timeout reached). Switching to PATROL.");
                return;
            }
        }

        // Always set target to player while chasing
        destinationSetter.target = player;
    }

    void HandleIdleState(float distanceToPlayerX, float distanceToPlayerY)
    {
        if (distanceToPlayerX <= aggroHorizontalRange && distanceToPlayerY <= aggroVerticalRange)
        {
            isIdle = false;
            isChasing = true;
            chaseTimer = 0f;
            aiPath.canMove = true;
            destinationSetter.target = player;
            Debug.Log("FlyingEnemyAI: Switching to CHASING state from IDLE (player entered aggro range).");
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            isIdle = false;
            targetPoint = targetPoint == pointA ? pointB : pointA;
            destinationSetter.target = targetPoint;
            aiPath.canMove = true;
            Debug.Log("FlyingEnemyAI: Leaving IDLE (idle time over). Switching patrol target to " + targetPoint);
        }
    }

    Transform ClosestPatrolPoint()
    {
        float distA = Vector3.Distance(transform.position, pointA.position);
        float distB = Vector3.Distance(transform.position, pointB.position);
        return distA < distB ? pointA : pointB;
    }

    public void PauseAI()
    {
        isPaused = true;
        aiPath.canMove = false;
        Debug.Log("FlyingEnemyAI: AI Paused.");
    }
    public void ResumeAI()
    {
        isPaused = false;
        aiPath.canMove = true;
        Debug.Log("FlyingEnemyAI: AI Resumed.");
    }
}