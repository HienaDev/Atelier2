using UnityEngine;

public class LookAwayFromCamera : MonoBehaviour
{
    private void OnEnable()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calculate the direction away from the camera
            Vector3 lookAwayPoint = 2 * transform.position - mainCamera.transform.position;
            transform.LookAt(lookAwayPoint);
        }
        else
        {
            Debug.LogWarning("Main Camera not found in the scene.");
        }
    }
}
