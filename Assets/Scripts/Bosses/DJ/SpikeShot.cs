using UnityEngine;
using System.Collections;
public class SpikeShot : MonoBehaviour
{
    [SerializeField] private ParticleSystem breakEffect;
    
    private Rigidbody rb; // Reference to the Rigidbody component
    private MeshRenderer meshRenderer;
    private Collider spikeCollider;
    private Vector3 startPosition; // To track the starting position
    private float maxTravelDistance; // Maximum distance the spike can travel
    private bool initialized = false;

    private void Awake()
    {
        // Get references to components
        meshRenderer = GetComponent<MeshRenderer>();
        spikeCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // Default initialization with speed 5f and max distance 20f
        //Initialize(5f, 10f);
    }

    // Initialize method to set the speed and maximum travel distance
    public void Initialize(float speed, float maxDistance)
    {
        breakEffect.Stop();

        // Prevent multiple initializations
        if (initialized)
            return;

        initialized = true;

        // Get the Rigidbody component
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody component is missing on " + gameObject.name);
                return;
            }
        }

        // Store the starting position
        startPosition = transform.position;

        // Set the maximum travel distance
        maxTravelDistance = maxDistance;

        // Apply velocity in the up direction
        rb.linearVelocity = transform.up * speed;

        // Start the coroutine to check travel distance
        StartCoroutine(CheckTravelDistance());
    }

    private IEnumerator CheckTravelDistance()
    {
        while (true)
        {
            // Check if we've exceeded the maximum travel distance
            if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
            {
                BreakApart();
                yield break; // Exit the coroutine
            }

            yield return null; // Wait for the next frame
        }
    }

    public void BreakApart()
    {
        // Prevent multiple calls to BreakApart
        if (meshRenderer == null || !meshRenderer.enabled)
            return;

        // Disable mesh renderer and collider
        if (meshRenderer != null)
            meshRenderer.enabled = false;
        if (spikeCollider != null)
            spikeCollider.enabled = false;

        // Stop the rigidbody from moving
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

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