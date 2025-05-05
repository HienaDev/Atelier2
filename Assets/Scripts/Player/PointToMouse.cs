using UnityEngine;

public class PointToMouse : MonoBehaviour
{
    [SerializeField] private GameObject topHalf;
    [SerializeField] private float rotationSpeed = 10f;

    void Update()
    {
        // Get direction from object to the hit point, but only on the X-Z plane
        Vector3 direction = Mouse3D.GetMouseObjectPosition() - topHalf.transform.position;

        direction.y = 0f;
        
        if (direction != Vector3.zero)
        {
            // Create target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target
            topHalf.transform.rotation = Quaternion.Slerp(topHalf.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
