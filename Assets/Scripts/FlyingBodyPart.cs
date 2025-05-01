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

    private OvalPath path;
    private Transform origin;
    private float pathProgress = 0f;
    private float timer = 0f;
    private bool goingToPath = true;
    private bool orbiting = false;
    private bool returning = false;
    private Vector3 initialScale;
    private float currentRotationX;
    private Action onReturnComplete;
    private bool initialized = false;

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
        Transform model = null)
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

        initialScale = transform.localScale;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        if (goingToPath)
        {
            transform.position += Vector3.back * moveSpeed * Time.deltaTime;

            for (int i = 0; i <= 100; i++)
            {
                float t = i / 100f;
                Vector3 p = path.GetPointOnPath(t);
                float dist = Vector3.Distance(transform.position, p);

                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestT = t;
                }
            }

            if (bestDistance <= pathDetectionThreshold)
            {
                pathProgress = bestT;
                transform.position = path.GetPointOnPath(pathProgress);
                goingToPath = false;
                orbiting = true;
            }
            else if (bestDistance < 0.5f && Vector3.Distance(transform.position, path.GetPointOnPath(bestT)) > bestDistance + 0.05f)
            {
                pathProgress = bestT;
                transform.position = path.GetPointOnPath(pathProgress);
                goingToPath = false;
                orbiting = true;
            }
        }
        else if (orbiting)
        {
            timer += Time.deltaTime;
            pathProgress += (moveSpeed / path.GetApproximateLength()) * Time.deltaTime;
            transform.position = path.GetPointOnPath(pathProgress % 1f);

            if (timer >= lifetimeOnPath)
            {
                orbiting = false;
                returning = true;
            }
        }
        else if (returning)
        {
            Vector3 dir = (origin.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, origin.position) < 0.2f)
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

        if (!orbiting && !returning && timer <= scaleDuration)
        {
            float scaleMultiplier = Mathf.Clamp01(timer / scaleDuration);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one, scaleMultiplier);
        }
    }
}