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
    [SerializeField] private AudioSource musicSource; // Reference to the music AudioSource

    // The "bop" flag that will be true for one frame on each beat
    [HideInInspector]
    public bool bop = false;

    // Time until the next bop in seconds
    [HideInInspector]
    public float timeUntilBop = 0f;

    // Internal timing variables
    private float secondsPerBeat;
    private float nextBeatTime;
    private float previousSongTime;
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
        if (musicSource == null)
        {
            Debug.LogError("MoveWithMusic: Music source is not assigned!");
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                Debug.LogError("MoveWithMusic: No AudioSource found on this GameObject!");
            }
        }

        // Initialize the next beat time using the song's time
        nextBeatTime = 0f;
    }

    private void Update()
    {
        // If no music source, can't sync to music
        if (musicSource == null) return;

        // Reset bop at the beginning of each frame
        bop = false;

        // Check if BPM was changed
        if (secondsPerBeat != 60f / BPM)
        {
            UpdateBPMSettings();
        }

        // Get current song time
        float songTime = musicSource.time;

        // Check if we've reached the next beat time
        if (songTime >= nextBeatTime)
        {
            // Set bop to true for this frame
            if(Time.timeScale > 0)
                bop = true;

            sequenceVertical = DOTween.Sequence();
            sequenceVertical.Append(matGridVertical.DOFloat(0.8f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));
            sequenceVertical.Append(matGridVertical.DOFloat(0.9f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));

            // Calculate next beat time
            nextBeatTime += secondsPerBeat;

            // If we've fallen way behind (e.g., after a pause or seek), resync
            if (nextBeatTime < songTime - secondsPerBeat)
            {
                // Calculate the closest beat time based on current song position
                nextBeatTime = songTime + (secondsPerBeat - (songTime % secondsPerBeat));
                if (nextBeatTime <= songTime) nextBeatTime += secondsPerBeat;
            }
        }

        // Handle song looping
        if (previousSongTime > songTime && previousSongTime > 0.5f)
        {
            // Song has looped or been restarted
            nextBeatTime = songTime + (secondsPerBeat - (songTime % secondsPerBeat));
            if (nextBeatTime <= songTime) nextBeatTime += secondsPerBeat;
        }

        // Update the time until next bop
        timeUntilBop = nextBeatTime - songTime;
        previousSongTime = songTime;
    }

    private void UpdateBPMSettings()
    {
        secondsPerBeat = 60f / BPM;
    }

    // Convenience method to manually sync the beat timing
    public void SyncBeat()
    {
        if (musicSource != null)
        {
            // Calculate the closest beat time based on current song position
            float songTime = musicSource.time;
            nextBeatTime = songTime + (secondsPerBeat - (songTime % secondsPerBeat));
            if (nextBeatTime <= songTime) nextBeatTime += secondsPerBeat;
        }
    }

    // Convenience method to change the music source at runtime
    public void SetMusicSource(AudioSource newSource)
    {
        musicSource = newSource;
        SyncBeat();
    }
}