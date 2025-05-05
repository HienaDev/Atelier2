using UnityEngine;
using DG.Tweening;

public class ScaleWithMusic : MonoBehaviour
{
    private Vector3 originalScale;
    private Tween currentTween;

    [SerializeField] private Vector3 scaleAmount = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private float duration = 0.4f;

    [SerializeField] private bool moveWithoutCall = true;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (MoveWithMusic.Instance.bop && moveWithoutCall)
        {

            Pulse();
        }
    }

    public void Pulse()
    {
        PulseTween(scaleAmount, duration);
    }

    public void PulseTween(Vector3 scaleAmount, float duration)
    {
        currentTween?.Kill();

        Vector3 targetScale = new Vector3(
            originalScale.x + scaleAmount.x,
            originalScale.y + scaleAmount.y,
            originalScale.z + scaleAmount.z
        );

        float halfDuration = duration / 2f;

        currentTween = transform.DOScale(targetScale, halfDuration)
            .SetEase(Ease.InBounce)
            .OnComplete(() =>
                transform.DOScale(originalScale, halfDuration)
                         .SetEase(Ease.OutBounce));
    }
}
