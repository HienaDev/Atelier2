using UnityEngine;

public class PlayerMovementQuark : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject bottomHalf;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float movSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private bool isGrounded;
    private bool isJumping = false;
    private bool isFalling = false;
    private float startY;
    private float jumpPeakY;

    void Awake()
    {
        startY = transform.position.y; // Store initial ground height
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
    }

    private void CheckGrounded()
    {
        // Perform a raycast slightly below the player to check for ground
        RaycastHit hit;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundCheckDistance + 0.2f, groundLayer);

        // Reset jump states when landing
        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
            isFalling = false;
            startY = hit.point.y; // Ensure accurate landing position
        }
    }

    private void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Horizontal");

        // Apply movement only on Z-axis
        transform.position += new Vector3(0, 0, moveZ * movSpeed * Time.deltaTime);

        // Smoothly rotate bottomHalf towards movement direction
        if (moveZ != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(0, 0, moveZ));
            bottomHalf.transform.rotation = Quaternion.Slerp(bottomHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            StartJump();
        }

        if (isJumping)
        {
            JumpUp();
        }
        else if (isFalling)
        {
            FallDown();
        }
    }

    private void StartJump()
    {
        isJumping = true;
        isFalling = false;
        jumpPeakY = startY + jumpHeight;
    }

    private void JumpUp()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, jumpPeakY, transform.position.z), jumpSpeed * Time.deltaTime);

        // If reached peak, start falling
        if (Mathf.Abs(transform.position.y - jumpPeakY) < 0.05f)
        {
            isJumping = false;
            isFalling = true;
        }
    }

    private void FallDown()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, startY, transform.position.z), fallSpeed * Time.deltaTime);

        // If landed, reset jump state (ground check will handle the final reset)
        if (Mathf.Abs(transform.position.y - startY) < 0.05f)
        {
            isFalling = false;
        }
    }
}