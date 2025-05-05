using UnityEngine;
using System.Collections;
using NUnit.Framework.Constraints;

public class ScorpionBoss : MonoBehaviour, BossInterface
{
    public enum BossState { Idle, Charge, TailAttack, StabAttack, SpikeDown }
    public enum BossDifficulty { Tutorial, Easy, Normal }

    [Header("General Settings")]
    private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private PhaseManager phaseManager;

    [Header("Charge Attack Settings")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float wallDetectionDistance = 1f;

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
    [SerializeField] private float extraWeakpointDelay = 5f;
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
    private bool extraSpawnLoopStarted;
    private BossState currentState;
    private bool waitingToSpawnNextWeakpoint;
    private GameObject currentExtraWeakpoint;
    private float baseAnimSpeed;

    [SerializeField] private Transform targetForWeapoints;

    [SerializeField] private DamageBoss health;

    private Coroutine bossAIcoroutine;
    [SerializeField] private Transform model;
    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.Euler(90, 90, 90);
    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        Debug.Log("Change Scorpion phase " + subPhase);
        switch (subPhase)
        {
            case PhaseManager.SubPhase.Tutorial:
                SetDifficulty(BossDifficulty.Tutorial); // Set initial difficulty

                SpawnWeakpoints(); // Spawn weakpoints at the start

                StartCoroutine(WaitForWeakpointsDestroyed()); // Wait for weakpoints to be destroyed before starting AI
                break;
            case PhaseManager.SubPhase.Easy:
                health?.ToggleDamageable(true);
                StopAllCoroutines();
                animator.Play("Idle", 0, 0f);
                SpawnNextWeakpointAfterDelay();
                StartCoroutine(ResetPosition());
                SetDifficulty(BossDifficulty.Easy); // Set initial difficulty
                StartCoroutine(BossAI());
                break;
            case PhaseManager.SubPhase.Normal:
                health?.ToggleDamageable(true);
                StopAllCoroutines();
                animator.Play("Idle", 0, 0f);
                SpawnNextWeakpointAfterDelay();
                StartCoroutine(ResetPosition());
                SetDifficulty(BossDifficulty.Normal);
                StartCoroutine(BossAI());
                break;
        }
    }

    private IEnumerator ResetPosition()
    {
        yield return new WaitForSeconds(0.1f);

        transform.position = startPosition;
        transform.rotation = startRotation;
        Debug.Log(transform.position);
        Debug.Log(transform.rotation);
        model.transform.position = Vector3.zero;
        model.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentState = BossState.Idle;
        isAttacking = false;
    }

    public void PhaseEnded()
    {
        if(bossAIcoroutine != null)
            StopCoroutine(bossAIcoroutine); 
    }

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
        chargeDuration = baseChargeDuration * speedMultiplier;
        chargeWindupTime = baseChargeWindupTime * inverseMultiplier;
        projectileSpeed = baseProjectileSpeed * speedMultiplier;
        attackCooldown = baseAttackCooldown * inverseMultiplier;
        dashSpeed = baseDashSpeed * speedMultiplier;
        dashDuration = baseDashDuration * inverseMultiplier;
        groundSpikesMoveTime = baseGroundSpikesMoveTime * inverseMultiplier;
        groundSpikesUpDuration = baseGroundSpikesUpDuration * inverseMultiplier;
        animator.speed = baseAnimSpeed * speedMultiplier;
    }

    private void Start()
    {
        currentState = BossState.Idle;
        player = FindAnyObjectByType<PlayerMovementQuark>().transform;
        originalGroundY = transform.position.y;
        isSlamImpactTriggered = false;
        isAttacking = false;
        weakpointsDestroyed = 0;
        extraSpawnLoopStarted = false;
        waitingToSpawnNextWeakpoint = false;
        baseAnimSpeed = 1f;

        if(startPosition == Vector3.zero)
            startPosition = transform.position;

        if (startRotation == Quaternion.Euler(90, 90, 90))
            startRotation = transform.rotation;

        Debug.Log("startposition: " + startPosition);
        Debug.Log("startRotation: " + startRotation);

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
        baseAnimSpeed = animator.speed;

        //StartBoss(PhaseManager.SubPhase.Easy);
    }

    private void Update()
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

        animator.CrossFade("Charge", 0.1f);
        phaseManager?.CurrentCamera.GetComponent<CameraShake>().ShakeCamera(0.5f, chargeWindupTime);
        yield return new WaitForSeconds(chargeWindupTime);

        Debug.Log("Scorpion Boss: **Charging forward!**");

        animator.CrossFade("ChargeDash", 0.1f);
        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(1f, chargeDuration + 0.5f);

        float timer = 0f;
        
        // Use direction based on player's position
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Prevent vertical movement

        float rayDistance = 1f;
        LayerMask wallMask = LayerMask.GetMask("Default");

        while (timer < chargeDuration)
        {
            // Detect walls ahead
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance, wallMask))
            {
                Debug.Log("Scorpion Boss: **Hit wall: " + hit.collider.name + "**");
                break;
            }

            transform.position += direction * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        animator.Play("Idle", 0, 0.1f);
        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator TailProjectileAttack()
    {
        isAttacking = true;
        currentState = BossState.TailAttack;

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Scorpion Boss: **Preparing Tail Projectile!**");

        animator.Play("PoisonGrowth", 0 , 0);

        yield return new WaitForSeconds(1f);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    public void FireTailProjectile()
    {
        Debug.Log("Scorpion Boss: **Firing Tail Projectile!**");

        GameObject projectile = Instantiate(projectilePrefab, tailFirePoint.position, Quaternion.identity);
        TailProjectile tailProjectile = projectile.GetComponent<TailProjectile>();

        if (tailProjectile != null)
        {
            tailProjectile.Initialize(player.position, projectileSpeed);
        }
    }

    private IEnumerator StabAttack()
    {
        isAttacking = true;
        currentState = BossState.StabAttack;

        Debug.Log("Scorpion Boss: **Dashing Forward!**");

        phaseManager?.CurrentCamera.GetComponent<CameraShake>().ShakeCamera(0.3f, dashDuration);

        // Dash windup animation
        animator.CrossFade("SmallDash", 0.1f);

        Vector3 dashDirection = player.position - transform.position;
        dashDirection.x = 0;
        dashDirection.y = 0;
        dashDirection.Normalize();

        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector3.zero;

        Debug.Log("Scorpion Boss: **Performing Stab Attack!**");

        // Stab attack animation
        animator.CrossFade("StabDash", 0.1f);

        yield return new WaitForSeconds(1f); // Adjust based on animation length

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator SpikeDownAttack()
    {
        isAttacking = true;
        currentState = BossState.SpikeDown;

        Debug.Log("Scorpion Boss: **Spike Down Attack!**");

        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(1.5f, 2f);

        // Windup animation before slamming
        animator.CrossFade("JumpToAereal", 0.1f);

        Debug.Log("JUMP UP");

        // Move scorpion to high position smoothly
        Vector3 highPos = new Vector3(transform.position.x, transform.position.y, highSpot.position.z);
        while (Vector3.Distance(transform.position, highPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, highPos, 10f * Time.deltaTime);
            yield return null;
        }
        Debug.Log("START LANDING");
        float jumpAnimDuration = GetAnimationClipLength("JumpToAereal");
        yield return new WaitForSeconds(jumpAnimDuration);

        // Slam animation
        animator.CrossFade("AerealStomp", 0.1f);

        // Trigger the slam impact
        float slamAnimDuration = GetAnimationClipLength("AerealStomp");
        yield return new WaitForSeconds(slamAnimDuration * 2f);

        RotateTowardsPlayer();

        Debug.Log("Scorpion Boss: **Returning to ground!**");

        animator.CrossFade("StompToGround", 0.1f);

        Vector3 groundPos = new Vector3(transform.position.x, originalGroundY, transform.position.z);
        while (Vector3.Distance(transform.position, groundPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPos, 20f * Time.deltaTime);
            yield return null;
        }

        float slamImpactDuration = GetAnimationClipLength("StompToGround");
        yield return new WaitForSeconds(slamImpactDuration + 1f);

        Debug.Log("Scorpion Boss: **Landed back on the ground!**");

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
        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(3, groundSpikesMoveTime + 0.1f);

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

        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(1.5f, groundSpikesMoveTime + 0.1f);

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

        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(0.5f, maxRotationTime);
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
            wpScript.SetTarget(targetForWeapoints);

            // First weakpoints never expire
            wpScript.DisableLifetime();
        }
    }

    private IEnumerator WaitForWeakpointsDestroyed()
    {
        while (weakpointsDestroyed < requiredWeakpointsToDestroy)
            yield return null;

        Debug.Log("Scorpion Boss: **Weakpoints destroyed!**");
        // HERE
        health.ChangePhase();
    }

    private void OnWeakpointDestroyed()
    {
        weakpointsDestroyed++;
        Debug.Log("Weakpoint destroyed! Total destroyed: " + weakpointsDestroyed);

        if (weakpointsDestroyed == requiredWeakpointsToDestroy && !extraSpawnLoopStarted)
        {
            extraSpawnLoopStarted = true;
            SpawnNextWeakpointAfterDelay();
        }
        else if (weakpointsDestroyed > requiredWeakpointsToDestroy)
        {
            Debug.Log("Spawning new weakpoints");
            currentExtraWeakpoint = null;

            if (!waitingToSpawnNextWeakpoint)
            {
                SpawnNextWeakpointAfterDelay();
            }
        }
    }

    private void SpawnNextWeakpointAfterDelay()
    {
        StartCoroutine(DelayedWeakpointSpawn());
    }

    private IEnumerator DelayedWeakpointSpawn()
    {
        waitingToSpawnNextWeakpoint = true;
        yield return new WaitForSeconds(extraWeakpointDelay);

        // Check if the current extra weakpoint is still alive
        if (currentExtraWeakpoint == null)
        {
            SpawnRandomWeakpoint();
        }

        waitingToSpawnNextWeakpoint = false;
    }
    
    public void DealWeakpointDamage()
    {
        health.DealDamage(30);
    }
    private void SpawnRandomWeakpoint()
    {
        int randomIndex = Random.Range(0, weakpointSpawnPoints.Length);
        WeakpointSlot slot = weakpointSpawnPoints[randomIndex];

        GameObject wp = Instantiate(weakpointPrefab, slot.spawnPoint.position, Quaternion.identity);
        wp.transform.SetParent(slot.spawnPoint, worldPositionStays: true);
        wp.transform.localScale = Vector3.one * slot.uniformScale;

        WeakPoint wpScript = wp.GetComponent<WeakPoint>();
        //wpScript.onDeath.AddListener(OnWeakpointDestroyed);
        wpScript.onDeath.AddListener(DealWeakpointDamage);
        
        wpScript.SetTarget(targetForWeapoints);

        currentExtraWeakpoint = wp;
    }

    private float GetAnimationClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"Animation '{clipName}' not found!");
        return 0f;
    }

    public void SmallCameraShake()
    {
        phaseManager?.CurrentCamera.GetComponent<CameraShake>().ShakeCamera(0.5f, 0.5f);
    }

    // Draw the range of the attacks in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Existing range gizmos
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tailAttackMinRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stabAttackMinRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stabAttackMaxRange);

        // Raycast for charge direction (always forward)
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, -transform.forward * wallDetectionDistance);
    }
}