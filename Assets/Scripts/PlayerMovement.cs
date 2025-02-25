using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject bottomHalf;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float movSpeed = 5f;
    [SerializeField] private Rigidbody rb;

    void Update()
    {
        rb.linearVelocity = Vector3.zero;

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
}
