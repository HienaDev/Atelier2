using UnityEngine;
using DG.Tweening;

public class UpDownMovement : MonoBehaviour
{
    public enum Position { Bottom, Top }

    [Header("Movement Settings")]
    [SerializeField] private float moveUpDistance = 2f;
    [SerializeField] private float moveDuration = 0.33f;
    [SerializeField] private float pauseDuration = 1f;
    [SerializeField] private Position startPosition = Position.Bottom;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private float audioPitch = 1f;

    private Position currentPosition;
    private Vector3 originalPosition;
    private Vector3 topPosition;
    private Sequence movementSequence;

    void Start()
    {
        originalPosition = transform.position;
        topPosition = originalPosition + Vector3.up * moveUpDistance;
        currentPosition = startPosition;

        if (startPosition == Position.Top)
        {
            transform.position = topPosition;
        }

        CreateMovementSequence();
    }

    void CreateMovementSequence()
    {
        movementSequence = DOTween.Sequence();

        if (startPosition == Position.Bottom)
        {
            // Move up
            movementSequence.AppendCallback(() =>
            {
                PlayMovementSound();
            });
            movementSequence.Append(transform.DOMoveY(topPosition.y, moveDuration).SetEase(Ease.InOutSine));
            movementSequence.AppendInterval(pauseDuration);

            // Move down
            movementSequence.AppendCallback(() =>
            {
                PlayMovementSound();
            });
            movementSequence.Append(transform.DOMoveY(originalPosition.y, moveDuration).SetEase(Ease.InOutSine));
            movementSequence.AppendInterval(pauseDuration);
        }
        else
        {
            // Move down
            movementSequence.AppendCallback(() =>
            {
                PlayMovementSound();
            });
            movementSequence.Append(transform.DOMoveY(originalPosition.y, moveDuration).SetEase(Ease.InOutSine));
            movementSequence.AppendInterval(pauseDuration);

            // Move up
            movementSequence.AppendCallback(() =>
            {
                PlayMovementSound();
            });
            movementSequence.Append(transform.DOMoveY(topPosition.y, moveDuration).SetEase(Ease.InOutSine));
            movementSequence.AppendInterval(pauseDuration);
        }

        movementSequence.SetLoops(-1);
    }

    void PlayMovementSound()
    {
        if (audioClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(audioClip, audioVolume, audioPitch, true, 0f);
        }
    }

    void OnDestroy()
    {
        if (movementSequence != null)
            movementSequence.Kill();
    }

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
