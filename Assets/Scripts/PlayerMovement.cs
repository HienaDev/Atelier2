using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject bottomHalf;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float movSpeed = 5f;
    [SerializeField] private float jumpForce = 5f; // Added jump force
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer; // LayerMask to detect ground
    [SerializeField] private float groundCheckDistance = 0.1f; // Distance to check for ground

    private bool isGrounded;

    void Update()
    {
        // Check if the player is grounded
        CheckGrounded();

        // Handle movement
        HandleMovement();

        // Handle jumping
        if (isGrounded && Input.GetButtonDown("Jump")) // Jump when grounded and spacebar is pressed
        {
            Jump();
        }
    }

    private void CheckGrounded()
    {
        // Perform a raycast to check if the player is on the ground
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        // Get instant input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get camera directions
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Calculate movement direction
        Vector3 moveDirection = camRight * horizontal + camForward * vertical;

        // Normalize only if there's movement
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // Apply movement
        rb.linearVelocity = new Vector3(moveDirection.x * movSpeed, rb.linearVelocity.y, moveDirection.z * movSpeed);

        // Rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            bottomHalf.transform.rotation = Quaternion.Slerp(bottomHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void Jump()
    {
        // Apply upward force for jumping
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }
}