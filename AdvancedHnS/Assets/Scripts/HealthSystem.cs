using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1f;
    public Color damageFlashColor = Color.red;
    public float flashSpeed = 10f;

    [Header("Events")]
    public UnityEvent onDamage;
    public UnityEvent onHeal;
    public UnityEvent onDeath;

    private bool isInvincible;
    private float invincibilityTimer;
    private bool isDead;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void Update()
    {
        if (!isInvincible) return;
        invincibilityTimer -= Time.deltaTime;
        if (sr != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            sr.color = Color.Lerp(originalColor, damageFlashColor, alpha);
        }
        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            if (sr != null) sr.color = originalColor;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible || isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        onDamage?.Invoke();
        if (currentHealth <= 0) Die();
        else { isInvincible = true; invincibilityTimer = invincibilityDuration; }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHeal?.Invoke();
    }

    public bool IsAlive() => !isDead;

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
        Destroy(gameObject, 0.5f);
    }

    void OnValidate() => currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
}