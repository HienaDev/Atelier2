using UnityEngine;
using System.Collections;

public class ScorpionBoss : MonoBehaviour
{
    public enum BossState { Idle, Charge, TailAttack, StabAttack, SpikeDown }
    public enum BossDifficulty { Tutorial, Easy, Normal }

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

    [Header("Weakpoint Settings")]
    [SerializeField] private GameObject weakpointPrefab;
    [SerializeField] private WeakpointSlot[] weakpointSpawnPoints;
    [SerializeField] private int requiredWeakpointsToDestroy = 2;
    [System.Serializable]
    public struct WeakpointSlot
    {
        public Transform spawnPoint;
        [Min(0.01f)] public float uniformScale; // single value for X, Y, Z
    }

    [Header("Difficulty Percentages")]
    [SerializeField] [Range(10, 200)] private float tutorialPercentage = 50f; 
    [SerializeField] [Range(10, 200)] private float easyPercentage = 75f;

    private BossDifficulty difficulty;
    private bool isSlamImpactTriggered;
    private bool isAttacking;
    private float originalGroundY;
    private float baseChargeSpeed;
    private float baseChargeDuration;
    private float baseChargeWindupTime;
    private float baseProjectileSpeed;
    private float baseAttackCooldown;
    private float baseDashSpeed;
    private float baseDashDuration;
    private float baseGroundSpikesMoveTime;
    private float baseGroundSpikesUpDuration;
    private int weakpointsDestroyed;

    public void SetDifficulty(BossDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
        ApplyDifficultySettings();
    }

    private void ApplyDifficultySettings()
    {
        float percent = 100f;

        switch (difficulty)
        {
            case BossDifficulty.Tutorial:
                percent = tutorialPercentage;
                break;
            case BossDifficulty.Easy:
                percent = easyPercentage;
                break;
            case BossDifficulty.Normal:
            default:
                percent = 100f;
                break;
        }

        float speedMultiplier = percent / 100f;           
        float inverseMultiplier = 200f / (percent + 100f);

        chargeSpeed = baseChargeSpeed * speedMultiplier;
        chargeDuration = baseChargeDuration * inverseMultiplier;
        chargeWindupTime = baseChargeWindupTime * inverseMultiplier;
        projectileSpeed = baseProjectileSpeed * speedMultiplier;
        attackCooldown = baseAttackCooldown * inverseMultiplier;
        dashSpeed = baseDashSpeed * speedMultiplier;
        dashDuration = baseDashDuration * inverseMultiplier;
        groundSpikesMoveTime = baseGroundSpikesMoveTime * inverseMultiplier;
        groundSpikesUpDuration = baseGroundSpikesUpDuration * inverseMultiplier;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerMovementQuark>().transform;
        originalGroundY = transform.position.y;
        isSlamImpactTriggered = false;
        isAttacking = false;
        weakpointsDestroyed = 0;

        // Store the base values before modifying them
        baseChargeSpeed = chargeSpeed;
        baseChargeDuration = chargeDuration;
        baseChargeWindupTime = chargeWindupTime;
        baseProjectileSpeed = projectileSpeed;
        baseAttackCooldown = attackCooldown;
        baseDashSpeed = dashSpeed;
        baseDashDuration = dashDuration;
        baseGroundSpikesMoveTime = groundSpikesMoveTime;
        baseGroundSpikesUpDuration = groundSpikesUpDuration;

        SetDifficulty(BossDifficulty.Tutorial); // Set initial difficulty

        SpawnWeakpoints(); // Spawn weakpoints at the start

        StartCoroutine(WaitForWeakpointsDestroyed()); // Wait for weakpoints to be destroyed before starting AI
    }

    private void Update()
    {
        FacePlayer();

        // If pressed space key, activate spikes 
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Scorpion Boss: **Spikes Activated!**");
            ActivateArenaSpikes();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetDifficulty(BossDifficulty.Tutorial);
            Debug.Log("Difficulty set to Tutorial");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetDifficulty(BossDifficulty.Easy);
            Debug.Log("Difficulty set to Easy");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetDifficulty(BossDifficulty.Normal);
            Debug.Log("Difficulty set to Normal");
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
                    int attackChoice = Random.Range(0, 4); // 0 = Charge, 1 = Tail, 2 = Stab, 3 = SpikeDown
                    float playerDistance = Vector3.Distance(transform.position, player.position);

                    switch (attackChoice)
                    {
                        case 0: // Charge Attack (Always allowed)
                            Debug.Log("Scorpion Boss: Preparing **Charge Attack**");
                            StartCoroutine(ChargeAttack());
                            attackChosen = true;
                            break;

                        case 1: // Tail Projectile Attack (Only if player is outside min range)
                            if (playerDistance > tailAttackMinRange)
                            {
                                Debug.Log("Scorpion Boss: Preparing **Tail Projectile Attack**");
                                StartCoroutine(TailProjectileAttack());
                                attackChosen = true;
                            }
                            break;

                        case 2: // Stab Attack (Only if player is within min-max range)
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

                // If no valid attack is found after max attempts, default to Charge Attack
                if (!attackChosen)
                {
                    Debug.Log("Scorpion Boss: doing default attack (Charge)");
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

        cameraShake.SmoothShakeCamera(1f, chargeDuration + 0.5f);

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

        GameObject projectile = Instantiate(projectilePrefab, tailFirePoint.position, Quaternion.identity);
        TailProjectile tailProjectile = projectile.GetComponent<TailProjectile>();

        if (tailProjectile != null)
        {
            // Pass projectileSpeed from ScorpionBoss
            tailProjectile.Initialize(player.position, projectileSpeed);
        }

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
        animator.Play("SmallDash", 0, 0);

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

        cameraShake.SmoothShakeCamera(1.5f, 2f);

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
        cameraShake.SmoothShakeCamera(3, groundSpikesMoveTime + 0.1f);

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

        cameraShake.SmoothShakeCamera(1.5f, groundSpikesMoveTime + 0.1f);

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

        cameraShake.SmoothShakeCamera(0.5f, maxRotationTime);
    }

    private void SpawnWeakpoints()
    {
        for (int i = 0; i < requiredWeakpointsToDestroy; i++)
        {
            WeakpointSlot slot = weakpointSpawnPoints[i];

            GameObject wp = Instantiate(weakpointPrefab, slot.spawnPoint.position, Quaternion.identity);

            // Attach to the spawn point to follow it
            wp.transform.SetParent(slot.spawnPoint, worldPositionStays: true);

            // Apply uniform scale
            wp.transform.localScale = Vector3.one * slot.uniformScale;

            WeakPoint wpScript = wp.GetComponent<WeakPoint>();
            wpScript.onDeath.AddListener(OnWeakpointDestroyed);

            // First weakpoints never expire
            wpScript.DisableLifetime();
        }
    }

    private IEnumerator WaitForWeakpointsDestroyed()
    {
        while (weakpointsDestroyed < requiredWeakpointsToDestroy)
            yield return null;

        Debug.Log("Scorpion Boss: **Weakpoints destroyed!**");
        StartCoroutine(BossAI());
    }

    private void OnWeakpointDestroyed()
    {
        weakpointsDestroyed++;
        Debug.Log("Weakpoint destroyed! Total destroyed: " + weakpointsDestroyed);
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