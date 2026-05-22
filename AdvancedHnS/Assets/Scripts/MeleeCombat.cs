using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MeleeCombat : MonoBehaviour
{
    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayers;
    public float attackCooldown = 0.7f;

    [Header("Combo")]
    public float comboWindow = 0.6f;
    public float[] comboDamages = { 15f, 20f, 30f };
    public float knockbackForce = 8f;
    public Vector2[] comboKnockbackAngles = {
        new Vector2(1f, 0.3f),
        new Vector2(1f, 0.8f),
        new Vector2(0.5f, 1.5f)
    };

    private int comboCount = -1;
    private float comboTimer;
    private float nextAttackTime;
    private bool isAttacking;
    private Animator anim;

    void Awake() => anim = GetComponent<Animator>();

    // Called from PlayerInput
    public void OnMelee(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Time.time >= nextAttackTime && !isAttacking)
            PerformAttack();
    }

    void PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;
        comboCount = (comboCount + 1) % comboDamages.Length;
        comboTimer = comboWindow;

        float damage = comboDamages[comboCount];
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D hit in hits)
        {
            HealthSystem hs = hit.GetComponent<HealthSystem>();
            if (hs != null) hs.TakeDamage(damage);

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = comboKnockbackAngles[comboCount];
                if (transform.localScale.x < 0) dir.x *= -1; // flip based on facing
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }

            EnemyAI ai = hit.GetComponent<EnemyAI>();
            if (ai != null) ai.Stun(0.2f * (comboCount + 1));
        }

        if (anim != null) anim.SetTrigger("Attack");
        StartCoroutine(EndAttack());
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}