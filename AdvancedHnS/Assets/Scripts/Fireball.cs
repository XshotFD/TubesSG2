using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fireball : MonoBehaviour
{
    public float speed = 14f;
    public float lifetime = 3f;
    public GameObject impactEffectPrefab;
    public float impactEffectLifetime = 0.4f;
    public float spriteAngleOffset = 0f;

    [HideInInspector] public Vector2 travelDirection = Vector2.right;
    private Rigidbody2D rb;
    private bool hasHit;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Start()
    {
        rb.linearVelocity = travelDirection.normalized * speed;
        float angle = Mathf.Atan2(travelDirection.y, travelDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + spriteAngleOffset);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        if (other.isTrigger) return;
        SpawnImpact();
    }

    void OnDestroy() { if (!hasHit) SpawnImpact(); }

    void SpawnImpact()
    {
        if (hasHit) return;
        hasHit = true;
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, impactEffectLifetime);
        }
    }
}