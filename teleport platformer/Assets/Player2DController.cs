using UnityEngine;

public class Player2DController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 12f;
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Physics Settings")]
    public float gravityScale = 3f;
    public LayerMask groundLayerMask = 1; // Default layer

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Crouch Settings")]
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.2f;
    public float crouchColliderHeight = 1f;

    // Components
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer spriteRenderer;

    // State variables
    private bool isGrounded;
    private bool isCrouching;
    private bool canStandUp;
    private float originalColliderHeight;
    private Vector2 originalColliderOffset;

    // Input variables
    private float horizontalInput;
    private bool jumpInput;
    private bool crouchInput;
    private bool sprintInput;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set gravity scale
        rb.gravityScale = gravityScale;

        // Store original collider values
        originalColliderHeight = capsuleCollider.size.y;
        originalColliderOffset = capsuleCollider.offset;

        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -capsuleCollider.size.y / 2, 0);
            groundCheck = groundCheckObj.transform;
        }

        // Create ceiling check if it doesn't exist
        if (ceilingCheck == null)
        {
            GameObject ceilingCheckObj = new GameObject("CeilingCheck");
            ceilingCheckObj.transform.SetParent(transform);
            ceilingCheckObj.transform.localPosition = new Vector3(0, capsuleCollider.size.y / 2, 0);
            ceilingCheck = ceilingCheckObj.transform;
        }
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        CheckCeiling();
        HandleMovement();
        HandleJump();
        HandleCrouch();
    }

    void HandleInput()
    {
        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        sprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }

    void CheckCeiling()
    {
        canStandUp = !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayerMask);
    }

    void HandleMovement()
    {
        float currentSpeed = moveSpeed;

        // Apply sprint multiplier
        if (sprintInput && !isCrouching)
        {
            currentSpeed *= sprintMultiplier;
        }

        // Apply crouch speed reduction
        if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }

        // Move the player
        rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);

        // Flip sprite based on movement direction
        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    void HandleJump()
    {
        if (jumpInput && isGrounded && !isCrouching)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void HandleCrouch()
    {
        if (crouchInput && isGrounded)
        {
            if (!isCrouching)
            {
                // Start crouching
                isCrouching = true;

                // Adjust collider
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchColliderHeight);
                capsuleCollider.offset = new Vector2(originalColliderOffset.x,
                    originalColliderOffset.y - (originalColliderHeight - crouchColliderHeight) / 2);
            }
        }
        else if (isCrouching && canStandUp)
        {
            // Stop crouching
            isCrouching = false;

            // Reset collider
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderHeight);
            capsuleCollider.offset = originalColliderOffset;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw ceiling check
        if (ceilingCheck != null)
        {
            Gizmos.color = canStandUp ? Color.green : Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }
}