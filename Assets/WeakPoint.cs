using DG.Tweening;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using static UnityEngine.ParticleSystem;

public class WeakPoint : MonoBehaviour
{

    private Renderer rend;  // Assign the renderer in the Inspector
    private Material mat;
    private Sequence sequence;

    private Color baseColor;
    private float currentIntensity;

    [SerializeField] private float intensitySteps = 0.1f;
    [SerializeField] private int lives = 20;
    private int currentLives;

    [SerializeField] private float extrusionIntensitySteps = 0.1f;

    private bool dying = false;

    [SerializeField] private Collider col;

    [SerializeField] private ParticleSystem particles;
    [SerializeField] private int numberOfParticles = 5;

    public UnityEvent onDeath;

    [SerializeField] private float timeAlive = 20f;
    private float justSpawned;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>(); // Get the renderer component
        mat = rend.material; // Get the material instance

        currentLives = lives;

        //transform.DOMove(transform.position + new Vector3(0, 0, -10), 2).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        //transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

        baseColor = mat.GetColor("_Color");

        currentIntensity = baseColor.maxColorComponent; // Get the highest RGB value


        justSpawned = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - justSpawned > timeAlive)
        {
            Destroy(gameObject);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        currentLives -= 1;
        if (currentLives <= 0 && !dying)
        {
            col.enabled = false;
            dying = true;
            sequence.Kill();
            transform.DOKill();
            transform.DOShakePosition(0.1f, 0.1f, 5, 50, false, true).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            mat.DOFloat(80f, "_PulseRatio", 0.25f).SetEase(Ease.InExpo).OnComplete(() => {
                transform.DOKill();
                onDeath.Invoke();
                Destroy(gameObject, 0.1f);
            });

        }
        else if (currentLives > 0)
        {
            currentIntensity += intensitySteps;
            particles.Emit(numberOfParticles);
            mat.SetColor("_Color", baseColor * currentIntensity);
            if (sequence != null) sequence.Kill();
            transform.DOShakePosition(0.1f, 0.3f, 5, 90, false, true);
            sequence = DOTween.Sequence();
            sequence.Append(mat.DOFloat(1f + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.05f).SetEase(Ease.InOutSine));
            sequence.Append(mat.DOFloat(-1f + ((lives - currentLives) * extrusionIntensitySteps), "_PulseRatio", 0.5f).SetEase(Ease.InOutSine));
        }
    }
}
