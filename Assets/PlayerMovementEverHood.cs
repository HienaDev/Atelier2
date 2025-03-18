using UnityEngine;

public class PlayerMovementEverHood : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private int gridSize = 5;
    public int GridSize { get { return gridSize; } }
    [SerializeField] private float cellDistance = 1.0f;
    public float CellDistance { get { return cellDistance; } }

    [SerializeField] private bool jumpEnabled = false;

    private Vector3 startPosition;
    public Vector3 StartPosition { get { return startPosition; } }
    private Vector3 targetPosition;
    private int currentZ;
    private bool isJumping = false;
    private float jumpTimer = 0f;

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
        // Handle Z-axis movement using horizontal input
        if (Mathf.Approximately(transform.position.z, targetPosition.z))
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            if (horizontalInput < 0 && currentZ < gridSize - 1)
            {
                // Move left along Z-axis
                currentZ++;
                targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
            }
            else if (horizontalInput > 0 && currentZ > 0)
            {
                // Move right along Z-axis
                currentZ--;
                targetPosition.z = startPosition.z + (currentZ - gridSize / 2) * cellDistance;
            }
        }

        // Handle jump input
        if (!isJumping && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            jumpTimer = 0f;
        }

        // Move toward target position (Z-axis only)
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(transform.position.x, transform.position.y, targetPosition.z),
            moveSpeed * Time.deltaTime
        );

        // Handle jumping
        if (isJumping)
        {
            if (jumpEnabled)
                Jump();
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