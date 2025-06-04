using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyCore : MonoBehaviour
{
    private float chargeTime;
    private float activeDuration;
    private int numberOfPhases;
    private GameObject projectilePrefab;
    private float projectileSpeed;
    private int numberOfFirePoints;
    private float firePointRadius;
    private float burstInterval;
    private float lingerTime;

    private List<Transform> firePoints = new List<Transform>();
    private bool useEvenPoints = true;

    private ClearProjectiles clearProjectiles;

    private void Start()
    {
        clearProjectiles = FindAnyObjectByType<ClearProjectiles>();
        clearProjectiles.AddProjectile(gameObject);
    }

    public void Initialize(float chargeTime, float activeDuration, int numberOfPhases,
                           GameObject projectilePrefab, float projectileSpeed,
                           int numberOfFirePoints, float firePointRadius, float burstInterval,
                           float lingerTime)
    {
        this.chargeTime = chargeTime;
        this.activeDuration = activeDuration;
        this.numberOfPhases = numberOfPhases;
        this.projectilePrefab = projectilePrefab;
        this.projectileSpeed = projectileSpeed;
        this.numberOfFirePoints = numberOfFirePoints;
        this.firePointRadius = firePointRadius;
        this.burstInterval = burstInterval;
        this.lingerTime = lingerTime;

        GenerateFirePointsOnYZ();
        StartCoroutine(ExecuteCorePattern());
    }

    private IEnumerator ExecuteCorePattern()
    {
        yield return new WaitForSeconds(chargeTime);

        float timer = 0f;
        int phase = 0;
        float phaseDuration = activeDuration / numberOfPhases;
        float phaseTimer = 0f;

        while (timer < activeDuration)
        {
            FireCurrentPhase();
            yield return new WaitForSeconds(burstInterval);

            timer += burstInterval;
            phaseTimer += burstInterval;

            if (phaseTimer >= phaseDuration)
            {
                useEvenPoints = !useEvenPoints;
                phase++;
                phaseTimer = 0f;
            }
        }

        yield return new WaitForSeconds(lingerTime);
        Destroy(gameObject);
    }

    private void FireCurrentPhase()
    {
        for (int i = 0; i < firePoints.Count; i++)
        {
            bool isEven = i % 2 == 0;

            if ((useEvenPoints && isEven) || (!useEvenPoints && !isEven))
            {
                Transform firePoint = firePoints[i];
                Vector3 dir = (firePoint.position - transform.position).normalized;
                Quaternion rot = Quaternion.LookRotation(dir);

                // Instantiate the projectile at the fire point's position and rotation
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);
                clearProjectiles.AddProjectile(proj);
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = dir * projectileSpeed;
            }
        }
    }

    private void GenerateFirePointsOnYZ()
    {
        firePoints.Clear();

        for (int i = 0; i < numberOfFirePoints; i++)
        {
            float angle = i * (360f / numberOfFirePoints);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * firePointRadius;

            GameObject firePointObj = new GameObject("FirePoint_" + i);
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = localPos;
            firePointObj.transform.LookAt(transform.position);

            firePoints.Add(firePointObj.transform);
        }
    }
}