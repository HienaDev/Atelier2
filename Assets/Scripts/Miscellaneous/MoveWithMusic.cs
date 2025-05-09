using DG.Tweening;
using UnityEngine;

public class MoveWithMusic : MonoBehaviour
{
    // Singleton instance
    public static MoveWithMusic Instance { get; private set; }

    [SerializeField] private float BPM;
    [SerializeField] private Material matGridVertical;

    private int colorIndex = 0;
    private float timeBetweenNotes;
    private float justBopped;
    [HideInInspector] public bool bop = true;
    private Sequence sequenceVertical;
    private Sequence sequenceColor;

    public float timeUntilBop = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeBetweenNotes = 1 / (BPM / 60);
        Debug.Log(timeBetweenNotes);
    }

    void Update()
    {
        if (bop)
            bop = false;

        timeUntilBop = timeBetweenNotes - (Time.timeSinceLevelLoad - justBopped);

        if (Time.timeSinceLevelLoad - justBopped > timeBetweenNotes)
        {
            bop = true;
        }

        if (bop)
        {
            sequenceVertical = DOTween.Sequence();
            sequenceVertical.Append(matGridVertical.DOFloat(0.8f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));
            sequenceVertical.Append(matGridVertical.DOFloat(0.9f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));

            sequenceColor = DOTween.Sequence();
            // Uncomment if needed
            // sequenceColor.Append(matGridVertical.DOFloat(0.2f, "_ColorChangeSpeed", 0.2f).SetEase(Ease.InOutSine));
            // sequenceColor.Append(matGridVertical.DOFloat(0.1f, "_ColorChangeSpeed", 0.2f).SetEase(Ease.InOutSine));

            justBopped = Time.timeSinceLevelLoad;
        }
    }
}
