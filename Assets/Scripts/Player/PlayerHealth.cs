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
    private float gracePeriodTimer = 0f;
    private bool inGracePeriod = false;
    [SerializeField] private float gracePeriodDuration = 1f;
    [SerializeField] private float blinkInterval = 0.1f; // Time between blinks
    private float blinkTimer = 0f;
    private bool isBlinkingOn = false;
    private Color originalColor; // Store original color

    [SerializeField] private Renderer gridRenderer;
    private Material gridMaterial;
    [SerializeField] private float gridMaterialIntensityIncrease = 0.5f;
    private float gridMaterialDefaultIntensity;
    [SerializeField] private Renderer mountainRenderer;
    private Material mountainMaterial;
    [SerializeField] private float mountainMaterialIntensityIncrease = 0.5f;
    private float mountainMaterialDefaultIntensity;
    [SerializeField] private Renderer starRenderer;
    private Material starMaterial;
    [SerializeField] private float starMaterialIntensityIncrease = 0.5f;
    private float starMaterialDefaultIntensity;

    [SerializeField] private GameObject[] livesUI;

    private Sequence sequence;

    [SerializeField] private AudioClip hurtSound;

    [SerializeField] private GameObject deathScreen;

    void Start()
    {
        currentLives = lives;

        gridMaterial = gridRenderer.sharedMaterial;
        gridMaterialDefaultIntensity = 0.01f;// gridMaterial.GetFloat("_ColorIntensity");
        mountainMaterial = mountainRenderer.sharedMaterial;
        mountainMaterialDefaultIntensity = 0.2f;// mountainMaterial.GetFloat("_ColorIntensity");
        starMaterial = starRenderer.sharedMaterial;
        starMaterialDefaultIntensity = 0.5f;// starMaterial.GetFloat("_ColorIntensity");

        // Store original color from the first renderer (assuming all have same color)
        if (renderers.Length > 0)
        {
            originalColor = renderers[0].material.GetColor("_Color");
        }
    }

    void Update()
    {
        if (inGracePeriod)
        {
            gracePeriodTimer -= Time.deltaTime;
            blinkTimer -= Time.deltaTime;

            if (blinkTimer <= 0f)
            {
                isBlinkingOn = !isBlinkingOn;
                blinkTimer = blinkInterval;

                // Toggle blink effect
                Color targetColor = isBlinkingOn ? Color.white : originalColor;
                foreach (var renderer in renderers)
                {
                    renderer.material.SetColor("_Color", targetColor);
                }
            }

            if (gracePeriodTimer <= 0f)
            {
                inGracePeriod = false;
                // Ensure we return to original color when grace period ends
                foreach (var renderer in renderers)
                {
                    renderer.material.SetColor("_Color", originalColor);
                }
            }
        }
    }

    public void ToggleInvulnerable()
    {
        invulnerable = !invulnerable;
    }

    public void StartGracePeriod()
    {
        inGracePeriod = true;
        gracePeriodTimer = gracePeriodDuration;
        blinkTimer = blinkInterval;
        isBlinkingOn = false; // Start with original color
    }

    public void DealDamage(int damage)
    {
        if (dead || invulnerable || inGracePeriod)
        {
            return;
        }

        AudioManager.Instance.PlaySound(hurtSound, volume:0.6f, pitch: 0.3f, true);

        Sequence sequenceGrid = DOTween.Sequence();
        sequenceGrid.Append(gridMaterial.DOFloat(gridMaterialDefaultIntensity + gridMaterialIntensityIncrease, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceGrid.Append(gridMaterial.DOFloat(gridMaterialDefaultIntensity, "_ColorIntensity", 0.2f).SetEase(Ease.InOutSine));

        Sequence sequenceMountain = DOTween.Sequence();
        sequenceMountain.Append(mountainMaterial.DOFloat(mountainMaterialDefaultIntensity + mountainMaterialIntensityIncrease, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceMountain.Append(mountainMaterial.DOFloat(mountainMaterialDefaultIntensity, "_ColorIntensity", 0.2f).SetEase(Ease.InOutSine));

        Sequence sequenceStar = DOTween.Sequence();
        sequenceStar.Append(starMaterial.DOFloat(starMaterialDefaultIntensity + starMaterialIntensityIncrease, "_ColorIntensity", 0.05f).SetEase(Ease.InOutSine));
        sequenceStar.Append(starMaterial.DOFloat(starMaterialDefaultIntensity, "_ColorIntensity", 0.2f).SetEase(Ease.InOutSine));

        CameraShake cameraShake = phaseManager.CurrentCamera.GetComponent<CameraShake>();

        if (cameraShake != null)
            cameraShake.ShakeCamera(2f, 0.1f);

        currentLives--;

        for (int i = 0; i < livesUI.Length; i++)
        {
            livesUI[i].SetActive(false);
        }

        for (int i = 0; i < currentLives; i++)
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
            sequence = DOTween.Sequence();
            sequence.Append(renderer.material.DOFloat(3f, "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(renderer.material.DOFloat(0f, "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
        }

        StartGracePeriod();
    }

    public void Death()
    {
        sequence.Kill();
        foreach (var renderer in renderers)
        {
            renderer.material.DOFloat(80f, "_PulseRatio", 0.25f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                deathScreen.SetActive(true);
            }) ;
        }
    }
}