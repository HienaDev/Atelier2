
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] private float intensitySteps = 0.1f;
    [SerializeField] private int lives = 20;
    [SerializeField] private bool isTriggerWeakPoint = false;
    [SerializeField] private float extrusionIntensitySteps = 0.1f;
    [SerializeField] private Collider col;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private int numberOfParticles = 5;
    [SerializeField] private float timeAlive = 20f;
    [SerializeField] private bool disableLifetime = false;

    private Renderer rend;  // Assign the renderer in the Inspector
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

    [SerializeField] private GameObject critTextUI;

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

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        currentLives = lives;
        baseColor = mat.GetColor("_Color");
        currentIntensity = baseColor.maxColorComponent;
        justSpawned = Time.time;
    }

    void Update()
    {
        if  (!disableLifetime && Time.time - justSpawned > timeAlive && !dying)
        {
            dying = true;
            col.enabled = false;
            BlowDown();
        }
    }

    public void SetTarget(Transform target)
    {
        bossSpawn = target;
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
            particles.Emit(numberOfParticles);
            mat.SetColor("_Color", baseColor * currentIntensity);
            sequence?.Kill();
            transform.DOShakePosition(0.1f, 0.3f, 5, 90, false, true);
            sequence = DOTween.Sequence();
            sequence.Append(mat.DOFloat(2f + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(mat.DOFloat(0f + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
            
            if(audioSource != null)
            {
                audioSource.pitch = 1f + (lives / currentLives);
                audioSource.Play();
            }
        }
    }


    public void BlowUp(bool invokeDeath = true)
    {
        if(col != null)
            col.enabled = false;

        dying = true;
        sequence?.Kill();

        Debug.Log("Blowing up weakpoint");  

        transform.DOKill();
        transform.DOShakeRotation(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        transform.DOShakeScale(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

        transform.DOMove(transform.position + new Vector3(0f, invokeDeath? 5f : 0f, 0f), 0.2f).SetEase(Ease.InOutSine).OnComplete(() => {
            Vector3 targetPosition;

            if (bossSpawn != null && invokeDeath)
            {
                targetPosition = bossSpawn.position;
            }
            else
            {
                targetPosition = transform.position;
            }

            transform.DOMove(targetPosition, flyingDuration).SetEase(Ease.InElastic, amplitude: amplitude, period: period).OnComplete(() =>
            {
                mat.DOFloat(160f, "_PulseRatio", timeToExplode).SetEase(Ease.InSine);
                transform.DOKill();
                if(invokeDeath)
                {
                    onDeath.Invoke();
                    GameObject critTextUIClone = Instantiate(critTextUI, targetPosition, Quaternion.identity);
                }
                    
                Destroy(gameObject, timeToExplode);

            });
        });
    }

    public void BlowDown()
    {
        col.enabled = true;
        dying = true;
        sequence?.Kill();
        transform.DOKill();

        Sequence deflateSequence = DOTween.Sequence();

        // Optional: small shake to signal it's about to "blow down"
        deflateSequence.Append(
            transform.DOShakeScale(0.2f, 0.1f, 4, 20, false, ShakeRandomnessMode.Harmonic)
                .SetEase(Ease.InOutSine)
        );

        // Deflate the visual pulse via shader parameter (from current to 0)
        deflateSequence.Append(
            mat.DOFloat(0f, "_PulseRatio", 0.6f)
                .SetEase(Ease.InOutSine)
        );

        // Simultaneous soft scaling down and rotation for visual effect
        deflateSequence.Join(
            transform.DOScale(Vector3.one * 0.2f, 0.6f)
                .SetEase(Ease.InBack)
        );

        deflateSequence.Join(
            transform.DORotate(new Vector3(0f, 0f, 30f), 0.6f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuad)
        );

        deflateSequence.AppendCallback(() =>
        {
            onLifetime.Invoke();
            Destroy(gameObject, 0.3f);
        });
    }
    
    public void DisableLifetime()
    {
        disableLifetime = true;
    }
}