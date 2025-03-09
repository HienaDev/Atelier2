using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private GameObject bottomHalf; 
    [SerializeField] private GameObject topHalf;    
    [SerializeField] private BobbingMotion bobbingScript; // Reference to the bobbing script

    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float movSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private bool isGrounded;
    private bool isFacingForward = true; 

    void Update()
    {
        CheckGrounded();
        HandleMovement();

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    private void CheckGrounded()
    {
        // Check if player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Reactivate bobbing when touching the ground
        if (isGrounded && !bobbingScript.enabled)
        {
            bobbingScript.enabled = true;
        }
    }

    private void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Horizontal"); 

        // Apply movement only on Z-axis
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, moveZ * movSpeed);

        if (moveZ > 0 && !isFacingForward)
        {
            Flip();
        }
        else if (moveZ < 0 && isFacingForward)
        {
            Flip();
        }

        // Smoothly rotate bottomHalf towards movement direction
        if (moveZ != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(0, 0, moveZ));
            bottomHalf.transform.rotation = Quaternion.Slerp(bottomHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void Jump()
    {
        // Disable bobbing while in the air
        bobbingScript.enabled = false;

        // Apply jump force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void Flip()
    {
        isFacingForward = !isFacingForward;

        // Instantly rotate the top half 180 degrees
        topHalf.transform.Rotate(0, 180, 0);
    }
}