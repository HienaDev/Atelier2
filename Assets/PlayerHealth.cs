using DG.Tweening;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private Renderer[] renderers;

    [SerializeField] private int lives = 3;
    private int currentLives;

    private bool dead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = lives;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(int damage)
    {
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
