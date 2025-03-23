using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveWithMusic : MonoBehaviour
{
    [SerializeField] private float BPM;


    private int colorIndex = 0;


    private float timeBetweenNotes;

    private float justBopped;
    private bool bop = true;


    private Sequence sequenceVertical;
    private Sequence sequenceColor;

    [SerializeField] private Material matGridVertical;


    // Start is called before the first frame update
    void Start()
    {
        timeBetweenNotes = 1 / (BPM / 60);
        Debug.Log(timeBetweenNotes);


    }

    // Update is called once per frame
    void Update()
    {


        if (Time.timeSinceLevelLoad - justBopped > timeBetweenNotes)
        {
            bop = true;
        }

        if (bop == true)
        {
 

            sequenceVertical = DOTween.Sequence();
            sequenceVertical.Append(matGridVertical.DOFloat(0.8f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));
            sequenceVertical.Append(matGridVertical.DOFloat(0.9f, "_LineWidth", 0.1f).SetEase(Ease.InOutSine));

            sequenceColor = DOTween.Sequence();
            //sequenceColor.Append(matGridVertical.DOFloat(0.2f, "_ColorChangeSpeed", 0.2f).SetEase(Ease.InOutSine));
            //sequenceColor.Append(matGridVertical.DOFloat(0.1f, "_ColorChangeSpeed", 0.2f).SetEase(Ease.InOutSine));


            bop = false;
            justBopped = Time.timeSinceLevelLoad;
        }


    }


}
