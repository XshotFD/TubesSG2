using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;

    [Header("Patrol")]
    public float patrolDistance = 4f;
    public Transform[] patrolPoints;
    public float waitAtPoint = 2f;
    private Vector2 leftPoint, rightPoint;
    private Vector2 patrolTarget;

    [Header("Combat")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;

    private Transform player;
    private Rigidbody2D rb;
    private HealthSystem health;
    private int currentPatrolIndex;
    private float waitTimer;
    private float nextAttackTime;
    private bool isStunned;
    private bool facingRight = true;
    private Animator anim;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthSystem>();
        if (health != null) health.onDeath.AddListener(OnDeath);

        leftPoint = new Vector2(transform.position.x - patrolDistance * 0.5f, transform.position.y);
        rightPoint = new Vector2(transform.position.x + patrolDistance * 0.5f, transform.position.y);
        patrolTarget = rightPoint;
    }

    void Update()
    {
        if (isStunned || (health != null && !health.IsAlive())) return;
        float dist = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        if (dist <= attackRange && Time.time >= nextAttackTime) Attack();
        else if (dist <= detectionRange) ChasePlayer();
        else Patrol();
    }

    void Patrol()
    {
        if (waitTimer > 0) { waitTimer -= Time.deltaTime; rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }

        Vector2 dir = (patrolTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        if (dir.x > 0 && !facingRight) Flip();
        else if (dir.x < 0 && facingRight) Flip();

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.3f)
        {
            patrolTarget = (patrolTarget == rightPoint) ? leftPoint : rightPoint;
            waitTimer = waitAtPoint;
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * chaseSpeed, rb.linearVelocity.y);
        if (dir.x > 0 && !facingRight) Flip();
        else if (dir.x < 0 && facingRight) Flip();
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        nextAttackTime = Time.time + attackCooldown;
        if (anim != null) anim.SetTrigger("Attack");                              // ← add
        if (player != null) player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
    }

    public void Stun(float duration) => StartCoroutine(StunRoutine(duration));

    IEnumerator StunRoutine(float d)
    {
        isStunned = true;
        if (anim != null) anim.SetTrigger("Hurt");
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.yellow;
        yield return new WaitForSeconds(d);
        isStunned = false;
        if (sr) sr.color = Color.white;
    }

    void Flip() { facingRight = !facingRight; Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s; }

    void OnDeath()
    {
        if (anim != null) anim.SetBool("IsDead", true);                          // ← add
        GameManager.Instance?.EnemyDefeated();
        Destroy(gameObject, 1f);   // ← was 0.3f; longer so the death animation can finish
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}