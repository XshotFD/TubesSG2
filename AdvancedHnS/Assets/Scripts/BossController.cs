using UnityEngine;

public class BossController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;

    private Transform player;
    private Animator anim;
    private HealthSystem health;
    private bool isDead;

    void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<HealthSystem>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (health != null) health.onDeath.AddListener(OnDeath);
    }

    void Update()
    {
        if (isDead || player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < detectionRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            GetComponent<SpriteRenderer>().flipX = dir.x < 0;
            anim.SetFloat("Speed", moveSpeed);
            if (dist < attackRange) anim.SetTrigger("Attack");
        }
        else anim.SetFloat("Speed", 0);
    }

    void OnDeath() { isDead = true; anim.SetBool("IsDead", true); GameManager.Instance?.EnemyDefeated(); Destroy(gameObject, 2f); }
}