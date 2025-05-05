using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Destroyable : MonoBehaviour
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

    [SerializeField] private Renderer rend;  // Assign the renderer in the Inspector
    private Material mat;
    private Sequence sequence;
    private Color baseColor;
    private float currentIntensity;
    private int currentLives;
    private bool dying;
    private float justSpawned;

    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float period = 0.1f;
    [SerializeField] private float flyingDuration = 2f;

    public UnityEvent onDeath;

    void Start()
    {
        mat = rend.material;
        currentLives = lives;
        baseColor = mat.GetColor("_Color");
        currentIntensity = baseColor.maxColorComponent;
        justSpawned = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        if (other.CompareTag("PlayerProjectile"))
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
            transform.DOShakeRotation(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            transform.DOShakeScale(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

            mat.DOFloat(160f, "_PulseRatio", 0.1f).SetEase(Ease.InSine).OnComplete(() =>
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
}
