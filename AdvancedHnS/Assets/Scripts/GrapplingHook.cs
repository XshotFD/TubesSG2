using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [Header("Hook Settings")]
    public float maxDistance = 15f;
    public float grappleSpeed = 30f;
    public LayerMask grappleableLayers;
    public Transform gunTip;

    [Header("Cooldown")]
    public float rechargeTime = 3f;          // <-- NEW: 3 seconds cooldown

    [Header("Visual")]
    public LineRenderer ropeRenderer;

    private Vector2 grapplePoint;
    private bool isGrappling;
    private DistanceJoint2D joint;
    private Rigidbody2D rb;

    private float rechargeTimer;             // <-- NEW: current cooldown remaining

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ropeRenderer != null) ropeRenderer.enabled = false;
        rechargeTimer = 0f;                 // ready immediately
    }

    void Update()
    {
        // Cooldown countdown
        if (rechargeTimer > 0f)
            rechargeTimer -= Time.deltaTime;

        // Rope visual update
        if (isGrappling && ropeRenderer != null)
        {
            ropeRenderer.SetPosition(0, gunTip.position);
            ropeRenderer.SetPosition(1, grapplePoint);
        }
    }

    public void OnGrapple(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) StartGrapple();
        else if (ctx.canceled) StopGrapple();
    }

    void StartGrapple()
    {
        // Cannot grapple if cooldown is still active or already grappling
        if (rechargeTimer > 0f || isGrappling) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxDistance, grappleableLayers);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            isGrappling = true;

            // Start the cooldown – will count down even while grappling
            rechargeTimer = rechargeTime;

            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedAnchor = grapplePoint;
            joint.distance = Vector2.Distance(transform.position, grapplePoint) * 0.8f;
            joint.maxDistanceOnly = true;

            Vector2 launchDir = (grapplePoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(launchDir * grappleSpeed, ForceMode2D.Impulse);

            if (ropeRenderer != null) ropeRenderer.enabled = true;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        if (joint != null) Destroy(joint);
        if (ropeRenderer != null) ropeRenderer.enabled = false;

        // Cooldown continues to run – no need to reset timer here.
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}