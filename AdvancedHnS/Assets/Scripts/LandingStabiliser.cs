using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LandingStabiliser : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private bool onGround;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Vector2.Dot(contact.normal, Vector2.up) > 0.7f)
            {
                onGround = true;

                // If the player is NOT jumping (no active jump buffer, not mid‑jump), kill vertical velocity
                bool isJumping = playerMovement.isJumping || playerMovement.jumpBufferCounter > 0;
                if (!isJumping)
                {
                    // Immediately zero vertical speed – catches hover‑induced bounce
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }

                // Constant downward force to keep the player glued
                rb.AddForce(Vector2.down * 5f, ForceMode2D.Force);
                return;
            }
        }
        onGround = false;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            onGround = false;
    }

    public bool IsOnGround() => onGround;
}