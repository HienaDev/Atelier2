using UnityEngine;
public class PlayerMovementEverHood : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private int gridSize = 5;
    [SerializeField] private bool jumpEnabled = false;
    [SerializeField] private float cellDistance = 1.0f;
    [SerializeField] private float inputBufferTime = 0.25f; // How long to buffer input for

    [SerializeField] private Transform bottomHalf;

    public int GridSize { get { return gridSize; } }
    public float CellDistance { get { return cellDistance; } }

    private Vector3 startPosition;
    public Vector3 StartPosition { get { return startPosition; } }
    private Vector3 targetPosition;
    private int currentZ;
    private bool isJumping = false;
    private float jumpTimer = 0f;

    // Input tracking variables
    private float previousHorizontalInput = 0f;
    private bool canMove = true;

    // Input buffering variables
    private float bufferedDirection = 0f;
    private float bufferTimeLeft = 0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // Get the starting position when the script is enabled
        startPosition = transform.position;
        targetPosition = startPosition;
        // Calculate current Z position (assuming middle of grid as default)
        currentZ = gridSize / 2;
    }

    void Update()
    {
        rb.linearVelocity = Vector3.zero;
        // Process movement inputs
        ProcessMovement();

        // Handle jump input
        if (!isJumping && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            jumpTimer = 0f;
        }

        // Handle jumping
        if (isJumping && jumpEnabled)
        {
            Jump();
        }
    }

    private void ProcessMovement()
    {
        // Get current horizontal input
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Update buffer timer
        if (bufferTimeLeft > 0)
        {
            bufferTimeLeft -= Time.deltaTime;
            if (bufferTimeLeft <= 0)
            {
                // Clear buffer when time expires
                bufferedDirection = 0f;
            }
        }

        // Check if we've reached the destination position
        bool atTargetPosition = Mathf.Approximately(transform.position.z, targetPosition.z);
        if (atTargetPosition)
        {
            canMove = true;
        }

        // Detect new button presses by checking for changes from 0 to non-zero
        bool newLeftPress = previousHorizontalInput == 0 && horizontalInput < 0;
        bool newRightPress = previousHorizontalInput == 0 && horizontalInput > 0;

        // Handle buffered input during movement
        if (!canMove && (newLeftPress || newRightPress))
        {
            bufferedDirection = horizontalInput;
            bufferTimeLeft = inputBufferTime;
        }

        // Process movement when at target position
        if (canMove && atTargetPosition)
        {
            // First check for immediate new press
            if (newLeftPress && currentZ < gridSize - 1)
            {
                // Move left along Z-axis
                currentZ++;
                targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
                canMove = false;
                bufferTimeLeft = 0f; // Clear any buffer
            }
            else if (newRightPress && currentZ > 0)
            {
                // Move right along Z-axis
                currentZ--;
                targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
                canMove = false;
                bufferTimeLeft = 0f; // Clear any buffer
            }
            // If no immediate press but we have buffered input
            else if (bufferedDirection != 0)
            {
                if (bufferedDirection < 0 && currentZ < gridSize - 1)
                {
                    // Move left using buffered input
                    currentZ++;
                    targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
                    canMove = false;
                }
                else if (bufferedDirection > 0 && currentZ > 0)
                {
                    // Move right using buffered input
                    currentZ--;
                    targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
                    canMove = false;
                }

                // Clear buffer after use
                bufferedDirection = 0f;
                bufferTimeLeft = 0f;
            }
        }

        // Store current input for next frame comparison
        previousHorizontalInput = horizontalInput;
    }

    private void FixedUpdate()
    {
        // Move toward target position (Z-axis only)
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(transform.position.x, transform.position.y, targetPosition.z),
            moveSpeed * Time.deltaTime
        );

        // Rotate bottomHalf toward movement direction even after reaching the target
        Vector3 desiredDirection = targetPosition - transform.position;

        // Only proceed if the direction is not nearly zero
        if (desiredDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection.normalized, Vector3.up);
            float rotationSpeedDegreesPerSecond = 1080; // adjust as needed (360 = 1 full rotation per second)
            bottomHalf.rotation = Quaternion.RotateTowards(
                bottomHalf.rotation,
                desiredRotation,
                rotationSpeedDegreesPerSecond * Time.deltaTime
            );
        }

    }

    private void Jump()
    {
        jumpTimer += Time.deltaTime;
        float jumpProgress = jumpTimer / jumpDuration;
        if (jumpProgress < 1f)
        {
            // Sine curve for smooth jump
            float jumpOffset = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + jumpOffset,
                transform.position.z
            );
        }
        else
        {
            // End jump
            transform.position = new Vector3(
                transform.position.x,
                startPosition.y,
                transform.position.z
            );
            isJumping = false;
        }
    }

    // Set a custom grid size and recalculate position
    public void SetGridSize(int newSize)
    {
        gridSize = newSize;
        currentZ = gridSize / 2;
        targetPosition.z = startPosition.z;
        transform.position = new Vector3(transform.position.x, transform.position.y, targetPosition.z);
    }

    // Set the distance between grid cells
    public void SetCellDistance(float distance)
    {
        cellDistance = distance;
    }
}