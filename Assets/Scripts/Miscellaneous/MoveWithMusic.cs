using DG.Tweening;
using UnityEngine;

public class MoveWithMusic : MonoBehaviour
{
    // Singleton instance
    private static MoveWithMusic _instance;
    public static MoveWithMusic Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindAnyObjectByType<MoveWithMusic>();

                // If no instance exists, create a new one
                if (_instance == null)
                {
                    GameObject obj = new GameObject("MoveWithMusic");
                    _instance = obj.AddComponent<MoveWithMusic>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // BPM setting (beats per minute)
    [Range(1, 300)]
    public float BPM = 120f;

    [SerializeField] private Material matGridVertical;

    // The "bop" flag that will be true for one frame on each beat
    [HideInInspector]
    public bool bop = false;

    // Time until the next bop in seconds
    [HideInInspector]
    public float timeUntilBop = 0f;

    // Internal timing variables
    private float secondsPerBeat;
    private float nextBeatTime;
    private float previousFrameTime;

    private Sequence sequenceVertical;

    private void Awake()
    {
        // Singleton pattern enforcement
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize timing variables
        UpdateBPMSettings();
    }

    private void Start()
    {
        nextBeatTime = Time.time;
    }

    private void Update()
    {
        // Reset bop at the beginning of each frame
        bop = false;

        // Check if BPM was changed
        if (secondsPerBeat != 60f / BPM)
        {
            UpdateBPMSettings();
        }

        // Check if we've reached the next beat time
        if (Time.time >= nextBeatTime)
        {
            // Set bop to true for this frame
            bop = true;

            sequenceVertical = DOTween.Sequence();
            sequenceVertical.Append(matGridVertical.DOFloat(0.8f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));
            sequenceVertical.Append(matGridVertical.DOFloat(0.9f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));

            // Calculate next beat time
            nextBeatTime += secondsPerBeat;

            // If we've fallen way behind (e.g., after a pause), resync
            if (nextBeatTime < Time.time - secondsPerBeat)
            {
                nextBeatTime = Time.time;
            }
        }

        // Update the time until next bop
        timeUntilBop = nextBeatTime - Time.time;

        previousFrameTime = Time.time;
    }

    private void UpdateBPMSettings()
    {
        secondsPerBeat = 60f / BPM;
    }

    // Convenience method to manually sync the beat timing
    public void SyncBeat()
    {
        nextBeatTime = Time.time;
    }
}


