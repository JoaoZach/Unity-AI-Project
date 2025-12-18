using UnityEngine;
using Pathfinding;

public enum EnemyState { Patrol, Chase }

public class EnemyBT : MonoBehaviour
{
    [Header("State")]
    public EnemyState currentState;

    [Header("Vision")]
    public float viewDistance = 6f;
    public float chaseDistance = 10f;

    [Header("Memory")]
    public float losePlayerDelay = 2f;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    private AIPath aiPath;
    private AIDestinationSetter destination;

    private int patrolIndex = 0;
    private float lostPlayerTimer = 0f;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        destination = GetComponent<AIDestinationSetter>();
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            destination.target = patrolPoints[patrolIndex];
    }

    void Update()
    {
        Think();

        if (currentState == EnemyState.Patrol)
            PatrolUpdate();
    }

    void Think()
    {
        if (player == null)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (dist <= viewDistance) SetState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (dist > chaseDistance)
                {
                    lostPlayerTimer += Time.deltaTime;
                    if (lostPlayerTimer >= losePlayerDelay)
                    {
                        lostPlayerTimer = 0f;
                        SetState(EnemyState.Patrol);
                    }
                }
                else
                {
                    lostPlayerTimer = 0f;
                }
                break;
        }
    }

    void SetState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Patrol:
                aiPath.isStopped = false;
                destination.target = patrolPoints[patrolIndex];
                break;

            case EnemyState.Chase:
                aiPath.isStopped = false;
                destination.target = player;
                break;
        }
    }

    void PatrolUpdate()
    {
        if (patrolPoints.Length == 0) return;

        if (aiPath.reachedDestination)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            destination.target = patrolPoints[patrolIndex];
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}
