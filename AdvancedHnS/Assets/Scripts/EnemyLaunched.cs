using UnityEngine;
using System.Collections;

public class EnemyLaunched : MonoBehaviour
{
    [HideInInspector] public bool isLaunched = false;
    private Rigidbody2D rb;
    private EnemyAI ai;
    private BossController boss;
    private SpriteRenderer sr;
    private float originalGravity;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<EnemyAI>();
        boss = GetComponent<BossController>();
        sr = GetComponent<SpriteRenderer>();
        if (rb != null) originalGravity = rb.gravityScale;
        if (sr != null) originalColor = sr.color;
    }

    public void Initialize()
    {
        isLaunched = true;
        StopAllCoroutines();
        if (rb != null) rb.gravityScale = Mathf.Max(originalGravity * 0.4f, 0.5f);
        if (sr != null) sr.color = new Color(1f, 0.55f, 0.1f, 1f);
        if (ai != null) ai.enabled = false;
        if (boss != null) boss.enabled = false;
        StartCoroutine(LaunchTimeout());
    }

    IEnumerator LaunchTimeout() { yield return new WaitForSeconds(3f); EndLaunch(); }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Ground")) EndLaunch();
    }

    void EndLaunch()
    {
        if (!isLaunched) return;
        isLaunched = false;
        if (rb != null) rb.gravityScale = originalGravity;
        if (sr != null) sr.color = originalColor;
        if (ai != null) ai.enabled = true;
        if (boss != null) boss.enabled = true;
        Destroy(this);
    }
}