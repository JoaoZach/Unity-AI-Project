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
    public float attackDistance = 1.5f;
    [Tooltip("Distance at which the enemy will stop moving towards the player")]
    public float stopDistance = 2f;
    public Animator animator;
    public float attackCooldown = 1f;

    private AIPath aiPath;
    private AIDestinationSetter destination;

    private int patrolIndex = 0;
    private float lostPlayerTimer = 0f;
    private float attackTimer = 0f;
    public float fleeDuration = 2f;
    public float fleeDistance = 3f;
    private float fleeTimer = 0f;
    private GameObject fleeTargetObj;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        if (aiPath == null) aiPath = GetComponentInChildren<AIPath>();

        destination = GetComponent<AIDestinationSetter>();
        if (destination == null) destination = GetComponentInChildren<AIDestinationSetter>();
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            destination.target = patrolPoints[patrolIndex];
    }

    void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (fleeTimer > 0f) fleeTimer -= Time.deltaTime;

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

        if (fleeTimer > 0f)
        {
            if (fleeTargetObj != null)
            {
                if (destination != null) destination.target = fleeTargetObj.transform;
                if (aiPath != null)
                {
                    aiPath.enabled = true;
                    aiPath.isStopped = false;
                }
            }
            return;
        }

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

                if (dist <= stopDistance)
                {
                    if (aiPath != null)
                    {
                        aiPath.isStopped = true;
                        aiPath.enabled = false;
                    }
                
                    if (dist <= attackDistance && attackTimer <= 0f)
                    {
                        attackTimer = attackCooldown;
                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                    }
                }
                else
                {
                    if (aiPath != null)
                    {
                        aiPath.enabled = true;
                        aiPath.isStopped = false;
                    }
                }
                break;
        }
    }


    public void OnPlayerAttackNearby()
    {
        if (player == null) return;

        Vector2 dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + dir * fleeDistance;

        if (fleeTargetObj == null)
        {
            fleeTargetObj = new GameObject("FleeTarget");
            fleeTargetObj.transform.parent = transform;
        }
        fleeTargetObj.transform.position = targetPos;

        fleeTimer = fleeDuration;

        if (destination != null) destination.target = fleeTargetObj.transform;
        if (aiPath != null)
        {
            aiPath.enabled = true;
            aiPath.isStopped = false;
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
