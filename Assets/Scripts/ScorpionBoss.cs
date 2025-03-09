using UnityEngine;
using System.Collections;

public class ScorpionBoss : MonoBehaviour
{
    public enum BossState { Idle, Charge, TailAttack, Shockwave }
    private BossState currentState = BossState.Idle;

    [Header("General Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Charge Attack Settings")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Tail Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform tailFirePoint;
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Shockwave Attack Settings")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Transform groundPoint;
    [SerializeField] private float shockwaveSpeed = 10f;
    [SerializeField] private float spikeLifetime = 2f;

    private bool isAttacking = false;
    private float groundYPosition;

    void Start()
    {
        StartCoroutine(BossAI());
    }

    void Update()
    {
        FacePlayer();
    }

    private IEnumerator BossAI()
    {
        while (true)
        {
            if (!isAttacking)
            {
                yield return new WaitForSeconds(attackCooldown);

                int attackChoice = Random.Range(0, 3);

                switch (attackChoice)
                {
                    case 0:
                        Debug.Log("Scorpion Boss: Preparing **Charge Attack**");
                        StartCoroutine(ChargeAttack());
                        break;
                    case 1:
                        Debug.Log("Scorpion Boss: Preparing **Tail Projectile Attack**");
                        StartCoroutine(TailProjectileAttack());
                        break;
                    case 2:
                        Debug.Log("Scorpion Boss: Preparing **Shockwave Attack**");
                        StartCoroutine(ShockwaveAttack());
                        break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator ChargeAttack()
    {
        isAttacking = true;
        currentState = BossState.Charge;

        yield return StartCoroutine(RotateTowardsPlayer());

        yield return new WaitForSeconds(chargeWindupTime);

        Debug.Log("Scorpion Boss: **Charging at Player!**");

        Vector3 chargeDirection = (player.position - transform.position);
        chargeDirection.x = 0;
        chargeDirection.y = 0;
        chargeDirection.Normalize();

        rb.linearVelocity = chargeDirection * chargeSpeed;

        yield return new WaitForSeconds(chargeDuration);

        rb.linearVelocity = Vector3.zero;
        transform.position = new Vector3(transform.position.x, groundYPosition, transform.position.z);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator ShockwaveAttack()
    {
        isAttacking = true;
        currentState = BossState.Shockwave;

        Debug.Log("Scorpion Boss: **Performing Shockwave Attack!**");

        Vector3 startPos = new Vector3(transform.position.x, groundPoint.position.y, transform.position.z);
        Vector3 direction = (player.position - startPos).normalized;
        direction.x = 0; // Ensure movement is only in Z-Y plane

        GameObject activeSpike = null;
        float elapsedTime = 0f;

        while (elapsedTime < spikeLifetime)
        {
            // Destroy the previous spike
            if (activeSpike != null)
            {
                Destroy(activeSpike);
            }

            // Spawn a new spike moving forward
            Vector3 spawnPos = startPos + direction * (shockwaveSpeed * elapsedTime);
            spawnPos.y = groundYPosition + 0.1f; // Ensure spikes are slightly above ground
            activeSpike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the last spike after lifetime expires
        if (activeSpike != null)
        {
            Destroy(activeSpike);
        }

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator TailProjectileAttack()
    {
        isAttacking = true;
        currentState = BossState.TailAttack;

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Scorpion Boss: **Firing Tail Projectile!**");

        GameObject projectile = Instantiate(projectilePrefab, tailFirePoint.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        Vector3 direction = player.position - tailFirePoint.position;
        direction.x = 0;
        direction.y = 0;
        direction.Normalize();

        projectileRb.linearVelocity = direction * projectileSpeed;

        yield return new WaitForSeconds(1f);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private void FacePlayer()
    {
        if (!isAttacking)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.x = 0;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                targetRotation *= Quaternion.Euler(0, 180, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private IEnumerator RotateTowardsPlayer()
    {
        float timeElapsed = 0f;
        float maxRotationTime = 0.5f;

        while (timeElapsed < maxRotationTime)
        {
            FacePlayer();
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalDirection = (player.position - transform.position);
        finalDirection.x = 0;
        finalDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
        targetRotation *= Quaternion.Euler(0, 180, 0);
        transform.rotation = targetRotation;
    }
}