using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthSystem))]
public class BossAI : MonoBehaviour
{
    [Header("Ranges")]
    public float detectionRange = 12f;   // how far the boss sees the player
    public float meleeRange = 2.5f;      // how close to use melee
    public float beamRange = 8f;         // how far to use the beam attack

    [Header("Timing")]
    public float attackCooldown = 2f;    // time between attacks
    public bool useBeam = true;
    public bool useMelee = true;

    [Header("Smooth Turn")]
    public float turnSpeed = 5f;               // how fast the boss turns
    private Coroutine turnCoroutine;

    private Transform player;
    private HealthSystem health;
    private Component dk;                // the asset's DarkKnightController
    private float nextAttackTime;
    private bool isDead;
    private bool facingRight = true;
    private string currentMove = "";     // prevents re-issuing the same animation

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        health = GetComponent<HealthSystem>();
        // Find the controller by its class name as a string (no namespace issues)
        dk = GetComponent("DarkKnightController");
        if (dk == null)
            Debug.LogError("DarkKnightController not found on the boss object!");

        if (health != null)
            health.onDeath.AddListener(OnDeath);

        SetMove("ActivateIdle");
    }

    void Update()
    {
        if (isDead || player == null || dk == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        FacePlayer();  // make the boss face the player

        // 1. If close enough, do a melee attack
        if (useMelee && dist <= meleeRange)
        {
            SetMove("ActivateIdle");  // stop moving
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                Call("Guard");  // the manual says Attack only works from Guard state
                Call("Attack");
            }
        }
        // 2. If at medium range, fire a beam (works from Idle)
        else if (useBeam && dist <= beamRange)
        {
            SetMove("ActivateIdle");
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                Call("Beam");
            }
        }
        // 3. If far but within detection, chase the player
        else if (dist <= detectionRange)
        {
            SetMove("ActivateRun");
        }
        // 4. Otherwise, just stand idle
        else
        {
            SetMove("ActivateIdle");
        }
    }

    // ── Helpers ────────────────────────────────────

    // Flips the boss to face the player using the controller's Flip method
    void FacePlayer()
    {
        bool playerOnRight = player.position.x > transform.position.x;
        float targetScaleX = playerOnRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);

        // Only start a new turn if the target direction is different
        if (Mathf.Abs(targetScaleX - transform.localScale.x) > 0.01f)
        {
            if (turnCoroutine != null) StopCoroutine(turnCoroutine);
            turnCoroutine = StartCoroutine(SmoothTurn(targetScaleX));
        }
    }

    IEnumerator SmoothTurn(float targetX)
    {
        float startX = transform.localScale.x;
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * turnSpeed;
            float newX = Mathf.Lerp(startX, targetX, elapsed);
            transform.localScale = new Vector3(newX, transform.localScale.y, transform.localScale.z);
            yield return null;
        }
        transform.localScale = new Vector3(targetX, transform.localScale.y, transform.localScale.z);
    }

    // Called when HealthSystem reaches 0 HP
    void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        Call("Death");   // plays the dissolve + explosion animation automatically
        GameManager.Instance?.EnemyDefeated();
    }

    // Only sends a movement command if it's actually different from the current one
    void SetMove(string method)
    {
        if (currentMove == method) return;
        currentMove = method;
        Call(method);
    }

    // Calls a method on the DarkKnightController by name (safe fallback)
    void Call(string method)
    {
        if (dk != null)
            dk.SendMessage(method, SendMessageOptions.DontRequireReceiver);
    }
}