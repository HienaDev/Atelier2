using UnityEngine;

public class ArmRecoil : MonoBehaviour
{
    public float recoilDistance = 0.2f; // Fixed recoil movement
    public float recoilSpeed = 10f; // Speed of recoil movement
    public float returnSpeed = 5f; // Speed of returning to rest

    private Vector3 restPosition;
    private float recoilProgress = 0f; // Tracks how far back the arm is

    void Start()
    {
        restPosition = transform.localPosition; // Store the original position
    }

    void Update()
    {
        

        // Smoothly return to rest position when not shooting
        recoilProgress = Mathf.MoveTowards(recoilProgress, 0f, returnSpeed * Time.deltaTime);
        transform.localPosition = restPosition + new Vector3(0, 0, -recoilProgress);
    }

    public void ApplyRecoil()
    {
        // Ensure the new recoil never exceeds the max distance from restPosition
        recoilProgress = Mathf.Min(recoilProgress + recoilDistance, recoilDistance);
    }
}
