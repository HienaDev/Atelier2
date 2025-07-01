using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Destructable : MonoBehaviour
{
    [SerializeField] private float intensitySteps = 0.1f;
    [SerializeField] private int lives = 20;
    [SerializeField] private bool isTriggerWeakPoint = false;
    [SerializeField] private float extrusionIntensitySteps = 0.1f;
    [SerializeField] private float minimumExtrusion = 0.5f;
    [SerializeField] private Collider col;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private int numberOfParticles = 5;
    [SerializeField] private float timeAlive = 20f;
    [SerializeField] private bool disableLifetime = false;

    private MeshRenderer rend;  // Assign the renderer in the Inspector
    private Material mat;
    private Sequence sequence;
    private Color baseColor;
    private float currentIntensity;
    private int currentLives;
    private bool dying;
    private float justSpawned;

    private Transform bossSpawn;

    [SerializeField] private float timeToExplode = 0.1f;
    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] public float period = 0.1f;
    [SerializeField] private float flyingDuration = 2f;

    private float originalPitch;

    public UnityEvent onDeath;
    public UnityEvent onLifetime;

    [Header("Pop Settings")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;

    private Vector3 originalScale;

    [Header("Audio Settings")]
    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private float audioPitch = 1f;
    [SerializeField] private bool enablePitchVariation = false; // New option, starts false
    [SerializeField] private float pitchVariationAmount = 0.2f; // How much the pitch can vary

    void Start()
    {
        originalPitch = audioPitch;
        rend = GetComponent<MeshRenderer>();

        if (rend == null)
        {
            rend = GetComponentInChildren<MeshRenderer>();
        }

        mat = rend.material;

        currentLives = lives;
        baseColor = mat.GetColor("_Color");
        currentIntensity = baseColor.maxColorComponent;
        justSpawned = Time.time;
    }

    private void HandleHit()
    {
        currentLives--;
        if (currentLives <= 0 && !dying)
        {
            BlowUp();
        }
        else
        {
            currentIntensity += intensitySteps;

            mat.SetColor("_Color", baseColor * currentIntensity);
            sequence?.Kill();
            transform.DOShakePosition(0.1f, 0.3f, 5, 90, false, true);
            sequence = DOTween.Sequence();
            sequence.Append(mat.DOFloat(minimumExtrusion + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(mat.DOFloat(0f + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));

            // Calculate pitch based on variation setting
            float finalPitch;
            if (enablePitchVariation)
            {
                // Add random pitch variation
                float randomVariation = Random.Range(-pitchVariationAmount, pitchVariationAmount);
                finalPitch = originalPitch + (lives / currentLives) + randomVariation;
            }
            else
            {
                // Use original pitch calculation
                finalPitch = originalPitch + (lives / currentLives);
            }

            // Use AudioManager for hit sounds
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(audioClip, audioVolume, finalPitch, true, 0f);
            }
        }
    }

    /// <summary>
    /// Method to be called by UnityEvent with specific AudioSource parameter
    /// </summary>
    public void PlayDeathSoundWithVariation(AudioSource specificAudioSource)
    {
        if (audioClip == null || specificAudioSource == null) return;

        float deathPitch = originalPitch;
        if (enablePitchVariation)
        {
            float randomVariation = Random.Range(-pitchVariationAmount, pitchVariationAmount);
            deathPitch += randomVariation;
        }

        specificAudioSource.clip = audioClip;
        specificAudioSource.volume = audioVolume;
        specificAudioSource.pitch = deathPitch;
        specificAudioSource.Play();
    }

    public void BlowUp()
    {
        Debug.Log("Blowing up " + gameObject.name);
        col.enabled = false;
        dying = true;
        sequence?.Kill();
        transform.DOKill();
        transform.DOShakeRotation(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        transform.DOShakeScale(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        sequence = DOTween.Sequence();
        sequence.AppendInterval(timeToExplode / 2).OnComplete(() =>
        {
            if (particles != null)
                particles.Play();
            mat.DOFloat(160f, "_PulseRatio", timeToExplode / 2).SetEase(Ease.InSine).OnComplete(() => { rend.enabled = false; } );
            transform.DOKill();
            onDeath.Invoke();
            Destroy(gameObject, 1f);

        }); ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            HandleHit();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            HandleHit();
        }
    }
}