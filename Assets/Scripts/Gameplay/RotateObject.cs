using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationSpeed; // Speed of rotation around each axis

    private void Update()
    {
        // Calculate the rotation amount for this frame
        Vector3 rotationAmount = rotationSpeed * Time.deltaTime;

        // Apply the rotation to the object
        transform.Rotate(rotationAmount);
    }
}
