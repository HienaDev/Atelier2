
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

    public UnityEvent onDeath;

    private void Awake()
    {
        // Force trigger settings if desired
        col.isTrigger = isTriggerWeakPoint;
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

    // Update is called once per frame
    void Update()
    {
        if (!disableLifetime && Time.time - justSpawned > timeAlive && !dying)
        {
            dying = true;
            col.enabled = false;
            onDeath.Invoke();
            Destroy(gameObject);
        }
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
            col.enabled = false;
            dying = true;
            sequence?.Kill();
            transform.DOKill();
            transform.DOShakePosition(0.1f, 0.1f, 5, 50, false, true)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            mat.DOFloat(80f, "_PulseRatio", 0.25f)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    transform.DOKill();
                    onDeath.Invoke();
                    Destroy(gameObject, 0.1f);
                });
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
        }
    }
    
    public void DisableLifetime()
    {
        disableLifetime = true;
    }
}