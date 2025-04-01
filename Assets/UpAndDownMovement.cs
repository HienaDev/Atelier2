using UnityEngine;
using DG.Tweening;

public class UpDownMovement : MonoBehaviour
{
    public enum StartPosition { Bottom, Top }

    [Header("Movement Settings")]
    public float moveUpDistance = 2f;       // How far up the object moves
    public float moveDuration = 0.5f;      // How long each movement takes
    public float pauseDuration = 1f;       // How long to pause at each position
    public StartPosition startPosition = StartPosition.Bottom; // Where to start the movement

    private Vector3 originalPosition;
    private Vector3 topPosition;
    private Sequence movementSequence;

    void Start()
    {
        originalPosition = transform.position;
        topPosition = originalPosition + Vector3.up * moveUpDistance;

        // Set initial position based on startPosition choice
        if (startPosition == StartPosition.Top)
        {
            transform.position = topPosition;
        }

        CreateMovementSequence();
    }

    void CreateMovementSequence()
    {
        // Create a new sequence
        movementSequence = DOTween.Sequence();

        // If starting at bottom (default)
        if (startPosition == StartPosition.Bottom)
        {
            // Move up quickly
            movementSequence.Append(transform.DOMoveY(topPosition.y, moveDuration).SetEase(Ease.InOutSine));
            // Pause at the top
            movementSequence.AppendInterval(pauseDuration);
            // Move down quickly
            movementSequence.Append(transform.DOMoveY(originalPosition.y, moveDuration).SetEase(Ease.InOutSine));
            // Pause at the bottom
            movementSequence.AppendInterval(pauseDuration);
        }
        else // If starting at top
        {
            // Move down quickly
            movementSequence.Append(transform.DOMoveY(originalPosition.y, moveDuration).SetEase(Ease.InOutSine));
            // Pause at the bottom
            movementSequence.AppendInterval(pauseDuration);
            // Move up quickly
            movementSequence.Append(transform.DOMoveY(topPosition.y, moveDuration).SetEase(Ease.InOutSine));
            // Pause at the top
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

    // Optional: Draw gizmos to visualize the movement range in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * moveUpDistance);
            Gizmos.DrawSphere(transform.position + Vector3.up * moveUpDistance, 0.1f);
        }
    }
}