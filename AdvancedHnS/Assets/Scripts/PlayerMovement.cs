using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement movement;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Slide")]
    public float slideSpeed = 15f;
    public float slideDuration = 0.5f;
    public float slideCooldown = 1f;

    [Header("Hover")]
    public float hoverForce = 6f;
    public float hoverDuration = 0.4f;
    public float hoverCooldown = 2f;
    public int maxHoverCharges = 1;

    [Header("Air Hang")]
    public float hangDuration = 1.5f;      
    public float hangVerticalSpeed = 0f;   

    private bool airHangHeld;              
    private bool isHanging;                
    private float hangTimer;               
    private bool airHangAvailable;

    // Public state for other scripts
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isSliding;
    
    private Vector2 moveInput;
    private float coyoteTimeCounter;
    public float jumpBufferCounter;
    public bool isJumping;

    private float slideTimer;
    private float slideCooldownTimer;

    private bool isHovering;
    private float hoverTimer;
    private float hoverCooldownTimer;
    private int currentHoverCharges;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();   // <--- add this line
    }

    void Start()
    {
        currentHoverCharges = maxHoverCharges;
        hangTimer = hangDuration;
        airHangAvailable = false;
        if (groundCheck == null) Debug.LogError("GroundCheck not assigned!");

    }

    void Update()
    {
        // Ground detection
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
        if (!isGrounded) airHangAvailable = true;
        if (isGrounded && jumpBufferCounter <= 0f && !isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        if (isGrounded)
        {
            currentHoverCharges = maxHoverCharges;
            airHangAvailable = false;
            hangTimer = hangDuration;        
        }

        UpdateTimers();
      
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", movement.isGrounded);
        anim.SetBool("isSliding", movement.isSliding);
        anim.SetBool("isHanging", movement.isHanging);
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleHover();
        HandleAirHang();
    }

    // ────────────── INPUT CALLBACKS ──────────────
    public void OnAirHang(InputAction.CallbackContext ctx)
    {
        airHangHeld = ctx.ReadValueAsButton();   // true while held, false when released
    }
    public void TriggerAttack() => anim.SetTrigger("Attack");
    public void SetDead(bool dead) => anim.SetBool("IsDead", dead);

    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log($"OnJump: performed={ctx.performed}, phase={ctx.phase}");
        if (ctx.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (ctx.canceled && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f); // variable jump height
    }

    public void OnSlide(InputAction.CallbackContext ctx)
    {
        Debug.Log($"OnSlide called - performed={ctx.performed}");
        if (ctx.performed && Mathf.Abs(moveInput.x) > 0.1f && !isSliding && slideCooldownTimer <= 0 && isGrounded)
            StartSlide();
        if (ctx.performed)
        {
            Debug.Log($"Slide check: moveInput.x={moveInput.x} (>{0.1f:?}), isSliding={isSliding}, cooldown={slideCooldownTimer} (<=0? {slideCooldownTimer <= 0}), isGrounded={isGrounded}");
            if (Mathf.Abs(moveInput.x) > 0.1f && !isSliding && slideCooldownTimer <= 0 && isGrounded)
                StartSlide();
        };
    }

    public void OnHover(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !isGrounded && !isHovering && currentHoverCharges > 0 && hoverCooldownTimer <= 0)
            StartHover();
    }
    // ──────────────────────────────────────────────
    void HandleMovement()
    {
        float targetSpeed;
        if (isSliding)
            targetSpeed = Mathf.Sign(transform.localScale.x) * slideSpeed;
        else
            targetSpeed = moveInput.x * moveSpeed;

        // Smooth velocity change (similar to the SeaSG repo)
        rb.linearVelocity = new Vector2(
            Mathf.Lerp(rb.linearVelocity.x, targetSpeed, Time.fixedDeltaTime * 10f),
            rb.linearVelocity.y
        );

        // Flip sprite
        if (!isSliding)
        {
            if (moveInput.x > 0.1f)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            else if (moveInput.x < -0.1f)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    void HandleJump()
    {
        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0 && !isJumping)
        {
            Debug.Log($"JUMP! coyote={coyoteTimeCounter}, buffer={jumpBufferCounter}");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            StartCoroutine(JumpCooldown());
        }
    }

    
    void HandleHover()
    {
        if (isHovering) rb.linearVelocity = new Vector2(rb.linearVelocity.x, hoverForce);
    }

    void HandleAirHang()
    {
        // Can we hang right now?
        if (airHangHeld && !isGrounded && airHangAvailable && hangTimer > 0f)
        {
            isHanging = true;
            hangTimer -= Time.fixedDeltaTime;

            // Override vertical velocity to freeze/slow descent
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, hangVerticalSpeed);

            // Optionally disable gravity while hanging (cleaner)
            rb.gravityScale = 0f;
        }
        else
        {
            isHanging = false;
            // Restore default gravity when not hanging
            rb.gravityScale = 3f;   // match your default Gravity Scale

            // Once we've used up the hang time or released, mark unavailable until next landing
            if (!airHangHeld || hangTimer <= 0f)
                airHangAvailable = false;
        }
    }

    IEnumerator JumpCooldown() { isJumping = true; yield return new WaitForSeconds(0.1f); isJumping = false; }

    void StartSlide() { isSliding = true; slideTimer = slideDuration; slideCooldownTimer = slideCooldown; }
    void StopSlide() => isSliding = false;

    void StartHover()
    {
        isHovering = true; 
        hoverTimer = hoverDuration; 
        currentHoverCharges--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // reset vertical speed
        jumpBufferCounter = 0f;
    }
    void StopHover() { isHovering = false; hoverCooldownTimer = hoverCooldown; }

    void UpdateTimers()
    {
        if (isSliding) { slideTimer -= Time.deltaTime; if (slideTimer <= 0) StopSlide(); }
        if (!isSliding && slideCooldownTimer > 0) slideCooldownTimer -= Time.deltaTime;

        if (isHovering) { hoverTimer -= Time.deltaTime; if (hoverTimer <= 0) StopHover(); }
        if (!isHovering && hoverCooldownTimer > 0) hoverCooldownTimer -= Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}