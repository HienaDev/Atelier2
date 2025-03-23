using DG.Tweening;
using UnityEngine;

public class DamageBoss : MonoBehaviour
{

    [SerializeField] private BossHealth health;

    [SerializeField] private Renderer[] partsRenderer;


    public void DealDamage(int damage)
    {
        health.DealDamage(damage);

        foreach (var renderer in partsRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(1f + damage * 0.1f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(0f, "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
        }
    }
}
