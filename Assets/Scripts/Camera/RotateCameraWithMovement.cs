using UnityEngine;

public class RotateCameraOnMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float maxYRotation = 30f; // Maximum rotation on Y axis (left/right)
    [SerializeField] private float maxXRotation = 20f; // Maximum rotation on X axis (up/down)
    [SerializeField] private float rotationSpeed = 3f; // How quickly rotation reaches max angle
    [SerializeField] private float returnSpeed = 2f; // How quickly camera returns to original position
    [SerializeField] private float inputThreshold = 0.1f; // Minimum input value to trigger rotation

    // Original rotation values
    private Quaternion originalRotation;
    // Target rotation we're easing towards
    private Quaternion targetRotation;

    private void Start()
    {
        // Store the original rotation when the game starts
        originalRotation = transform.rotation;
        targetRotation = originalRotation;
    }

    private void LateUpdate()
    {
        // Get input from Unity's input system
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D keys or left/right arrows by default
        float verticalInput = -Input.GetAxisRaw("Vertical");     // W/S keys or up/down arrows by default

        // Check if input exceeds the threshold
        bool hasSignificantInput = Mathf.Abs(horizontalInput) > inputThreshold ||
                                   Mathf.Abs(verticalInput) > inputThreshold;

        // Calculate new target rotation based on input
        if (hasSignificantInput)
        {
            // Create rotation values based on input
            float targetYRotation = horizontalInput * maxYRotation;
            float targetXRotation = verticalInput * maxXRotation;

            // Create a target rotation with our desired angles
            targetRotation = Quaternion.Euler(
                originalRotation.eulerAngles.x + targetXRotation,
                originalRotation.eulerAngles.y + targetYRotation,
                originalRotation.eulerAngles.z
            );
        }
        else
        {
            // If no input, set target to original rotation
            targetRotation = originalRotation;
        }

        // Apply smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            (hasSignificantInput ? rotationSpeed : returnSpeed) * Time.deltaTime
        );
    }

    // Reset camera to original position
    public void ResetCamera()
    {
        transform.rotation = originalRotation;
        targetRotation = originalRotation;
    }
}