using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Movement speed
    [SerializeField] private Transform cameraTransform; // Assign the main camera in the inspector

    void Update()
    {


        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrow
        float vertical = Input.GetAxisRaw("Vertical"); // W/S or Up/Down arrow
        float up = Input.GetAxisRaw("Jump"); // Space bar/left alt

        // Get camera's forward and right vectors
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Project to XZ plane
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Calculate movement direction
        Vector3 moveDirection = camForward * vertical + camRight * horizontal + Vector3.up * up;


        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        // Apply movement
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
