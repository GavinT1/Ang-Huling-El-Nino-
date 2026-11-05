using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHydration))] 
[RequireComponent(typeof(PlayerHealth))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpForce = 12f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private PlayerHydration hydrationSystem;
    private PlayerHealth healthSystem;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hydrationSystem = GetComponent<PlayerHydration>(); // Link to the new script
        healthSystem = GetComponent<PlayerHealth>(); // Link to the health script
    }

    void Update()
    {
        // Stop movement if hydration is empty
        if (hydrationSystem.currentHydration <= 0 || healthSystem.isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; 
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

        
        if (isSprinting && moveInput != 0) 
        {
            hydrationSystem.DrainFromSprint();
        }

        
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            hydrationSystem.DrainFromJump(); 
        }
    }
}