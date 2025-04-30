using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyCore : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private float activeDuration = 4f;
    [SerializeField] private int numberOfPhases = 2;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Fire Point Settings")]
    [SerializeField] private int numberOfFirePoints = 32;
    [SerializeField] private float firePointRadius = 1.5f;
    [SerializeField] private float burstInterval = 0.2f;

    private List<Transform> firePoints = new List<Transform>();
    private bool useEvenPoints = true;

    private void Start()
    {
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

        Destroy(gameObject);
    }

    private void FireCurrentPhase()
    {
        for (int i = 0; i < firePoints.Count; i++)
        {
            bool isEven = i % 2 == 0;

            if (useEvenPoints && isEven || !useEvenPoints && !isEven)
            {
                Transform firePoint = firePoints[i];

                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                Rigidbody rb = proj.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 dir = (firePoint.position - transform.position).normalized;
                    rb.linearVelocity = dir * projectileSpeed;
                }
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