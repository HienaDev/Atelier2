using UnityEngine;
using DG.Tweening;

/// <summary>
/// Animates a UI element to pop in from scale 0, rise up, then pop out and deactivate itself.
/// Requires DOTween to be installed in your project.
/// </summary>
public class UIScalePopAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float popInDuration = 0.3f;
    [SerializeField] private float riseDuration = 1.5f;
    [SerializeField] private float riseDistance = 2f;
    [SerializeField] private float hoverDuration = 0.3f; // Time to wait at the top before popping out
    [SerializeField] private float popOutDuration = 0.2f;

    [Header("Animation Feel")]
    [SerializeField] private Ease popInEase = Ease.OutBack; // OutBack gives a nice overshoot
    [SerializeField] private Ease riseEase = Ease.InOutSine;
    [SerializeField] private Ease popOutEase = Ease.InBack;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Sequence animationSequence;

    private void Awake()
    {
        // Store the original scale and position
        originalScale = transform.localScale;
        originalPosition = transform.position;
    }

    private void OnEnable()
    {
        // Start at scale zero
        transform.localScale = Vector3.zero;

        // Reset position to starting point
        transform.position = originalPosition;

        // Create and start the animation sequence
        PlayPopAnimation();
    }

    private void OnDisable()
    {
        // Kill the animation if the object is disabled
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
    }

    /// <summary>
    /// Creates and plays the complete animation sequence
    /// </summary>
    public void PlayPopAnimation()
    {
        // Kill any existing animation
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        // Create a new sequence
        animationSequence = DOTween.Sequence();

        // 1. Pop in from scale 0 to original scale
        animationSequence.Append(transform.DOScale(originalScale, popInDuration).SetEase(Ease.InOutElastic));

        // 2. Rise upward
        animationSequence.Append(transform.DOMove(originalPosition + Vector3.up * riseDistance, riseDuration).SetEase(Ease.InOutElastic));

        // 3. Wait briefly at the top
        animationSequence.AppendInterval(hoverDuration);

        // 4. Pop out (scale back to zero)
        animationSequence.Append(transform.DOScale(Vector3.zero, popOutDuration).SetEase(Ease.InOutElastic));

        // 5. Deactivate when complete
        animationSequence.OnComplete(() => Destroy(gameObject));

        // Start the sequence
        animationSequence.Play();
    }

    /// <summary>
    /// Restarts the animation immediately
    /// </summary>
    public void RestartAnimation()
    {
        if (gameObject.activeSelf)
        {
            // Kill current animation
            if (animationSequence != null && animationSequence.IsActive())
            {
                animationSequence.Kill();
            }

            // Reset position
            transform.position = originalPosition;
            transform.localScale = Vector3.zero;

            // Start new animation
            PlayPopAnimation();
        }
        else
        {
            // Just activate the object - OnEnable will handle the animation
            gameObject.SetActive(true);
        }
    }
}