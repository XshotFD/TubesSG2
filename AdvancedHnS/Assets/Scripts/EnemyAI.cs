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
    public Transform[] patrolPoints;
    public float waitAtPoint = 2f;

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

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthSystem>();
        if (health != null) health.onDeath.AddListener(OnDeath);
    }

    void Update()
    {
        if (isStunned || (health != null && !health.IsAlive())) return;
        float dist = player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        if (dist <= attackRange && Time.time >= nextAttackTime) Attack();
        else if (dist <= detectionRange) ChasePlayer();
        else Patrol();
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }
        if (waitTimer > 0) { waitTimer -= Time.deltaTime; rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }

        Transform target = patrolPoints[currentPatrolIndex];
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        if (dir.x > 0 && !facingRight) Flip();
        else if (dir.x < 0 && facingRight) Flip();

        if (Vector2.Distance(transform.position, target.position) < 0.3f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
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
        if (player != null) player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
    }

    public void Stun(float duration) => StartCoroutine(StunRoutine(duration));

    IEnumerator StunRoutine(float d)
    {
        isStunned = true;
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.yellow;
        yield return new WaitForSeconds(d);
        isStunned = false;
        if (sr) sr.color = Color.white;
    }

    void Flip() { facingRight = !facingRight; Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s; }

    void OnDeath() { GameManager.Instance?.EnemyDefeated(); Destroy(gameObject, 0.3f); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}