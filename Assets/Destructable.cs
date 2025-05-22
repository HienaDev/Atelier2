using DG.Tweening;
using System.Net.NetworkInformation;
using UnityEditor.ShaderGraph.Internal;
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

    public UnityEvent onDeath;
    public UnityEvent onLifetime;

    [Header("Pop Settings")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutBack;

    private Vector3 originalScale;

    [SerializeField] private AudioSource audioSource;

    void Start()
    {

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
    // Update is called once per frame
    void Update()
    {
        
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

            if (audioSource != null)
            {
                audioSource.pitch = 1f + (lives / currentLives);
                audioSource.Play();
            }
        }
    }

    public void BlowUp()
    {
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
