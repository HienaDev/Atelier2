using UnityEngine;

public class Spike : MonoBehaviour
{
    public float moveDistance = 5f; // Distance to move up
    public float duration = 3f;     // Duration in seconds to complete the movement
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;
    private bool comingUp = false;

    // Add reference to the particle system
    [SerializeField] private ParticleSystem breakEffect;
    private MeshRenderer meshRenderer;
    private Collider spikeCollider;

    private void Awake()
    {
        // Get references to components
        meshRenderer = GetComponent<MeshRenderer>();
        spikeCollider = GetComponent<Collider>();
    }

    public void Initialize(float distance, float durationTime)
    {
        moveDistance = distance;
        duration = durationTime;
        // Store the initial position of the object
        startPosition = transform.position;
        // Calculate the target position based on the moveDistance
        targetPosition = startPosition + Vector3.up * moveDistance;
        comingUp = true;
        breakEffect.Stop();
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

    // Method to break the spike apart
    public void BreakApart()
    {
        // Disable mesh renderer and collider
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (spikeCollider != null)
            spikeCollider.enabled = false;

        // Activate particle system if it exists
        if (breakEffect != null)
        {
            
            breakEffect.Play();

            // Destroy this object after the particle system finishes
            float lifetime = breakEffect.main.duration + breakEffect.main.startLifetime.constantMax;
            Destroy(gameObject, lifetime);
        }
        else
        {
            Debug.LogWarning("No particle system assigned to Spike!");
            Destroy(gameObject); // Destroy immediately if no particle system
        }
    }
}