using UnityEngine;
using DG.Tweening;

public class OnEnablePop : MonoBehaviour
{
    [Header("Pop Settings")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = Vector3.zero;

        transform.DOScale(originalScale, duration)
                 .SetEase(ease);
    }
}
