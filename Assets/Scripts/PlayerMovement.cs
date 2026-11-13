using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHydration))] 
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;

    [Header("Downhill Buff Settings")]
    public float downhillSpeedMultiplier = 1.3f;
    public float downhillHydrationMultiplier = 0.5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f; 
    public float coyotetime = 0.12f;
    private float coyoteTimeCounter;

    [Header("Ladder Settings")]
    public float ladderClimbSpeed = 3f;
    private bool isNearLadder = false;
    private bool isClimbingLadder = false;
    private float originalGravity;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public float slopeCheckDistance = 0.5f;
    
    private Rigidbody2D rb;
    private PlayerHydration hydrationSystem;
    private PlayerHealth healthSystem;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool isJumping; 
    private bool isSprinting;
    private bool isOnSlope;
    private bool isGoingDownhill;
    private bool hasTriggerDeath;
    private Vector2 slopeNormalPerp;
    private float moveInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hydrationSystem = GetComponent<PlayerHydration>(); 
        healthSystem = GetComponent<PlayerHealth>(); 
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (healthSystem.isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if(!hasTriggerDeath && anim !=null)
            {
                anim.SetTrigger("Death");
                hasTriggerDeath = true;
            }
            return; 
        }
        else if (hasTriggerDeath)
        {
            hasTriggerDeath = false;
            if(anim !=null)
            {
                anim.ResetTrigger("Death");
                anim.Play("Idle");
            }
        }

        // 1. GROUND CHECK (Ignore ground detection while climbing ladder)
        if (!isClimbingLadder)
        {
            Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            isGrounded = hitCollider != null;
        }
        else
        {
            isGrounded = false;
        }

        if(isGrounded)
        {
            coyoteTimeCounter = coyotetime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // STRICT SINGLE JUMP RESET
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
        }

        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        SlopeCheck(moveInput);

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (isGrounded)
        {
            isSprinting = shiftHeld;
        }
        else if (!shiftHeld)
        {
            isSprinting =false;
        }
        float baseSpeed = isSprinting ? sprintSpeed : walkSpeed;

        float hydrationPct = hydrationSystem.currentHydration / hydrationSystem.maxHydration;
        float speedMultiplier = 1f;

        if (hydrationPct <= 0.25f)
        {
            speedMultiplier = 0.5f;
        }
        else if (hydrationPct <= 0.5f)
        {
            speedMultiplier = 0.75f;
        }

        float currentSpeed = baseSpeed * speedMultiplier;

        if (isGrounded && isOnSlope && isGoingDownhill)
        {
            currentSpeed *= downhillSpeedMultiplier;
        }

        // HORIZONTAL MOVEMENT
        if (!isClimbingLadder)
        {
            if (isGrounded && isOnSlope && !isJumping)
            {
                rb.linearVelocity = new Vector2(-moveInput * currentSpeed * slopeNormalPerp.x, -moveInput * currentSpeed * slopeNormalPerp.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
            }
        }

        if (isSprinting && moveInput != 0) 
        {
            float drainMultiplier = (isOnSlope && isGoingDownhill) ? downhillHydrationMultiplier : 1f;
            hydrationSystem.DrainFromSprint(drainMultiplier);
        }

        // 2. LADDER CLIMB INPUT
        verticalInput = Input.GetAxisRaw("Vertical");
        if (isNearLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbingLadder = true;
        }

        // 3. LADDER DISMOUNT & JUMP HANDLING
        if (isClimbingLadder)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopClimbingLadder();
                return; // Prevents triggering a ground jump on the same frame
            }

            if (!isNearLadder)
            {
                StopClimbingLadder();
            }
        }
        else
        {
            // INITIATE GROUND JUMP
            if (Input.GetKeyDown(KeyCode.Space) && coyoteTimeCounter >0f && !isJumping)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                hydrationSystem.DrainFromJump(); 
                isJumping = true;
                coyoteTimeCounter = 0f;
            }
        }

        // VARIABLE JUMP HEIGHT
        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
        {
            if (rb.linearVelocity.y > 0) 
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }

        if (spriteRenderer !=null)
        {
            if (moveInput > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
        }

        if(anim !=null )
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        if (isClimbingLadder)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput * walkSpeed, verticalInput * ladderClimbSpeed);
        }
    }

    public void SetNearLadder(bool near, float speed)
    {
        isNearLadder = near;
        if (speed > 0f) ladderClimbSpeed = speed;

        if (!near)
        {
            StopClimbingLadder();
        }
    }

    private void StopClimbingLadder()
    {
        if (isClimbingLadder)
        {
            isClimbingLadder = false;
            rb.gravityScale = originalGravity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void SlopeCheck(float moveInput)
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, slopeCheckDistance, groundLayer);
        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            if (Mathf.Abs(hit.normal.x) > 0.1f)
            {
                isOnSlope = true;

                float slopeDirectionY = -moveInput * slopeNormalPerp.y;
                isGoingDownhill = slopeDirectionY < -0.05f;
            }
            else
            {
                isOnSlope = false;
                isGoingDownhill = false;
            }

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.green);
            Debug.DrawRay(hit.point, hit.normal, Color.red);
        }
        else
        {
            isOnSlope = false;
            isGoingDownhill = false;
        }
    }
}
