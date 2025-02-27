using UnityEngine;

public class Spike : MonoBehaviour
{
    public float moveDistance = 5f; // Distance to move up
    public float duration = 3f;     // Duration in seconds to complete the movement

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;

    private bool comingUp = false;


    public void Initialize(float distance, float durationTime)
    {
        moveDistance = distance;
        duration = durationTime;

        // Store the initial position of the object
        startPosition = transform.position;

        // Calculate the target position based on the moveDistance
        targetPosition = startPosition + Vector3.up * moveDistance;

        comingUp = true;
    }

    void Update()
    {
        if (!comingUp)
            return;

        if (elapsedTime < duration)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Calculate the fraction of the duration that has passed
            float t = elapsedTime / duration;

            // Interpolate the position between start and target using Lerp
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
        }
        else
        {
            comingUp = false;
            // Ensure the object is exactly at the target position
            transform.position = targetPosition;
            Debug.Log("Object has reached the target position.");
        }


    }
}
