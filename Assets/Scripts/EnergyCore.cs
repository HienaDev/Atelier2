using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyCore : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectilesPerPoint = 10;
    [SerializeField] private float intervalBetweenProjectiles = 0.1f;

    [Header("Circular Fire Points")]
    [SerializeField] private int numberOfFirePoints = 8;
    [SerializeField] private float firePointRadius = 1f;

    private List<Transform> firePoints = new List<Transform>();

    private void Start()
    {
        GenerateFirePointsOnYZ();
        StartCoroutine(ChargeAndExplode());
    }

    private void GenerateFirePointsOnYZ()
    {
        firePoints.Clear();

        for (int i = 0; i < numberOfFirePoints; i++)
        {
            float angle = i * (360f / numberOfFirePoints);
            float rad = angle * Mathf.Deg2Rad;

            // Create position on the YZ circle (around X axis)
            Vector3 localPos = new Vector3(0f, Mathf.Cos(rad), Mathf.Sin(rad)) * firePointRadius;

            GameObject firePointObj = new GameObject("FirePoint_" + i);
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = localPos;
            firePointObj.transform.LookAt(transform.position); // orientado para o centro (opcional)

            firePoints.Add(firePointObj.transform);
        }
    }

    private IEnumerator ChargeAndExplode()
    {
        yield return new WaitForSeconds(chargeTime);
        yield return StartCoroutine(LaunchProjectiles());
        Destroy(gameObject);
    }

    private IEnumerator LaunchProjectiles()
    {
        List<Coroutine> activeBursts = new List<Coroutine>();

        foreach (Transform firePoint in firePoints)
        {
            Coroutine burst = StartCoroutine(FireBurst(firePoint));
            activeBursts.Add(burst);
        }

        foreach (Coroutine c in activeBursts)
        {
            yield return c;
        }
    }

    private IEnumerator FireBurst(Transform firePoint)
    {
        for (int i = 0; i < projectilesPerPoint; i++)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = proj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 dir = (firePoint.position - transform.position).normalized;
                rb.linearVelocity = dir * projectileSpeed;
            }

            yield return new WaitForSeconds(intervalBetweenProjectiles);
        }
    }
}