using DG.Tweening;
using UnityEngine;

public class DamageBoss : MonoBehaviour
{

    [SerializeField] private BossHealth health;

    [SerializeField] private Renderer[] partsRenderer;

    

    private bool damageable = false;

    public void ToggleDamageable(bool toggle)
    {
        damageable = toggle;
    }

    public void DealDamage(int damage)
    {
        if (!damageable)
            return;

        health.DealDamage(damage);

        foreach (var renderer in partsRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(1f + damage * 0.5f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(0f, "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
        }
    }

    public void ChangePhase()
    {
        health.ChangePhase();
    }
}
