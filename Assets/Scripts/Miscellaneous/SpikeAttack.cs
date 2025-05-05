using UnityEngine;

public class SpikeAttack : MonoBehaviour
{
    [SerializeField] private GameObject spike;
    [SerializeField] private float fullSpikeLaneLength = 10f;
    [SerializeField] private int spikeNumber = 5;
    private int spikeCounter = 0;
    [SerializeField] private float timeBetweenSpikeSpawn = 0.2f;
    private float justSpawned = 0f;

    private float currentSpikePosition;
    private float spikeDistanceIterator;

    [SerializeField] private float spikeComeUpDuration = 0.5f;
    [SerializeField] private float spikeMoveDistance = 2.5f;

    [SerializeField] private int gapSize = 2;
    private int gapCounter = 0;
    private bool gapping = false;
    private bool alreadyGapped = false;

    private bool startTravelling = false;

    public Transform targetObject; // The object to rotate towards
    public float rotationDuration = 3f; // Duration for the rotation
    public float movementSpeed = 5f; // Speed at which the spikes move after rotation
    public float movementDelay = 2f; // Delay before spikes start moving
    public float maxTravelDistance = 10f; // Maximum distance spikes can travel before being destroyed

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float elapsedTime = 0f;
    private bool isRotating = false;
    private bool isMoving = false;
    private float movementStartTime = 0f;
    private Vector3 initialPosition; // Initial position of the spikes

    [SerializeField] private float heightOffset = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetObject = FindAnyObjectByType<PlayerMovementMonkeyHell>().transform;
        currentSpikePosition = 0;
        spikeDistanceIterator = fullSpikeLaneLength / spikeNumber;

        transform.position += new Vector3(0f, heightOffset, 0f);
        initialPosition = transform.position; // Store the initial position
    }





    // Update is called once per frame
    void Update()
    {


        if (Time.time - justSpawned > timeBetweenSpikeSpawn && spikeCounter < spikeNumber)
        {
            SpawnSpike();
            justSpawned = Time.time;
            spikeCounter++;
            if (spikeCounter == spikeNumber)
            {
                isRotating = true;
                CheckForPlayerPosition();
            }
        }

        if (isRotating)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Calculate the fraction of the duration that has passed
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);

            // Interpolate the rotation between start and target using Slerp
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Stop rotating once the duration is reached
            if (t >= 1f)
            {
                isRotating = false;
                movementStartTime = Time.time; // Record the time when rotation completes
                isMoving = true;
                //Debug.Log("Rotation complete. Waiting to start moving.");
            }
        }

        if (isMoving && Time.time - movementStartTime >= movementDelay)
        {
            MoveSpikes();
            CheckTravelDistance();
        }
    }

    public void CheckForPlayerPosition()
    {
        // Store the initial rotation
        startRotation = transform.rotation;

        // Calculate the direction to the target object
        Vector2 directionToTarget = targetObject.position - transform.position;
        directionToTarget.Normalize();

        // Calculate the current forward direction of this object
        Vector2 currentForward = transform.up; // Assuming the object's "forward" is along the Y-axis

        // Calculate the angle between the current forward and the direction to the target
        float angleToTarget = Vector2.SignedAngle(currentForward, directionToTarget);

        // Determine whether to rotate left or right (90 degrees maximum)
        float targetAngle = 0f;
        if (angleToTarget > 0)
        {
            // Rotate right (clockwise)
            //Debug.Log(gameObject.name + " is rotating right.");
            targetAngle = Mathf.Min(angleToTarget, 90f); // Limit to 90 degrees
        }
        else
        {
            // Rotate left (counterclockwise)
            //Debug.Log(gameObject.name + " is rotating left.");
            targetAngle = Mathf.Max(angleToTarget, -90f); // Limit to -90 degrees
        }

        // Calculate the target rotation
        targetRotation = startRotation * Quaternion.Euler(0, 0, targetAngle);

        // Start the rotation
        isRotating = true;
    }

    public void SpawnSpike()
    {
        if (spike == null)
            return;

        if (!alreadyGapped && spikeCounter >= 1)
            gapping = Random.Range(0, 100) < 100 / spikeNumber * 2 ? true : false;

        if (!alreadyGapped && spikeCounter + gapSize + 1 >= spikeNumber)
            gapping = true;

        if (gapping)
        {
            alreadyGapped = true;
        }

        if (gapping && gapCounter < gapSize)
        {
            gapCounter++;
            currentSpikePosition += spikeDistanceIterator;
            return;
        }

        GameObject spikeTemp = Instantiate(spike, transform);

        spikeTemp.transform.localPosition = new Vector3(0f, -2.5f, currentSpikePosition);

        spikeTemp.GetComponent<Spike>().Initialize(spikeMoveDistance, spikeComeUpDuration);

        currentSpikePosition += spikeDistanceIterator;
    }

    private void MoveSpikes()
    {
        // Move the spikes in the direction they are facing
        transform.position += transform.up * movementSpeed * Time.deltaTime;
    }

    private void CheckTravelDistance()
    {
        // Calculate the distance traveled from the initial position
        float distanceTraveled = Vector3.Distance(initialPosition, transform.position);

        // If the spikes have traveled the maximum distance, trigger break effect instead of destroying
        if (distanceTraveled >= maxTravelDistance)
        {
            //Debug.Log("Spikes have traveled the maximum distance. Breaking them apart.");

            // Get all child spikes and trigger break effect
            Spike[] spikes = GetComponentsInChildren<Spike>();
            foreach (Spike spikeComponent in spikes)
            {
                spikeComponent.BreakApart();
            }

            // Destroy the parent object after all spikes have broken
            Destroy(gameObject, 5f); // 5 seconds should be enough for particles to finish

            // Disable this script to prevent further updates
            enabled = false;
        }
    }
}