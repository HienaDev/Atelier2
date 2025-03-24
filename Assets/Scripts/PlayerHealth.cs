using DG.Tweening;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private int lives = 3;
    [SerializeField] private PhaseManager phaseManager;

    private int currentLives;
    private bool dead = false;

    void Start()
    {
        currentLives = lives;
    }

    public void DealDamage(int damage)
    {
        CameraShake cameraShake = phaseManager.CurrentCamera.GetComponent<CameraShake>();
        if (cameraShake != null)
            cameraShake.ShakeCamera(2f, 0.1f);

        if (dead)
        {
            return;
        }

        currentLives--;

        if (currentLives == 0)
        {
            dead = true;
            Death();
            return;
        }

        foreach (var renderer in renderers)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(3f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(0f, "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
        }
    }

    public void Death()
    {
        foreach (var renderer in renderers)
        {
            renderer.material.DOFloat(80f, "_PulseRatio", 0.25f).SetEase(Ease.InExpo);
        }
    }
}