using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class RandomSplineGenerator : MonoBehaviour
{
    [SerializeField] private int controlPointCount = 5; // Number of control points
    [SerializeField] private float areaSize = 10f; // Size of the area where points are generated

    private SplineContainer splineContainer; // Reference to the SplineContainer

    private void Start()
    {
        // Create a new SplineContainer if one doesn't exist
        CreateSplineContainer();

        // Generate the random spline
        GenerateRandomSpline();
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

    // Method to generate a random spline
    public void GenerateRandomSpline()
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is missing.");
            return;
        }

        // Clear any existing spline data
        splineContainer.Spline.Clear();

        // Generate random control points
        for (int i = 0; i < controlPointCount; i++)
        {
            // Random position within the defined area
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-areaSize, areaSize),
                UnityEngine.Random.Range(-areaSize, areaSize),
                UnityEngine.Random.Range(-areaSize, areaSize)
            );

            // Add the control point to the spline
            splineContainer.Spline.Add(new BezierKnot(randomPosition));
        }

        Debug.Log("Random spline generated with " + controlPointCount + " control points.");
    }
}