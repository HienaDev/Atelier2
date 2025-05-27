using UnityEngine;
using System;

public class FlyingBodyPart : MonoBehaviour
{
    private float moveSpeed;
    private float rotationSpeed;
    private float scaleDuration;
    private float lifetimeOnPath;
    private float pathDetectionThreshold;
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
        bool shouldHoming = false
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
        initialScale = transform.localScale;
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        if (goingToPath)
        {
            transform.position += new Vector3(0f, 0f, -moveSpeed * Time.deltaTime);
            for (int i = 0; i <= 100; i++)
            {
                float t = i / 100f;
                Vector3 p = path.GetPointOnPath(t);
                p.x = 0f;
                float dist = Vector3.Distance(transform.position, p);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestT = t;
                }
            }
            if (bestDistance <= pathDetectionThreshold ||
                (bestDistance < 0.5f && Vector3.Distance(transform.position, path.GetPointOnPath(bestT)) > bestDistance + 0.05f))
            {
                pathProgress = bestT;
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

                float step = moveSpeed * Time.deltaTime;
                transform.position += homingDirection * step;
                homingTravelled += step;

                if (homingTravelled >= homingDistance)
                {
                    transform.position = homingTarget;
                    inHomingPhase = false;
                    returnStartPoint = transform.position;
                    Vector3 pathTarget = path.GetPointOnPath(pathProgress % 1f);
                    pathTarget.x = 0f;
                    float dist = Vector3.Distance(returnStartPoint, pathTarget);
                    returnToPathDuration = dist / moveSpeed;
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
}