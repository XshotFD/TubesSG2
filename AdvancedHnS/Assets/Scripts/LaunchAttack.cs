using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class LaunchAttack : MonoBehaviour
{
    [Header("Shared")]
    public Transform attackPoint;
    public float attackRange = 1.8f;
    public LayerMask enemyLayers;

    [Header("Launch (Ground)")]
    public float launchEnemyForce = 18f;
    public float launchPlayerForce = 14f;
    public float launchDamage = 30f;

    [Header("Aerial (Air)")]
    public float aerialEnemyForce = 10f;
    public float aerialHangTime = 0.35f;
    public float aerialDamage = 18f;

    [Header("Cooldown")]
    public float attackCooldown = 0.4f;

    [Header("Animation")]
    public string launchAnimParam = "LaunchAttack";
    public string aerialAnimParam = "AerialAttack";

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerMovement movement;
    private float nextAttackTime;
    private bool performingAerial;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    public void OnLaunchAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        if (movement.isGrounded) PerformLaunchAttack();
        else PerformAerialAttack();
    }

    void PerformLaunchAttack()
    {
        anim?.SetTrigger(launchAnimParam);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool hit = false;
        foreach (var c in hits)
        {
            hit = true;
            c.GetComponent<HealthSystem>()?.TakeDamage(launchDamage);
            Rigidbody2D eRb = c.GetComponent<Rigidbody2D>();
            if (eRb != null)
            {
                eRb.linearVelocity = new Vector2(eRb.linearVelocity.x * 0.3f, launchEnemyForce);
                EnemyLaunched l = c.GetComponent<EnemyLaunched>();
                if (l == null) l = c.gameObject.AddComponent<EnemyLaunched>();
                l.Initialize();
            }
        }
        if (hit) rb.linearVelocity = new Vector2(rb.linearVelocity.x, launchPlayerForce);
    }

    void PerformAerialAttack()
    {
        if (performingAerial) return;
        anim?.SetTrigger(aerialAnimParam);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (var c in hits)
        {
            c.GetComponent<HealthSystem>()?.TakeDamage(aerialDamage);
            Rigidbody2D eRb = c.GetComponent<Rigidbody2D>();
            if (eRb != null)
            {
                eRb.linearVelocity = new Vector2(eRb.linearVelocity.x, Mathf.Max(eRb.linearVelocity.y + 5f, aerialEnemyForce));
                EnemyLaunched l = c.GetComponent<EnemyLaunched>();
                if (l == null) l = c.gameObject.AddComponent<EnemyLaunched>();
                l.Initialize();
            }
        }
        StartCoroutine(AerialHang());
    }

    IEnumerator AerialHang()
    {
        performingAerial = true;
        float g = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        yield return new WaitForSeconds(aerialHangTime);
        rb.gravityScale = g;
        performingAerial = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}