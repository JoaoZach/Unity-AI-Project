# Trabalho Prático
Trabalho de Inteligência Artificial Aplicada a Jogos

# Tilebound

# Membros do Grupo:
- João Faria - 25590
- Samuel Fernandes - 31470

# Metodos de Aplicação de IA escolhidos
- Path Finding: Usado para que o Boss encontre o melhor caminho para ir ter ao jogador
- State Machine: Chamado através de ações feitas pelo Player, como por exemplo saltar, que faz com que o Player ative a animação de saltar, etc.
- Behavior Tree: Atribuido para o Boss ter mecânicas diferentes, quando o Player não está perto ele patrulha, se o Player está perto começa a focar nele, e se o Player está a atacar o Boss tenta fugir.

# Game Engine e Linguagem Escolhidas
- Game Engine: Unity
- Linguagem: C#

# Pastas Importantes
- Animations: Contem todas as animações do jogo
- Scene: Contem o cenário do jogo, sem ela o jogo não existe
- Scripts: Contem os scripts do jogo, ou seja o código do jogo

# Scripts
- Enemy
- EnemyBT
- EnemyHitbox
- PlayerAttack
- PlayerJump
- PlayerMovement
- PlayerMovementState

# Enemy
A classe Enemy atualiza a cada frame a orientação do inimigo lendo aiPath.desiredVelocity.x e ajustando transform.localScale para valores fixos (virar para a esquerda ou para a direita) conforme a direção do movimento; depende de um componente AIPath atribuído e não faz mais nada (pode dar NullReferenceException se aiPath estiver ausente), sendo comum substituir a escala fixa por SpriteRenderer.flipX ou adicionar verificação if (aiPath == null) return; como melhoria.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    public AIPath aiPath;

    void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f){
            transform.localScale = new Vector3(-6f, 6f, 6f);
        } else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(6f, 6f, 6f);
        }
    }
}
```

</details>

# EnemyBT
A classe EnemyBT trata-se da Behaviour Tree do inimigo, ou seja, o comportamento que este toma, através de certas ações do Player, ou ações já programadas nele, como o Patrol

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

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
        var fleeSequence = new Sequence(new List<BTNode>
        {
            new ShouldFlee(this),
            new Timer(new FleeAction(this), fleeDuration)
        });

        var attackSequence = new Sequence(new List<BTNode>
        {
            new IsPlayerInAttackRange(this),
            new Inverter(new HasAttackCooldown(this)),
            new ChaseAction(this) // Este nó inclui o ataque
        });

        var chaseSequence = new Sequence(new List<BTNode>
        {
            new CanSeePlayer(this),
            new ChaseAction(this)
        });

        var patrolSequence = new Sequence(new List<BTNode>
        {
            new PatrolAction(this)
        });

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
```

</details>

# EnemyHitbox
É a classe encarregada de calcular e atribuir a Hitbox do inimigo, para que o Player possa o atacar

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.8f);

    void Awake()
    {
        // Make all colliders on this object and its children triggers so they don't apply physics forces
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var c in cols)
        {
            if (c != null)
                c.isTrigger = true;
        }

        // Ensure there is at least one Rigidbody2D and make all present kinematic
        var rbs = GetComponentsInChildren<Rigidbody2D>();
        if (rbs.Length == 0)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            foreach (var rb in rbs)
            {
                if (rb != null)
                    rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            GetComponent<Enemy>()?.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            if (col is CircleCollider2D circle)
            {
                Vector3 center = transform.TransformPoint(circle.offset);
                float radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                Gizmos.DrawWireSphere(center, radius);
                return;
            }

            if (col is BoxCollider2D box)
            {
                Vector3 center = transform.TransformPoint(box.offset);
                Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
                Gizmos.DrawWireCube(center, size);
                return;
            }

            if (col is PolygonCollider2D poly)
            {
                for (int p = 0; p < poly.pathCount; p++)
                {
                    var points = poly.GetPath(p);
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 a = transform.TransformPoint(points[i]);
                        Vector3 b = transform.TransformPoint(points[(i + 1) % points.Length]);
                        Gizmos.DrawLine(a, b);
                    }
                }
                return;
            }
        }

        // fallback: draw sprite bounds if no collider
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Bounds b = sr.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
```

</details>

# PlayerAttack
Detecta quando o jogador pressiona E, dispara a animação de ataque e muda o estado de movimento para ataque; aplica dano aos inimigos dentro do alcance do ponto de ataque e notifica inimigos num raio próximo para reagirem; também desenha no editor uma indicação do alcance do ataque.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int damage = 1;
    public LayerMask enemyLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var state = GetComponent<PlayerMovementState>();
            if (state != null)
                state.SetMoveState(PlayerMovementState.MoveState.Attack);
        }
    }

    public void PerformAttack()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            hit.SendMessage(
                "TakeDamage",
                damage,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

```

</details>

# PlayerJump
Classe que trata toda a lógica de saltos do jogador:escuta a tecla de salto, determina se o jogador está no chão (raycast) ou encostado a uma parede e executa salto normal, duplo-salto ou wall‑jump conforme o caso; aplica forças ao Rigidbody2D, zera velocidades antes do salto quando necessário, controla um cooldown de movimento para wall‑jump e atualiza o PlayerMovementState para refletir o estado (Jump, Double_Jump, Wall_Jump). Também calcula dimensões do jogador (para raycasts) e garante que o duplo salto só ocorre uma vez até aterrissar.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private PlayerMovementState playerMovementState;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float jumpForce = 6;
    [SerializeField] private float doubleJumpForce = 6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(4f, 8f);
    [SerializeField] private float wallJumpMovementCooldown = 0.2f;
    private PlayerMovement playerMovement;
    
    private float playerHalfHeight;
    private float playerHalfWidth;

    private bool canDoubleJump;

    private void Start()
    {
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        playerHalfHeight = spriteRenderer.bounds.extents.y;
        playerMovement = GetComponent<PlayerMovement>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckJumpType();
        }
    }

    private void CheckJumpType()
    {
        bool isGrounded = GetIsGrounded();



        if (isGrounded)
        {
            playerMovementState.SetMoveState(PlayerMovementState.MoveState.Jump);
            Jump(jumpForce);
        }
        else
        {

            int direction = GetWallJumpDirection();
            if (direction == 0 && canDoubleJump && rigidBody.linearVelocity.y <= 0.1f)
            {
                DoubleJump();
            }
            else if (direction != 0)
            {
                WallJump(direction);
            }

           
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GetIsGrounded();
    }

    private int GetWallJumpDirection()
    {
        if(Physics2D.Raycast(transform.position, Vector2.right, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground")))
        {
            return -1;
        }
        if (Physics2D.Raycast(transform.position, Vector2.left, playerHalfWidth + 0.1f, LayerMask.GetMask("Ground")))
        {
            return 1;
        }

        return 0;
    }

    private bool GetIsGrounded() 
    {
        bool hit = Physics2D.Raycast(transform.position, Vector2.down, playerHalfHeight + 0.1f, LayerMask.GetMask("Ground"));
        if (hit)
        {
            canDoubleJump = true;
        }

        return hit;
    }

    private void DoubleJump()
    {
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
        Jump(doubleJumpForce);
        canDoubleJump = false;
        playerMovementState.SetMoveState(PlayerMovementState.MoveState.Double_Jump);
    }

    private void WallJump(int directions)
    {
        Vector2 force = wallJumpForce;
        force.x *= directions;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
        playerMovement.wallJumpCooldown = wallJumpMovementCooldown;
        rigidBody.AddForce(force, ForceMode2D.Impulse);
        playerMovementState.SetMoveState(PlayerMovementState.MoveState.Wall_Jump);
    }

    private void Jump(float force)
    {
        rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
}

```

</details>

# PlayerMovement
Classe que trata o movimento horizontal do jogador: lê o eixo "Horizontal", aplica deslocamento multiplicado por speed via transform.Translate, atualiza a orientação do sprite (flip) comparando a posição atual com a do frame anterior e decrementa o wallJumpCooldown. Também calcula limites de ecrã e metade da largura do jogador no Start() e expõe animator/spriteRenderer para ligações no Inspector; nota que usa movimento por transformação direta em vez de física (Rigidbody2D).

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;

public class PlayerMovement : MonoBehaviour 
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    public float wallJumpCooldown { get; set;}
    private Vector2 movement;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float xPosLastFrame;
    private void Start()
    {
        
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = spriteRenderer.bounds.extents.x;
    }

    private void Update()
    {
        HandleMovement();
        FlipCharacterX();

        if (wallJumpCooldown > 0 )
        {
            wallJumpCooldown -= Time.deltaTime;
        }
    }

    private void FlipCharacterX()
    {
        float input = Input.GetAxis("Horizontal");
        if (input > 0 && (transform.position.x > xPosLastFrame))
        {
            spriteRenderer.flipX = false;
        }
        else if (input < 0 && (transform.position.x < xPosLastFrame))
        {
            spriteRenderer.flipX = true;
        }

        xPosLastFrame = transform.position.x;
    }

    private void HandleMovement()
    {
        if (wallJumpCooldown > 0f) return;

        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);
    }

}
```

</details>

# PlayerMovementState
Classe que centraliza o estado de movimento do jogador: define o enum MoveState (Idle, Run, Attack, Jump, Fall, Double_Jump, Wall_Jump), determina o estado atual a cada frame com base na posição e na Rigidbody2D (velocidade vertical) e expõe SetMoveState para forçar transições. Para cada estado chama um handler que dispara a animação correspondente (usa o Animator) e notifica ouvintes via OnPlayerMoveStateChanged. Também tenta obter Rigidbody2D e Animator em Awake() se não estiverem atribuídos. Em resumo: liga a física/entrada ao sistema de animações e fornece um ponto único para mudar/consultar o estado de movimento.

<details>
    <summary>Clique aqui para ver a função completa</summary>
  
  ```csharp 
using UnityEngine;
using System;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Run,
        Attack,
        Jump,
        Fall,
        Double_Jump,
        Wall_Jump,
    }
    public MoveState CurrentMoveState {  get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";
    private const string doubleJumpAnim = "Double Jump";
    private const string wallJumpingAnim = "Wall Jump";
    private const string attackAnim = "Attack";
    public static Action<MoveState> OnPlayerMoveStateChanged;
    private float xPosLastFrame;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (transform.position.x == xPosLastFrame && rb.linearVelocity.y == 0)
        {
            SetMoveState(MoveState.Idle);
        }
        else if (transform.position.x != xPosLastFrame && rb.linearVelocity.y == 0)
        {
            SetMoveState(MoveState.Run);
        }
        else if (rb.linearVelocity.y < 0)
        {
            SetMoveState(MoveState.Fall);
        }

            xPosLastFrame = transform.position.x;
    }

    public void SetMoveState(MoveState moveState)
    {
        if (moveState == CurrentMoveState) return;

        switch (moveState)
        {
            case MoveState.Idle:
                HandleIdle();
                break;

            case MoveState.Run:
                HandleRun();
                break;

            case MoveState.Jump:
                HandleJump();
                break;

            case MoveState.Fall:
                HandleFall();
                break;

            case MoveState.Double_Jump:
                HandleDoubleJump();
                break;

            case MoveState.Wall_Jump:
                HandleWallJump();
                break;

            case MoveState.Attack:
                HandleAttack();
                break;

            default:
                Debug.LogError($"Invalid movement state: {moveState}");
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }

    private void HandleIdle()
    {
        animator.Play(idleAnim);
    }

    private void HandleRun()
    {
        animator.Play(runAnim);

    }

    private void HandleJump()
    {
        animator.Play(jumpAnim);

    }

    private void HandleAttack()
    {
        animator.Play(attackAnim);
    }

    private void HandleFall()
    {
        animator.Play(fallAnim);

    }

    private void HandleDoubleJump()
    {
        animator.Play(doubleJumpAnim);

    }

    private void HandleWallJump()
    {
        animator.Play(wallJumpingAnim);

    }

}
```

</details>

# Créditos
- Sprite do player: https://aamatniekss.itch.io/fantasy-knight-free-pixelart-animated-character
- Sprite do Boss: https://darkpixel-kronovi.itch.io/mecha-golem-free
- Sprite das plataformas: https://brackeysgames.itch.io/brackeys-platformer-bundle
- Script de pathfinding: https://arongranberg.com/astar/front
