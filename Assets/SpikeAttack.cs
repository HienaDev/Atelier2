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

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float elapsedTime = 0f;
    private bool isRotating = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpikePosition = 0;
        spikeDistanceIterator = fullSpikeLaneLength / spikeNumber;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - justSpawned > timeBetweenSpikeSpawn && spikeCounter < spikeNumber)
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
                Debug.Log("Rotation complete.");
            }
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
            targetAngle = Mathf.Min(angleToTarget, 90f); // Limit to 90 degrees
        }
        else
        {
            // Rotate left (counterclockwise)
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


        if(!alreadyGapped && spikeCounter >= 1)
            gapping = Random.Range(0, 100) < 100/spikeNumber * 2 ? true : false;


        if (!alreadyGapped && spikeCounter + gapSize + 1 >= spikeNumber)
            gapping = true;

        if (gapping)
        {
            alreadyGapped = true;
        }


        if(gapping && gapCounter < gapSize)
        {
            gapCounter++;
            currentSpikePosition -= spikeDistanceIterator;
            return;
        }



        GameObject spikeTemp = Instantiate(spike, transform);

        spikeTemp.transform.localPosition = new Vector3(0, -2.5f, currentSpikePosition);

        spikeTemp.GetComponent<Spike>().Initialize(spikeMoveDistance, spikeComeUpDuration);

        currentSpikePosition -= spikeDistanceIterator;

    }
}
