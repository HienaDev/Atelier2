using DG.Tweening;
using UnityEngine;

public class DamageBoss : MonoBehaviour
{
    [SerializeField] private BossHealth health;
    [SerializeField] private Renderer[] partsRenderer;

    private bool damageable = false;

    private void OnEnable()
    {
        Transform[] bossParts = new Transform[partsRenderer.Length];
        for (int i = 0; i < partsRenderer.Length; i++)
        {
            bossParts[i] = partsRenderer[i].transform;
        }

    }

    public void ToggleDamageable(bool toggle)
    {
        damageable = toggle;
    }

    public void DealCritDamage(bool clearWeakpoint = false)
    {
        Transform[] bossParts = new Transform[partsRenderer.Length];

        for (int i = 0; i < partsRenderer.Length; i++)
        {
            bossParts[i] = partsRenderer[i].transform;
        }

        health.DealCritDamage(clearWeakpoints: clearWeakpoint);
        // Only deals damage if crit is false, we put 100 to scale up the displacement
        DealDamage(100, true);
    }

    public void DealDamage(int damage, bool crit = false)
    {
        if (!damageable)
            return;

        if(!crit)
            health.DealDamage(damage);

        foreach (var renderer in partsRenderer)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(5f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0.15f + damage * 0.5f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(1f, "_ColorBrightness", 0.05f).SetEase(Ease.InOutSine));
            sequence.Join(renderer.material.DOFloat(0f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));

        }
    }

    public void ChangePhase()
    {
        health.ChangePhase();
    }
}
