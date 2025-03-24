using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class RandomSplineGenerator : MonoBehaviour
{
    [SerializeField] private int controlPointCount = 5; // Number of control points
    [SerializeField] private float randomnessAmount = 0.5f; // Controls how random the path is (0-1)
    [SerializeField] private float verticalRandomness = 0.3f; // Controls vertical randomness relative to the main path
    [SerializeField] private Transform startPointTransform; // Reference to the start position
    [SerializeField] private Transform endPointTransform; // Reference to the end position
    [SerializeField] private bool autoGenerate = true; // Whether to generate on start

    private SplineContainer splineContainer; // Reference to the SplineContainer

    private void Start()
    {
        // Create a new SplineContainer if one doesn't exist
        CreateSplineContainer();

        // Generate the random spline if auto-generate is enabled
        if (autoGenerate)
        {
            GenerateRandomLinearSpline(startPointTransform, endPointTransform);
        }
    }

    // Method to create a new SplineContainer
    private void CreateSplineContainer()
    {
        // Check if a SplineContainer already exists on this GameObject
        splineContainer = GetComponent<SplineContainer>();

        // If not, create a new one
        if (splineContainer == null)
        {
            splineContainer = gameObject.AddComponent<SplineContainer>();
            Debug.Log("Created a new SplineContainer.");
        }
    }

    // Method to generate a semi-random spline between two points
    public void GenerateRandomLinearSpline(Transform startPoint, Transform endPoint)
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is missing.");
            return;
        }

        // Use default positions if start/end points aren't assigned
        Vector3 startPosition = startPoint != null ? startPoint.position : new Vector3(-5, 0, 0);
        Vector3 endPosition = endPoint != null ? endPoint.position : new Vector3(5, 0, 0);

        // Calculate the main direction vector
        Vector3 mainDirection = (endPosition - startPosition).normalized;
        Vector3 totalDistance = endPosition - startPosition;
        float pathLength = totalDistance.magnitude;

        // Find perpendicular vectors for random offsets
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(mainDirection, up).normalized;
        if (right.magnitude < 0.1f) // If mainDirection is parallel to up
        {
            right = Vector3.Cross(mainDirection, Vector3.right).normalized;
        }
        Vector3 upVector = Vector3.Cross(right, mainDirection).normalized;

        // Clear any existing spline data
        splineContainer.Spline.Clear();

        // Add the start point
        BezierKnot startKnot = new BezierKnot(startPosition);
        float tangentLength = pathLength * 0.2f; // Adjust tangent length for smoothness
        startKnot.TangentOut = new float3(mainDirection * tangentLength);
        splineContainer.Spline.Add(startKnot);

        // Generate intermediate control points
        for (int i = 1; i < controlPointCount - 1; i++)
        {
            // Calculate position along the linear path
            float t = (float)i / (controlPointCount - 1);
            Vector3 linearPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Add randomness perpendicular to the main direction
            float randomRightOffset = UnityEngine.Random.Range(-randomnessAmount, randomnessAmount) * pathLength * 0.15f;
            float randomUpOffset = UnityEngine.Random.Range(-verticalRandomness, verticalRandomness) * pathLength * 0.15f;

            // Apply the random offset
            Vector3 randomPosition = linearPosition + (right * randomRightOffset) + (upVector * randomUpOffset);

            // Create a new BezierKnot with smooth tangents
            BezierKnot knot = new BezierKnot(randomPosition);

            // Set tangents to maintain flow along the main direction, adjusted for randomness
            Vector3 tangentDirection = mainDirection + (right * randomRightOffset * 0.1f) + (upVector * randomUpOffset * 0.1f);
            knot.TangentIn = new float3(-tangentDirection * tangentLength);
            knot.TangentOut = new float3(tangentDirection * tangentLength);

            // Add the control point to the spline
            splineContainer.Spline.Add(knot);
        }

        // Add the end point
        BezierKnot endKnot = new BezierKnot(endPosition);
        endKnot.TangentIn = new float3(-mainDirection * tangentLength);
        splineContainer.Spline.Add(endKnot);

        // Ensure the spline is not closed
        splineContainer.Spline.Closed = false;

        Debug.Log("Random semi-linear spline generated from " + startPosition + " to " + endPosition);
    }

    // Editor utility method to regenerate the spline
    public void RegenerateSpline()
    {
        GenerateRandomLinearSpline(startPointTransform, endPointTransform);
    }
}