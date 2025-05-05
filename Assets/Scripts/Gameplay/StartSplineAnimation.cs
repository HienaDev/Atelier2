using UnityEngine;
using UnityEngine.Splines;

public class StartSplineAnimation : MonoBehaviour
{
    private SplineAnimate splineAnimate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();

        splineAnimate.Play();
    }
}
