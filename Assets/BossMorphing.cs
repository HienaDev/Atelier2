using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BossMorphing : MonoBehaviour
{
    [Serializable]
    public class BossPhase
    {
        public int numberOfPhase;
        public string name;
        public Transform[] bossParts;
    }

    [Serializable]
    public class Boss
    {
        public Transform[] parts;
        public BossPhase[] bossPhases;
    }

    [SerializeField] private Boss boss;
    [SerializeField] private float transformationSpeedSpline = 1f;
    [SerializeField] private float transformationSpeedLerp = 0.2f;

    private Coroutine currentCoroutine;

    private List<SplineContainer> splineContainerList = new List<SplineContainer>();
    private List<SplineAnimate> splineAnimateList = new List<SplineAnimate>();

    [SerializeField] private int controlPointCount = 5; // Number of control points
    [SerializeField] private float randomnessAmount = 0.5f; // Controls how random the path is (0-1)
    [SerializeField] private float verticalRandomness = 0.3f; // Controls vertical randomness relative to the main path
    [SerializeField] private float archHeight = 0.5f; // Controls the height of the arch (0-1)
    [SerializeField] private bool useArchedPath = false; // Toggle between random and arched paths

    public bool Morpinhg { get; private set; }

    private void Start()
    {
        ToggleMeshes(false);

        // Create a new SplineContainer if one doesn't exist
        CreateSplineContainer();

        Morpinhg = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(PhaseMorphSpline(boss, boss.bossPhases[1]));
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(PhaseMorphSpline(boss, boss.bossPhases[0]));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(PhaseMorphLerp(boss, boss.bossPhases[1]));
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(PhaseMorphLerp(boss, boss.bossPhases[0]));
        }
    }

    private void ToggleMeshes(bool toggle)
    {
        foreach (BossPhase phase in boss.bossPhases)
        {
            foreach (Transform part in phase.bossParts)
            {
                Renderer renderer = part.gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = toggle;
                }
            }
        }
    }

    private void ToggleColliders(bool toggle)
    {
        foreach (Transform part in boss.parts)
        {
            Collider collider = part.gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = toggle;
            }

        }
    }

    public bool ChangePhase(int phaseNumber)
    {
        foreach (BossPhase phase in boss.bossPhases)
        {
            if (phase.numberOfPhase == phaseNumber)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(PhaseMorphSpline(boss, phase));
                return true;

            }
        }

        return false;
    }

    public IEnumerator PhaseMorphLerp(Boss boss, BossPhase toPhase)
    {

        ToggleColliders(false);

        Morpinhg = true;
        float lerpValue = 0f;

        List<Vector3> originalPositions = new List<Vector3>();
        List<Quaternion> originalRotations = new List<Quaternion>();
        List<Vector3> originalScales = new List<Vector3>();
        for (int i = 0; i < boss.parts.Length; i++)
        {
            originalPositions.Add(boss.parts[i].position);
            originalRotations.Add(boss.parts[i].rotation);
            originalScales.Add(boss.parts[i].localScale);
        }

        while (lerpValue < 0.999f)
        {
            lerpValue += Time.deltaTime * transformationSpeedLerp;
            lerpValue = Mathf.Clamp(lerpValue, 0, 0.999f);
            for (int i = 0; i < boss.parts.Length; i++)
            {
                boss.parts[i].position = Vector3.Lerp(originalPositions[i], toPhase.bossParts[i].position, lerpValue);
                boss.parts[i].rotation = Quaternion.Lerp(originalRotations[i], toPhase.bossParts[i].rotation, lerpValue);
                boss.parts[i].localScale = Vector3.Lerp(originalScales[i], toPhase.bossParts[i].localScale, lerpValue);
            }

            yield return null;
        }

        Morpinhg = false;

        ToggleColliders(true);
    }

    public IEnumerator PhaseMorphSpline(Boss boss, BossPhase toPhase)
    {

        ToggleColliders(false);

        Morpinhg = true;
        float lerpValue = 0f;

        // Collect original transformations
        List<Vector3> originalPositions = new List<Vector3>();
        List<Quaternion> originalRotations = new List<Quaternion>();
        List<Vector3> originalScales = new List<Vector3>();
        for (int i = 0; i < boss.parts.Length; i++)
        {
            originalPositions.Add(boss.parts[i].position);
            originalRotations.Add(boss.parts[i].rotation);
            originalScales.Add(boss.parts[i].localScale);
        }

        // Generate splines
        for (int i = 0; i < boss.parts.Length; i++)
        {
            // Start position is the current position of the part
            // End position is the target position from the toPhase
            if (useArchedPath)
            {
                GenerateArchedSpline(boss.parts[i].position, toPhase.bossParts[i].position, splineContainerList[i]);
            }
            else
            {
                GenerateRandomLinearSpline(boss.parts[i].position, toPhase.bossParts[i].position, splineContainerList[i]);
            }

            // Set the SplineAnimate component to use the current position as the starting point
            splineAnimateList[i].Container = splineContainerList[i];
            splineAnimateList[i].Duration = 1f; // Match with our normalized time
            splineAnimateList[i].ElapsedTime = 0f;
        }

        while (lerpValue < 0.999f)
        {
            lerpValue += Time.deltaTime * transformationSpeedSpline;
            lerpValue = Mathf.Clamp(lerpValue, 0f, 0.999f);

            for (int i = 0; i < boss.parts.Length; i++)
            {
                // Update spline position
                splineAnimateList[i].ElapsedTime = lerpValue;

                // Rotation and scale are still handled with Lerp
                boss.parts[i].rotation = Quaternion.Lerp(originalRotations[i], toPhase.bossParts[i].rotation, lerpValue);
                boss.parts[i].localScale = Vector3.Lerp(originalScales[i], toPhase.bossParts[i].localScale, lerpValue);
            }

            yield return null;
        }

        Morpinhg = false;

        ToggleColliders(true);
    }

    private void CreateSplineContainer()
    {
        foreach (Transform part in boss.parts)
        {
            // Create a GameObject to hold the SplineContainer
            GameObject splineObj = new GameObject($"SplineContainer_{part.name}");
            splineObj.transform.SetParent(transform);

            // Add the SplineContainer component
            SplineContainer splineContainer = splineObj.AddComponent<SplineContainer>();
            splineContainerList.Add(splineContainer);

            // Add SplineAnimate component to the part
            SplineAnimate splineAnimate = part.gameObject.AddComponent<SplineAnimate>();
            splineAnimate.Container = splineContainer;
            splineAnimateList.Add(splineAnimate);
        }
    }

    public void GenerateRandomLinearSpline(Vector3 startPosition, Vector3 endPosition, SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is missing.");
            return;
        }

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

        Debug.Log($"Random semi-linear spline generated from {startPosition} to {endPosition}");
    }

    public void GenerateArchedSpline(Vector3 startPosition, Vector3 endPosition, SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is missing.");
            return;
        }

        // Calculate the main direction vector
        Vector3 mainDirection = (endPosition - startPosition).normalized;
        Vector3 totalDistance = endPosition - startPosition;
        float pathLength = totalDistance.magnitude;

        // Find perpendicular vectors for creating the arch
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
        float tangentLength = pathLength * 0.25f; // Slightly longer tangents for arch
        startKnot.TangentOut = new float3(mainDirection * tangentLength + upVector * (archHeight * pathLength * 0.2f));
        splineContainer.Spline.Add(startKnot);

        // Calculate the mid point with an arch
        Vector3 midPoint = Vector3.Lerp(startPosition, endPosition, 0.5f);
        // Add the arch height in the upVector direction
        Vector3 archedMidPoint = midPoint + (upVector * (archHeight * pathLength * 0.5f));

        // Add the mid control point for the arch
        BezierKnot midKnot = new BezierKnot(archedMidPoint);

        // Calculate tangent directions that create a smooth arch
        Vector3 toStartTangent = (startPosition - archedMidPoint).normalized;
        Vector3 toEndTangent = (endPosition - archedMidPoint).normalized;

        midKnot.TangentIn = new float3(toStartTangent * tangentLength);
        midKnot.TangentOut = new float3(toEndTangent * tangentLength);

        splineContainer.Spline.Add(midKnot);

        // Add the end point
        BezierKnot endKnot = new BezierKnot(endPosition);
        endKnot.TangentIn = new float3(-mainDirection * tangentLength + upVector * (archHeight * pathLength * 0.2f));
        splineContainer.Spline.Add(endKnot);

        // Ensure the spline is not closed
        splineContainer.Spline.Closed = false;

        Debug.Log($"Arched spline generated from {startPosition} to {endPosition} with arch height {archHeight}");
    }
}