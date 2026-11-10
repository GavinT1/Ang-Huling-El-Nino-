using UnityEngine;

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

    [Header("Ladder Settings")]
    private float ladderClimbSpeed = 3f;
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
    private bool isGrounded;
    private bool isJumping; 
    private bool isOnSlope;
    private bool isGoingDownhill;
    private Vector2 slopeNormalPerp;
    private float moveInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hydrationSystem = GetComponent<PlayerHydration>(); 
        healthSystem = GetComponent<PlayerHealth>(); 
        originalGravity = rb.gravityScale;
    }

    void Update()
{
    if (healthSystem.isDead)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        return; 
    }

    // GROUND CHECK
    Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    isGrounded = hitCollider != null;

    if (isGrounded)
    {
        Debug.Log("Grounded on object: " + hitCollider.gameObject.name);
    }
    else
    {
        Debug.Log("NOT Grounded!");
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

    bool isSprinting = Input.GetKey(KeyCode.LeftShift);
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
        Debug.Log("Downhill speed boost applied. Current Speed: " + currentSpeed);
    }

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

    // INITIATE JUMP
    if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping && !isClimbingLadder)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        hydrationSystem.DrainFromJump(); 
        isJumping = true;
    }

    // VARIABLE JUMP HEIGHT
    if (Input.GetKeyUp(KeyCode.Space) && isJumping)
    {
        if (rb.linearVelocity.y > 0) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }
    verticalInput = Input.GetAxisRaw("Vertical");
    if (isNearLadder && Mathf.Abs(verticalInput) > 0.1f)
    {
        isClimbingLadder = true;
    }
    if (isClimbingLadder && (Input.GetKeyDown(KeyCode.Space) || !isNearLadder))
    {
        StopClimbingLadder();
    }
}
    private void FixedUpdate()
    {
        if(isClimbingLadder)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput * walkSpeed, verticalInput * ladderClimbSpeed);
        }
    }

    public void SetNearLadder(bool near, float speed)
    {
        isNearLadder = near;
        ladderClimbSpeed = speed;
        if (!near)
        {
            StopClimbingLadder();
        }
    }

    private void StopClimbingLadder()
    {
        isClimbingLadder = false;
        rb.gravityScale = originalGravity;
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
