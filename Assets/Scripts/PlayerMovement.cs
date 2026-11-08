using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHydration))] 
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    
    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f; 

    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    
    private Rigidbody2D rb;
    private PlayerHydration hydrationSystem;
    private PlayerHealth healthSystem;
    private bool isGrounded;
    private bool isJumping; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hydrationSystem = GetComponent<PlayerHydration>(); 
        healthSystem = GetComponent<PlayerHealth>(); 
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

    // DEBUG: See what the ground check is actually hitting!
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

    float moveInput = 0f;
    if (Input.GetKey(KeyCode.A)) moveInput = -1f;
    if (Input.GetKey(KeyCode.D)) moveInput = 1f;

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

    rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

    if (isSprinting && moveInput != 0) 
    {
        hydrationSystem.DrainFromSprint();
    }

    // INITIATE JUMP
    if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
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
}
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}