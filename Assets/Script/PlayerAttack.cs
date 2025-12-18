using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int damage = 1;
    public LayerMask enemyLayer;
    [Tooltip("Radius to notify nearby enemies that the player performed an attack (they won't flee)")]
    public float notifyRadius = 1.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator?.SetTrigger("Attack");
   
            var movementState = GetComponent<PlayerMovementState>();
            if (movementState != null)
                movementState.SetMoveState(PlayerMovementState.MoveState.Attack);

            PerformAttack(); 
        }
    }

    public void PerformAttack()
    {
        if (attackPoint == null) return;
        var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var c in hits)
        {
            c.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        var nearby = Physics2D.OverlapCircleAll(attackPoint.position, notifyRadius, enemyLayer);
        foreach (var c in nearby)
        {
            var enemy = c.GetComponentInParent<EnemyBT>();
            if (enemy != null)
            {
                enemy.OnPlayerAttackNearby();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
