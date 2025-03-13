using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static PhaseManager;

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
        foreach (BossPhase phase in boss.bossPhases)
        {
            ToggleMeshes(false, phase.numberOfPhase);
        }

        // Create a new SplineContainer if one doesn't exist
        CreateSplineContainer();

        Morpinhg = false;
    }

    private void ToggleMeshes(bool toggle, int numberOfPhase)
    {
        if (numberOfPhase == -1)
        {
            foreach (Transform part in boss.parts)
            {
                Renderer renderer = part.gameObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = toggle;
                }
            }

        }
        else
        {
            foreach (BossPhase phase in boss.bossPhases)
            {
                if (phase.numberOfPhase == numberOfPhase)
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
        ToggleMeshes(true, -1);
        ToggleMeshes(false, toPhase.numberOfPhase);

        Morpinhg = true;
        float lerpValue = 0f;

        List<Vector3> originalPositions = new List<Vector3>();
        List<Quaternion> originalRotations = new List<Quaternion>();
        List<Vector3> originalScales = new List<Vector3>();

        for (int i = 0; i < boss.parts.Length; i++)
        {
            originalPositions.Add(boss.parts[i].position);
            originalRotations.Add(boss.parts[i].rotation);
            originalScales.Add(boss.parts[i].lossyScale); // Use lossyScale to get world scale
        }

        while (lerpValue < 0.999f)
        {
            lerpValue += Time.deltaTime * transformationSpeedLerp;
            lerpValue = Mathf.Clamp(lerpValue, 0, 0.999f);

            for (int i = 0; i < boss.parts.Length; i++)
            {
                // Get target values in world space
                Vector3 targetPosition = toPhase.bossParts[i].position;
                Quaternion targetRotation = toPhase.bossParts[i].rotation;
                Vector3 targetScale = toPhase.bossParts[i].lossyScale;

                // Apply position and rotation
                boss.parts[i].position = Vector3.Lerp(originalPositions[i], targetPosition, lerpValue);
                boss.parts[i].rotation = Quaternion.Lerp(originalRotations[i], targetRotation, lerpValue);

                // Handle scale more carefully - convert from world to local scale
                Vector3 currentWorldScale = Vector3.Lerp(originalScales[i], targetScale, lerpValue);
                // Convert world scale to local scale if there's a parent
                if (boss.parts[i].parent != null)
                {
                    Vector3 parentWorldScale = boss.parts[i].parent.lossyScale;
                    boss.parts[i].localScale = new Vector3(
                        currentWorldScale.x / parentWorldScale.x,
                        currentWorldScale.y / parentWorldScale.y,
                        currentWorldScale.z / parentWorldScale.z
                    );
                }
                else
                {
                    boss.parts[i].localScale = currentWorldScale;
                }
            }

            yield return null;
        }

        Morpinhg = false;

        ToggleMeshes(false, -1);
        ToggleMeshes(true, toPhase.numberOfPhase);
        ToggleColliders(true);
    }

    public IEnumerator PhaseMorphSpline(Boss boss, BossPhase toPhase)
    {
        ToggleColliders(false);

        Morpinhg = true;
        float lerpValue = 0f;

        // Collect original transformations in world space
        List<Vector3> originalPositions = new List<Vector3>();
        List<Quaternion> originalRotations = new List<Quaternion>();
        List<Vector3> originalScales = new List<Vector3>();

        // Get the parent transform of the destination phase
        Transform phaseParent = toPhase.bossParts[0].parent;

        for (int i = 0; i < boss.parts.Length; i++)
        {
            originalPositions.Add(boss.parts[i].position);
            originalRotations.Add(boss.parts[i].rotation);
            originalScales.Add(boss.parts[i].localScale);
        }

        // Generate splines
        for (int i = 0; i < boss.parts.Length; i++)
        {
            // Get the target position in world space
            Vector3 targetPosition = toPhase.bossParts[i].position;

            // Generate spline from current position to target position
            GenerateRandomLinearSpline(boss.parts[i].position, targetPosition, splineContainerList[i]);

            // Set the SplineAnimate component
            splineAnimateList[i].Container = splineContainerList[i];
            splineAnimateList[i].Duration = 1f;
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

                // Get the target rotation and scale in world space
                Quaternion targetRotation = toPhase.bossParts[i].rotation;
                Vector3 targetScale = toPhase.bossParts[i].lossyScale; // Use lossyScale to get world scale

                // Apply rotation and scale
                boss.parts[i].rotation = Quaternion.Lerp(originalRotations[i], targetRotation, lerpValue);

                // Handle scale more carefully - convert from world to local scale
                Vector3 currentWorldScale = Vector3.Lerp(originalScales[i], targetScale, lerpValue);
                // Convert world scale to local scale if there's a parent
                if (boss.parts[i].parent != null)
                {
                    Vector3 parentWorldScale = boss.parts[i].parent.lossyScale;
                    boss.parts[i].localScale = new Vector3(
                        currentWorldScale.x / parentWorldScale.x,
                        currentWorldScale.y / parentWorldScale.y,
                        currentWorldScale.z / parentWorldScale.z
                    );
                }
                else
                {
                    boss.parts[i].localScale = currentWorldScale;
                }
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

}