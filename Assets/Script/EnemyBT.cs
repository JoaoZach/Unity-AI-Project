using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

// Nós da Behavior Tree
public abstract class BTNode
{
    public enum Status { Running, Success, Failure }
    public abstract Status Execute();
}

// Decorators
public class Inverter : BTNode
{
    private BTNode child;

    public Inverter(BTNode child)
    {
        this.child = child;
    }

    public override Status Execute()
    {
        Status result = child.Execute();
        return result == Status.Success ? Status.Failure :
               result == Status.Failure ? Status.Success : Status.Running;
    }
}

public class Timer : BTNode
{
    private BTNode child;
    private float duration;
    private float timer;
    private bool started;

    public Timer(BTNode child, float duration)
    {
        this.child = child;
        this.duration = duration;
    }

    public override Status Execute()
    {
        if (!started)
        {
            timer = Time.time;
            started = true;
        }

        if (Time.time - timer < duration)
        {
            Status result = child.Execute();
            if (result == Status.Running)
                return Status.Running;
            return Status.Running; // Continua rodando até o timer acabar
        }

        started = false;
        return Status.Success;
    }
}

// Composite Nodes
public class Sequence : BTNode
{
    protected List<BTNode> children = new List<BTNode>();
    private int currentChild = 0;

    public Sequence(List<BTNode> children)
    {
        this.children = children;
    }

    public override Status Execute()
    {
        Status result = children[currentChild].Execute();

        if (result == Status.Running)
            return Status.Running;

        if (result == Status.Failure)
        {
            currentChild = 0;
            return Status.Failure;
        }

        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.Success;
        }

        return Status.Running;
    }
}

public class Selector : BTNode
{
    protected List<BTNode> children = new List<BTNode>();
    private int currentChild = 0;

    public Selector(List<BTNode> children)
    {
        this.children = children;
    }

    public override Status Execute()
    {
        Status result = children[currentChild].Execute();

        if (result == Status.Running)
            return Status.Running;

        if (result == Status.Success)
        {
            currentChild = 0;
            return Status.Success;
        }

        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.Failure;
        }

        return Status.Running;
    }
}

// Leaf Nodes (Ações e Condições)
public class PatrolAction : BTNode
{
    private EnemyBT enemy;
    private int patrolIndex = 0;

    public PatrolAction(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        if (enemy.patrolPoints.Length == 0)
            return Status.Failure;

        if (enemy.aiPath.reachedDestination)
        {
            patrolIndex = (patrolIndex + 1) % enemy.patrolPoints.Length;
            enemy.destination.target = enemy.patrolPoints[patrolIndex];
        }

        enemy.aiPath.isStopped = false;
        enemy.destination.target = enemy.patrolPoints[patrolIndex];
        enemy.animator?.SetBool("Moving", true);

        return Status.Running;
    }
}

public class ChaseAction : BTNode
{
    private EnemyBT enemy;

    public ChaseAction(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);

        if (dist <= enemy.stopDistance)
        {
            enemy.aiPath.isStopped = true;
            enemy.aiPath.enabled = false;

            if (dist <= enemy.attackDistance && enemy.attackTimer <= 0f)
            {
                enemy.attackTimer = enemy.attackCooldown;
                enemy.animator?.SetTrigger("Attack");
                return Status.Success;
            }

            return Status.Running;
        }
        else
        {
            enemy.aiPath.enabled = true;
            enemy.aiPath.isStopped = false;
            enemy.destination.target = enemy.player;
            enemy.animator?.SetBool("Moving", true);
            return Status.Running;
        }
    }
}

public class FleeAction : BTNode
{
    private EnemyBT enemy;
    private GameObject fleeTargetObj;

    public FleeAction(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        if (enemy.player == null) return Status.Failure;

        Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)enemy.player.position).normalized;
        Vector2 targetPos = (Vector2)enemy.transform.position + dir * enemy.fleeDistance;

        if (fleeTargetObj == null)
        {
            fleeTargetObj = new GameObject("FleeTarget");
            fleeTargetObj.transform.parent = enemy.transform;
        }
        fleeTargetObj.transform.position = targetPos;

        enemy.destination.target = fleeTargetObj.transform;
        enemy.aiPath.enabled = true;
        enemy.aiPath.isStopped = false;
        enemy.animator?.SetBool("Moving", true);

        return Status.Running;
    }
}

// Condições
public class CanSeePlayer : BTNode
{
    private EnemyBT enemy;

    public CanSeePlayer(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        if (enemy.player == null) return Status.Failure;

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);
        return dist <= enemy.viewDistance ? Status.Success : Status.Failure;
    }
}

public class IsPlayerInChaseRange : BTNode
{
    private EnemyBT enemy;

    public IsPlayerInChaseRange(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        if (enemy.player == null) return Status.Failure;

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);
        return dist <= enemy.chaseDistance ? Status.Success : Status.Failure;
    }
}

public class IsPlayerInAttackRange : BTNode
{
    private EnemyBT enemy;

    public IsPlayerInAttackRange(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        if (enemy.player == null) return Status.Failure;

        float dist = Vector2.Distance(enemy.transform.position, enemy.player.position);
        return dist <= enemy.attackDistance ? Status.Success : Status.Failure;
    }
}

public class ShouldFlee : BTNode
{
    private EnemyBT enemy;
    private float fleeTimer;

    public ShouldFlee(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        // Esta condição seria ativada por evento externo
        // Por simplicidade, vamos manter o temporizador
        if (enemy.fleeTimer > 0f)
        {
            enemy.fleeTimer -= Time.deltaTime;
            return Status.Success;
        }
        return Status.Failure;
    }
}

public class HasAttackCooldown : BTNode
{
    private EnemyBT enemy;

    public HasAttackCooldown(EnemyBT enemy)
    {
        this.enemy = enemy;
    }

    public override Status Execute()
    {
        return enemy.attackTimer > 0f ? Status.Success : Status.Failure;
    }
}

// Classe principal do inimigo com Behavior Tree
public class EnemyBT : MonoBehaviour
{
    [Header("Vision")]
    public float viewDistance = 6f;
    public float chaseDistance = 10f;

    [Header("Memory")]
    public float losePlayerDelay = 2f;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    public float attackDistance = 1.5f;
    public float stopDistance = 2f;
    public Animator animator;
    public float attackCooldown = 1f;

    [Header("Flee")]
    public float fleeDuration = 2f;
    public float fleeDistance = 3f;

    [HideInInspector]
    public AIPath aiPath;
    [HideInInspector]
    public AIDestinationSetter destination;

    [HideInInspector]
    public float attackTimer = 0f;
    [HideInInspector]
    public float fleeTimer = 0f;

    private BTNode behaviorTree;
    private float lostPlayerTimer = 0f;

    void Awake()
    {
        aiPath = GetComponent<AIPath>();
        if (aiPath == null) aiPath = GetComponentInChildren<AIPath>();

        destination = GetComponent<AIDestinationSetter>();
        if (destination == null) destination = GetComponentInChildren<AIDestinationSetter>();

        BuildBehaviorTree();
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
            destination.target = patrolPoints[0];
    }

    void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        // Executar a Behavior Tree
        if (behaviorTree != null)
        {
            behaviorTree.Execute();
        }
    }

    void BuildBehaviorTree()
    {
        // Construir a árvore de comportamento
        // Estrutura: Selector (tenta comportamentos por ordem de prioridade)

        // 1. Flee (maior prioridade)
        var fleeSequence = new Sequence(new List<BTNode>
        {
            new ShouldFlee(this),
            new Timer(new FleeAction(this), fleeDuration)
        });

        // 2. Attack (segunda prioridade)
        var attackSequence = new Sequence(new List<BTNode>
        {
            new IsPlayerInAttackRange(this),
            new Inverter(new HasAttackCooldown(this)),
            new ChaseAction(this) // Este nó inclui o ataque
        });

        // 3. Chase (terceira prioridade)
        var chaseSequence = new Sequence(new List<BTNode>
        {
            new CanSeePlayer(this),
            new ChaseAction(this)
        });

        // 4. Patrol (comportamento padrão)
        var patrolSequence = new Sequence(new List<BTNode>
        {
            new PatrolAction(this)
        });

        // Árvore completa
        behaviorTree = new Selector(new List<BTNode>
        {
            fleeSequence,
            attackSequence,
            chaseSequence,
            patrolSequence
        });
    }

    public void OnPlayerAttackNearby()
    {
        // Ativar flee por um tempo
        fleeTimer = fleeDuration;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
