using UnityEngine;
using System.Collections;

public class ScorpionBoss : MonoBehaviour
{
    public enum BossState { Idle, Charge, TailAttack, StabAttack, SpikeDown }
    private BossState currentState = BossState.Idle;

    [Header("General Settings")]
    private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private CameraShake cameraShake;

    [Header("Charge Attack Settings")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeWindupTime = 0.8f;

    [Header("Tail Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform tailFirePoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float tailAttackMinRange = 6f;

    [Header("Stab Attack Settings")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float stabAttackMinRange = 3f;
    [SerializeField] private float stabAttackMaxRange = 7f;

    [Header("SpikeDown")]
    [SerializeField] private Transform highSpot;
    [SerializeField] private GameObject groundSpikesPrefab;
    [SerializeField] private Transform groundSpikesSpawnPoint;
    [SerializeField] private float groundSpikesMoveTime = 0.5f;
    [SerializeField] private float groundSpikesUpDuration = 1f;

    private bool isSlamImpactTriggered;
    private bool isAttacking;
    private float originalGroundY;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovementQuark>().transform;
        originalGroundY = transform.position.y;
        isSlamImpactTriggered = false;
        isAttacking = false;
        StartCoroutine(BossAI());
    }

    void Update()
    {
        FacePlayer();

        // If pressed space key, activate spikes 
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Scorpion Boss: **Spikes Activated!**");
            ActivateArenaSpikes();
        }

    }

    private IEnumerator BossAI()
    {
        while (true)
        {
            if (!isAttacking)
            {
                yield return new WaitForSeconds(attackCooldown);

                bool attackChosen = false;
                int maxAttempts = 10; // Prevent infinite loops
                int attempts = 0;

                while (!attackChosen && attempts < maxAttempts)
                {
                    int attackChoice = Random.Range(2, 3); // 0 = Charge, 1 = Tail, 2 = Stab, 3 = SpikeDown
                    
                    switch (attackChoice)
                    {
                        case 0: // Charge Attack (Always allowed)
                            Debug.Log("Scorpion Boss: Preparing **Charge Attack**");
                            StartCoroutine(ChargeAttack());
                            attackChosen = true;
                            break;

                        case 1: // Tail Projectile Attack (Only if player is FAR from boss)
                            if (Vector3.Distance(transform.position, player.position) > tailAttackMinRange)
                            {
                                Debug.Log("Scorpion Boss: Preparing **Tail Projectile Attack**");
                                StartCoroutine(TailProjectileAttack());
                                attackChosen = true;
                            }
                            break;

                        case 2: // Stab Attack (Only if player is within a specific range)
                            float playerDistance = Vector3.Distance(transform.position, player.position);
                            if (playerDistance > stabAttackMinRange && playerDistance < stabAttackMaxRange)
                            {
                                Debug.Log("Scorpion Boss: Preparing **Stab Attack**");
                                StartCoroutine(StabAttack());
                                attackChosen = true;
                            }
                            break;

                        case 3: // Spike Down Attack (Always allowed)
                            Debug.Log("Scorpion Boss: Preparing **Spike Down Attack**");
                            StartCoroutine(SpikeDownAttack());
                            attackChosen = true;
                            break;
                    }

                    attempts++;
                }

                // If no valid attack was found, do a default attack
                if (!attackChosen)
                {
                    Debug.LogWarning("Scorpion Boss: No valid attack found! Defaulting to Charge Attack.");
                    StartCoroutine(ChargeAttack());
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

        // Windup animation before charging
        // animator.SetTrigger("ChargeWindup");

        cameraShake.ShakeCamera(0.5f, chargeWindupTime);

        yield return new WaitForSeconds(chargeWindupTime);

        Debug.Log("Scorpion Boss: **Charging at Player!**");

        cameraShake.ShakeCamera(1f, chargeDuration);

        // Charge attack animation
        // animator.SetTrigger("Charge");

        Vector3 chargeDirection = player.position - transform.position;
        chargeDirection.x = 0;
        chargeDirection.y = 0;
        chargeDirection.Normalize();

        rb.linearVelocity = chargeDirection * chargeSpeed;

        yield return new WaitForSeconds(chargeDuration);

        rb.linearVelocity = Vector3.zero;

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator TailProjectileAttack()
    {
        isAttacking = true;
        currentState = BossState.TailAttack;

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Scorpion Boss: **Firing Tail Projectile!**");

        // Tail attack animation
        // animator.SetTrigger("TailAttack");

        GameObject projectile = Instantiate(projectilePrefab, tailFirePoint.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        Vector3 direction = player.position - tailFirePoint.position;
        direction.x = 0;
        direction.y = 0;
        direction.Normalize();

        projectileRb.linearVelocity = direction * projectileSpeed;

        Destroy(projectile, 3f);

        yield return new WaitForSeconds(1f);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator StabAttack()
    {
        isAttacking = true;
        currentState = BossState.StabAttack;

        Debug.Log("Scorpion Boss: **Dashing Forward!**");

        cameraShake.ShakeCamera(0.3f, dashDuration);

        // Dash windup animation
        // animator.Play("StabDash");

        Vector3 dashDirection = player.position - transform.position;
        dashDirection.x = 0;
        dashDirection.y = 0;
        dashDirection.Normalize();

        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector3.zero;

        Debug.Log("Scorpion Boss: **Performing Stab Attack!**");

        // Stab attack animation
        animator.Play("StabDash", 0, 0);

        yield return new WaitForSeconds(1f); // Adjust based on animation length

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator SpikeDownAttack()
    {
        isAttacking = true;
        currentState = BossState.SpikeDown;

        Debug.Log("Scorpion Boss: **Spike Down Attack!**");

        // Move scorpion to high position smoothly
        while (Vector3.Distance(transform.position, highSpot.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, highSpot.position, 15f * Time.deltaTime);
            yield return null;
        }

        // Smoothly rotate downward
        Quaternion targetRotation = Quaternion.Euler(0, -90, 0);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            yield return null;
        }

        // Windup animation before slamming
        // animator.SetTrigger("SpikeDown");

        yield return new WaitForSeconds(1f); // Adjust based on animation length

        // Wait until the impact is triggered
        //yield return new WaitUntil(() => isSlamImpactTriggered);

        // **Placeholder**: Wait 1 second before descending (Replace with animation trigger later)
        Debug.Log("Scorpion Boss: **Waiting 1s before descending...**");
        yield return new WaitForSeconds(1f);

        Debug.Log("Scorpion Boss: **Returning to ground!**");
        Vector3 groundPos = new Vector3(transform.position.x, originalGroundY, transform.position.z);
        while (Vector3.Distance(transform.position, groundPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPos, 20f * Time.deltaTime);
            yield return null;
        }

        // Smoothly rotate back to original position
        RotateTowardsPlayer();

        Debug.Log("Scorpion Boss: **Landed back on the ground!**");

        yield return new WaitForSeconds(0.5f); // Short delay before resuming normal state

        currentState = BossState.Idle;
        isAttacking = false;
    }

    public void TriggerSpikeActivation()
    {
        isSlamImpactTriggered = true;
    }

    private void ActivateArenaSpikes()
    {
        GameObject spike = Instantiate(groundSpikesPrefab, groundSpikesSpawnPoint.position, Quaternion.identity);
        StartCoroutine(RaiseAndLowerSpike(spike));
    }

    private IEnumerator RaiseAndLowerSpike(GameObject spike)
    {
        Vector3 startPos = spike.transform.position;
        Vector3 targetPos = startPos + Vector3.up * 2f;
        cameraShake.ShakeCamera(3, groundSpikesMoveTime);

        // Raise spikes over spikeMoveTime seconds
        float elapsedTime = 0f;
        while (elapsedTime < groundSpikesMoveTime)
        {
            spike.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / groundSpikesMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spike.transform.position = targetPos; // Ensure it reaches the exact position

        // Spikes stay up for the configured duration
        yield return new WaitForSeconds(groundSpikesUpDuration);

        cameraShake.ShakeCamera(1.5f, groundSpikesMoveTime);

        // Lower spikes over spikeMoveTime seconds
        elapsedTime = 0f;
        while (elapsedTime < groundSpikesMoveTime)
        {
            spike.transform.position = Vector3.Lerp(targetPos, startPos, elapsedTime / groundSpikesMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spike.transform.position = startPos; // Ensure it returns exactly to the ground

        Destroy(spike);
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

        Vector3 finalDirection = player.position - transform.position;
        finalDirection.x = 0;
        finalDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
        targetRotation *= Quaternion.Euler(0, 180, 0);
        transform.rotation = targetRotation;
    }

    // Draw the range of the attacks in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tailAttackMinRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stabAttackMinRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stabAttackMaxRange);
    }
}