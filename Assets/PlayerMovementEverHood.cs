using UnityEngine;

public class PlayerMovementEverHood : MonoBehaviour
{
    //[Header("Movement Settings")]
    //[SerializeField] private float moveSpeed = 5f;
    //[SerializeField] private int gridSize = 5;

    //[Header("Input Settings")]
    //[SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    //[SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    //private int currentPosition = 0;
    //private bool isMoving = false;
    //private Vector3 targetPosition;
    //private int maxPosition;

    //void Start()
    //{
    //    // Calculate the maximum position based on grid size
    //    // For a grid size of 5, we can move 2 units left and 2 units right from center
    //    maxPosition = gridSize / 2;

    //    // Set the initial position to the center of the grid
    //    transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    //    targetPosition = transform.position;
    //}

    //void Update()
    //{
    //    HandleInput();
    //    HandleMovement();
    //}

    //private void HandleInput()
    //{
    //    // Only process input if we're not already moving
    //    if (isMoving)
    //        return;

    //    // Move left
    //    if (Input.GetKeyDown(leftKey) && currentPosition > -maxPosition)
    //    {
    //        currentPosition--;
    //        targetPosition = new Vector3(transform.position.x, transform.position.y, currentPosition);
    //        isMoving = true;
    //    }
    //    // Move right
    //    else if (Input.GetKeyDown(rightKey) && currentPosition < maxPosition)
    //    {
    //        currentPosition++;
    //        targetPosition = new Vector3(transform.position.x, transform.position.y, currentPosition);
    //        //targetPosition = new Vector3(currentPosition, transform.position.y, transform.position.z);
    //        isMoving = true;
    //    }
    //}

    //private void HandleMovement()
    //{
    //    if (isMoving)
    //    {
    //        // Move towards the target position
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

    //        // Check if we've reached the target position
    //        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
    //        {
    //            transform.position = targetPosition;
    //            isMoving = false;
    //        }
    //    }
    //}

    //// Get the current grid position
    //public int GetCurrentPosition()
    //{
    //    return currentPosition;
    //}

    //// Check if the player is currently moving
    //public bool IsMoving()
    //{
    //    return isMoving;
    //}
}