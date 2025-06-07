using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FlyingBodyPart : MonoBehaviour
{
    private float moveSpeed;
    private float rotationSpeed;
    private float scaleDuration;
    private float lifetimeOnPath;
    private float pathDetectionThreshold;
    private float homingSpeed;
    private Transform model;
    private Transform player;

    private OvalPath path;
    private Transform origin;
    private float pathProgress = 0f;
    private float timer = 0f;
    private bool goingToPath = true;
    private bool orbiting = false;
    private bool returning = false;
    private bool shouldHoming = false;
    private Vector3 initialScale;
    private float currentRotationX;
    private Action onReturnComplete;
    private bool initialized = false;
    private bool hasWeakpoint = false;

    private float[] homingTimes;
    private int nextHomingIndex = 0;
    private float orbitTimer = 0f;
    private Vector3 homingDirection;
    private bool inHomingPhase = false;
    private Vector3 homingTarget;
    private float homingDistance;
    private float homingTravelled;
    private bool returningToPath = false;
    private Vector3 pathExitPoint;
    private float returnToPathTimer = 0f;
    private float returnToPathDuration = 0f;
    private Vector3 returnStartPoint;
    private float bestDistance = float.MaxValue;
    private float bestT = -1f;

    private bool isBlinking = false;
    private List<Renderer> allRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private Material blinkMaterial;
    private float blinkInterval = 0.1f;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        SetupBlinkSystem();
    }

    private void SetupBlinkSystem()
    {
        allRenderers.Clear();
        originalMaterials.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.materials != null && renderer.materials.Length > 0)
            {
                allRenderers.Add(renderer);
                
                Material[] originalMats = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (renderer.materials[i] != null)
                    {
                        originalMats[i] = new Material(renderer.materials[i]);
                    }
                }
                originalMaterials.Add(originalMats);
            }
        }

        CreateBlinkMaterial();
    }

    private void CreateBlinkMaterial()
    {
        Shader shader = Shader.Find("Shader Graphs/VertexPulse");
        if (shader != null)
        {
            blinkMaterial = new Material(shader);
            blinkMaterial.SetColor("_Color", Color.white);
        }
    }

    public void Initialize(
        OvalPath selectedPath,
        Transform originPoint,
        Action onComplete = null,
        float moveSpeed = 5f,
        float rotationSpeed = 180f,
        float scaleDuration = 1f,
        float lifetimeOnPath = 4f,
        float pathDetectionThreshold = 0.3f,
        Transform model = null,
        Transform player = null,
        float[] homingTimes = null,
        bool shouldHoming = false,
        float homingSpeed = 8f
    )
    {
        path = selectedPath;
        origin = originPoint;
        onReturnComplete = onComplete;
        this.moveSpeed = moveSpeed;
        this.rotationSpeed = rotationSpeed;
        this.scaleDuration = scaleDuration;
        this.lifetimeOnPath = lifetimeOnPath;
        this.pathDetectionThreshold = pathDetectionThreshold;
        this.model = model;
        this.player = player;
        this.homingTimes = homingTimes ?? new float[0];
        this.shouldHoming = shouldHoming;
        this.homingSpeed = homingSpeed;
        initialScale = transform.localScale;
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        initialized = true;

        StartCoroutine(DelayedBlinkSetup());
    }

    private IEnumerator DelayedBlinkSetup()
    {
        yield return new WaitForEndOfFrame();
        SetupBlinkSystem();
    }

    public void SetHasWeakpoint(bool hasWeakpoint)
    {
        this.hasWeakpoint = hasWeakpoint;
    }

    public bool HasWeakpoint()
    {
        return hasWeakpoint;
    }

    private void Update()
    {
        if (!initialized) return;

        if (goingToPath)
        {
            transform.position += new Vector3(0f, 0f, -moveSpeed * Time.deltaTime);
            
            float closestDistance = float.MaxValue;
            float closestT = 0f;
            
            for (int i = 0; i <= 100; i++)
            {
                float t = i / 100f;
                Vector3 p = path.GetPointOnPath(t);
                p.x = 0f;
                float dist = Vector3.Distance(transform.position, p);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestT = t;
                }
            }
            
            if (closestDistance <= pathDetectionThreshold)
            {
                pathProgress = closestT;
                Vector3 pathPoint = path.GetPointOnPath(pathProgress);
                pathPoint.x = 0f;
                transform.position = pathPoint;
                goingToPath = false;
                orbiting = true;
                timer = 0f;
                orbitTimer = 0f;
                nextHomingIndex = 0;
                inHomingPhase = false;
                returningToPath = false;
            }
            
            if (transform.position.z < path.GetMinZ() - 2f)
            {
                pathProgress = 0f;
                Vector3 pathPoint = path.GetPointOnPath(pathProgress);
                pathPoint.x = 0f;
                transform.position = pathPoint;
                goingToPath = false;
                orbiting = true;
                timer = 0f;
                orbitTimer = 0f;
                nextHomingIndex = 0;
                inHomingPhase = false;
                returningToPath = false;
            }
        }
        else if (orbiting)
        {
            if (!inHomingPhase && !returningToPath)
            {
                orbitTimer += Time.deltaTime;
                pathProgress += (moveSpeed / path.GetApproximateLength()) * Time.deltaTime;
                Vector3 pathPoint = path.GetPointOnPath(pathProgress % 1f);
                pathPoint.x = 0f;
                transform.position = pathPoint;

                if (shouldHoming && nextHomingIndex < homingTimes.Length && orbitTimer >= homingTimes[nextHomingIndex])
                {
                    inHomingPhase = true;
                    StartBlinking();
                    pathExitPoint = pathPoint;
                    Vector3 target = player ? player.position : Vector3.zero;
                    target.x = 0f;
                    homingTarget = target;
                    homingDirection = (homingTarget - transform.position);
                    homingDirection.x = 0f;
                    homingDistance = homingDirection.magnitude;
                    homingDirection = homingDirection.normalized;
                    homingTravelled = 0f;
                    if (homingDirection.sqrMagnitude < 0.1f) homingDirection = Vector3.forward;
                }
            }
            else if (inHomingPhase)
            {
                orbitTimer += Time.deltaTime;
                pathProgress += (moveSpeed / path.GetApproximateLength()) * Time.deltaTime;

                float step = homingSpeed * Time.deltaTime;
                transform.position += homingDirection * step;
                homingTravelled += step;

                if (homingTravelled >= homingDistance)
                {
                    transform.position = homingTarget;
                    inHomingPhase = false;
                    StopBlinking();
                    returnStartPoint = transform.position;
                    Vector3 pathTarget = path.GetPointOnPath(pathProgress % 1f);
                    pathTarget.x = 0f;
                    float dist = Vector3.Distance(returnStartPoint, pathTarget);
                    returnToPathDuration = dist / homingSpeed;
                    returnToPathTimer = 0f;
                    returningToPath = true;
                    nextHomingIndex++;
                }
            }
            else if (returningToPath)
            {
                orbitTimer += Time.deltaTime;
                pathProgress += (moveSpeed / path.GetApproximateLength()) * Time.deltaTime;

                returnToPathTimer += Time.deltaTime;
                float t = Mathf.Clamp01(returnToPathTimer / returnToPathDuration);
                Vector3 pathTarget = path.GetPointOnPath(pathProgress % 1f);
                pathTarget.x = 0f;
                Vector3 newPos = Vector3.Lerp(returnStartPoint, pathTarget, t);
                newPos.x = 0f;
                transform.position = newPos;
                if (t >= 1f)
                {
                    returningToPath = false;
                }
            }

            if (orbitTimer >= lifetimeOnPath && !inHomingPhase && !returningToPath)
            {
                orbiting = false;
                returning = true;
                StopBlinking();
            }
        }
        else if (returning)
        {
            Vector3 originPos = origin.position;
            originPos.x = 0f;
            Vector3 dir = (originPos - transform.position);
            dir.x = 0f;
            dir = dir.sqrMagnitude < 0.01f ? Vector3.back : dir.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(new Vector3(0f, transform.position.y, transform.position.z),
                                    new Vector3(0f, originPos.y, originPos.z)) < 0.2f)
            {
                onReturnComplete?.Invoke();
                Destroy(gameObject);
            }
        }

        if (model != null)
        {
            currentRotationX += rotationSpeed * Time.deltaTime;
            model.rotation = Quaternion.Euler(currentRotationX, 90f, -90f);
        }

        if (!orbiting && !returning && !inHomingPhase && !returningToPath && timer <= scaleDuration)
        {
            float scaleMultiplier = Mathf.Clamp01(timer / scaleDuration);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one, scaleMultiplier);
        }
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (transform.position.x != 0f)
        {
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        }
    }

    public void EnableHoming(float[] newHomingTimes)
    {
        shouldHoming = true;
        homingTimes = newHomingTimes ?? new float[0];
        nextHomingIndex = 0;
    }

    public bool IsOrbiting()
    {
        return orbiting && !inHomingPhase && !returningToPath;
    }

    private void StartBlinking()
    {
        if (blinkMaterial == null) return;

        if (!isBlinking && blinkCoroutine == null)
        {
            isBlinking = true;
            blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    private void StopBlinking()
    {
        if (isBlinking)
        {
            isBlinking = false;
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            RestoreOriginalMaterials();
        }
    }

    private IEnumerator BlinkEffect()
    {
        bool useWhite = false;
        
        while (isBlinking)
        {
            if (useWhite)
            {
                ApplyWhiteMaterial();
            }
            else
            {
                RestoreOriginalMaterials();
            }
            
            useWhite = !useWhite;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void ApplyWhiteMaterial()
    {
        if (blinkMaterial == null) return;

        for (int i = 0; i < allRenderers.Count; i++)
        {
            if (allRenderers[i] != null)
            {
                Material[] whiteMaterials = new Material[allRenderers[i].materials.Length];
                for (int j = 0; j < whiteMaterials.Length; j++)
                {
                    whiteMaterials[j] = blinkMaterial;
                }
                allRenderers[i].materials = whiteMaterials;
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < allRenderers.Count && i < originalMaterials.Count; i++)
        {
            if (allRenderers[i] != null && originalMaterials[i] != null)
            {
                allRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void OnDestroy()
    {
        StopBlinking();
        
        if (blinkMaterial != null)
        {
            Destroy(blinkMaterial);
        }

        foreach (Material[] materials in originalMaterials)
        {
            if (materials != null)
            {
                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }
    }
}