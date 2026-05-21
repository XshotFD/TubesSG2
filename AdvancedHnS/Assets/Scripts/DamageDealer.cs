using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public float damageAmount = 10f;
    public bool destroyOnContact = true;
    public bool damagePlayer = true;
    public bool damageEnemies = false;
    public bool applyKnockback = false;
    public float knockbackForce = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (damagePlayer && other.CompareTag("Player")) ApplyDamage(other);
        if (damageEnemies && other.CompareTag("Enemy")) ApplyDamage(other);
    }

    void ApplyDamage(Collider2D other)
    {
        HealthSystem hs = other.GetComponent<HealthSystem>();
        if (hs == null) return;
        hs.TakeDamage(damageAmount);
        if (applyKnockback)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        if (destroyOnContact) Destroy(gameObject);
    }
}