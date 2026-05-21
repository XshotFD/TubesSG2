using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    public float maxDistance = 15f;
    public float grappleSpeed = 30f;
    public LayerMask grappleableLayers;
    public Transform gunTip;
    public LineRenderer ropeRenderer;

    private Vector2 grapplePoint;
    private bool isGrappling;
    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    private bool grappleHeld;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (ropeRenderer != null) ropeRenderer.enabled = false;
    }

    public void OnGrapple(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) StartGrapple();
        else if (ctx.canceled) StopGrapple();
    }

    void StartGrapple()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, maxDistance, grappleableLayers);
        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            isGrappling = true;
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
    }

    void Update()
    {
        if (isGrappling && ropeRenderer != null)
        {
            ropeRenderer.SetPosition(0, gunTip.position);
            ropeRenderer.SetPosition(1, grapplePoint);
        }
    }
}