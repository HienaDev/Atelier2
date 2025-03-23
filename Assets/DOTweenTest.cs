using DG.Tweening;
using UnityEngine;

public class DOTweenTest : MonoBehaviour
{

    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float period = 0.1f;

    [SerializeField] private Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //transform.DOShakePosition(0.1f, 0.1f, 5, 50, false, true).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        transform.DOShakeRotation(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        transform.DOShakeScale(0.2f, 0.2f, 5, 50, false, ShakeRandomnessMode.Harmonic).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        
        transform.DOMove(target.position, 2).SetEase(Ease.InOutElastic, amplitude: amplitude, period: period);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
