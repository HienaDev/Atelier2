using UnityEngine;

public class ProceduralChainMovement : MonoBehaviour
{
    private float moveSpeed = 5f; // Movement speed
    private float radius = 1f; // Radius of the chain
    private Transform nextChain;
    public Vector3 faceAxis = Vector3.forward; // Axis to face towards the next chain

    public void Initialize(float movSpeed, float rad, Transform nextC)
    {
        moveSpeed = movSpeed;
        radius = rad;
        nextChain = nextC;
    }

    void Update()
    {
        if (nextChain)
        {
            if (Vector3.Distance(transform.position, nextChain.position) > radius)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextChain.position, moveSpeed * Time.deltaTime);
            }

            // Calculate the direction to the next chain
            Vector3 directionToNextChain = (nextChain.position - transform.position).normalized;

            // Create a rotation that looks in the direction of the next chain
            Quaternion targetRotation = Quaternion.LookRotation(directionToNextChain);

            // Adjust the rotation based on the chosen axis
            transform.rotation = Quaternion.FromToRotation(faceAxis, directionToNextChain) * targetRotation;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}