using DG.Tweening;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private int lives = 3;
    [SerializeField] private PhaseManager phaseManager;

    private int currentLives;
    private bool dead = false;

    private bool invulnerable = false;

    [SerializeField] private Renderer gridRenderer;
    private Material gridMaterial;
    [SerializeField] private Renderer mountainRenderer;
    private Material mountainMaterial;
    [SerializeField] private Renderer starRenderer;
    private Material starMaterial;

    [SerializeField] private GameObject[] livesUI;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = lives;

        gridMaterial = gridRenderer.sharedMaterial;
        mountainMaterial = mountainRenderer.sharedMaterial;
        starMaterial = starRenderer.sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleInvulnerable()
    {
        invulnerable = !invulnerable;
    }

    public void DealDamage(int damage)
    {

        if (dead)
        {
            return;
        }



        Sequence sequenceGrid = DOTween.Sequence();
        sequenceGrid.Append(gridMaterial.DOFloat(1f, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceGrid.Append(gridMaterial.DOFloat(0.1f, "_ColorIntensity", 0.2f).SetEase(Ease.InOutSine));

        Sequence sequenceMountain = DOTween.Sequence();
        sequenceMountain.Append(mountainMaterial.DOFloat(1f, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceMountain.Append(mountainMaterial.DOFloat(0.2f, "_ColorIntensity", 0.2f).SetEase(Ease.InOutSine));

        Sequence sequenceStar = DOTween.Sequence();
        sequenceStar.Append(starMaterial.DOFloat(1f, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceStar.Append(starMaterial.DOFloat(0.1f, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));

        CameraShake cameraShake = phaseManager.CurrentCamera.GetComponent<CameraShake>();

        if (invulnerable)
            return;
        
        if (cameraShake != null)
            cameraShake.ShakeCamera(2f, 0.1f);



        currentLives--;

        for (int i = 0; i < livesUI.Length; i++)
        {
            livesUI[i].SetActive(false);
        }

        for (int i = 0; i < currentLives - 1; i++)
        {
            livesUI[i].SetActive(true);
        }

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