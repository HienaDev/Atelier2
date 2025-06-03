using UnityEngine;
using DG.Tweening;

public class LeftRightMovement : MonoBehaviour
{
    public enum Direction { Left, Right }

    [Header("Movement Settings")]
    [SerializeField] float gridSpacing = 1.5f;             // Distance between grid positions
    [SerializeField] float moveDuration = 0.33f;         // How long each movement takes
    [SerializeField] float pauseDuration = 1f;           // How long to pause at each position
    [SerializeField] Direction startDirection = Direction.Left; // Which direction to move first

    private Vector3 centerPosition;   // Middle position (where object starts)
    private Vector3 leftPosition;     // Left grid position
    private Vector3 rightPosition;    // Right grid position
    private Sequence movementSequence;

    void Start()
    {
        // Object starts in center, calculate left and right positions
        centerPosition = transform.position;
        leftPosition = centerPosition + Vector3.back * gridSpacing;
        rightPosition = centerPosition + Vector3.forward * gridSpacing;

        CreateMovementSequence();
    }

    void CreateMovementSequence()
    {
        // Create a new sequence
        movementSequence = DOTween.Sequence();

        // If starting by moving left
        if (startDirection == Direction.Left)
        {
            // Move to left position
            movementSequence.Append(transform.DOMoveZ(leftPosition.z, moveDuration).SetEase(Ease.InOutSine));
            // Pause at left
            movementSequence.AppendInterval(pauseDuration);
            // Move back to center
            movementSequence.Append(transform.DOMoveZ(centerPosition.z, moveDuration).SetEase(Ease.InOutSine));
            // Pause at center
            movementSequence.AppendInterval(pauseDuration);
        }
        else // If starting by moving right
        {
            // Move to right position
            movementSequence.Append(transform.DOMoveZ(rightPosition.z, moveDuration).SetEase(Ease.InOutSine));
            // Pause at right
            movementSequence.AppendInterval(pauseDuration);
            // Move back to center
            movementSequence.Append(transform.DOMoveZ(centerPosition.z, moveDuration).SetEase(Ease.InOutSine));
            // Pause at center
            movementSequence.AppendInterval(pauseDuration);
        }

        // Set the sequence to loop indefinitely
        movementSequence.SetLoops(-1);
    }

    void OnDestroy()
    {
        // Clean up the tween when the object is destroyed
        if (movementSequence != null)
            movementSequence.Kill();
    }

    // Optional: Draw gizmos to visualize the 3-position grid in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 center = transform.position;
            Vector3 left = center + Vector3.back * gridSpacing;
            Vector3 right = center + Vector3.forward * gridSpacing;

            // Draw grid positions
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.1f);  // Center (starting position)

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(left, 0.1f);   // Left position
            Gizmos.DrawSphere(right, 0.1f);  // Right position

            // Draw lines connecting positions
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(left, center);
            Gizmos.DrawLine(center, right);
        }
    }
}