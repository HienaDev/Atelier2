using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuitarBoss : MonoBehaviour, BossInterface
{
    public enum BossState { Idle, EncirclingAssault, LegBarrage, EnergyCoreAttack }
    public enum BossDifficulty { Tutorial, Easy, Normal }

    [System.Serializable]
    private class FirePointSlot
    {
        public Transform firePoint;
        public GameObject visual;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider bossCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private DamageBoss health;
    [SerializeField] private PhaseManager phaseManager;

    [Header("Boss Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;

    [Header("Encircling Assault")]
    [SerializeField] private GameObject bodyPartPrefab;
    [SerializeField] private List<FirePointSlot> firePointSlots;
    [SerializeField] private List<OvalPath> availablePaths;
    [SerializeField] private float flyingPartMoveSpeed = 5f;
    [SerializeField] private float flyingPartRotationSpeed = 180f;
    [SerializeField] private float flyingPartScaleDuration = 1f;
    [SerializeField] private float flyingPartLifetimeOnPath = 4f;
    [SerializeField] private float flyingPartPathDetectionThreshold = 0.3f;
    [SerializeField] private float flyingPartHomingCount = 1f;
    [SerializeField] private float flyingPartHomingSpeed = 15f;
    [SerializeField] private float delayBetweenLaunches = 0.3f;
    [SerializeField] private float evasiveMoveSpeed = 3f;
    [SerializeField] private float evasiveMoveRadius = 5f;
    [SerializeField] private float weakpointPartSpeedMultiplier = 0.5f;
    [SerializeField] private float timeBetweenRandomMoves = 1.5f;

    [Header("Leg Barrage")]
    [SerializeField] private GameObject legProjectilePrefab;
    [SerializeField] private float legProjectileSpeed = 10f;
    [SerializeField] private float legRegrowTime = 2f;
    [SerializeField] private float legTimeBetweenLaunches = 1.5f;
    [SerializeField] private List<FirePointSlot> airborneLegs;
    [SerializeField] private float legAttackDuration = 5f;

    [Header("Energy Core Attack")]
    [SerializeField] private GameObject energyCorePrefab;
    [SerializeField] private Transform energyCoreSpawnPoint;
    [SerializeField] private float energyCoreChargeTime = 3f;
    [SerializeField] private float energyCoreActiveDuration = 4f;
    [SerializeField] private float energyCoreLingerTime = 2f;
    [SerializeField] private int energyCorePhases = 2;
    [SerializeField] private GameObject energyCoreProjectilePrefab;
    [SerializeField] private float energyCoreProjectileSpeed = 10f;
    [SerializeField] private int energyCoreFirePoints = 32;
    [SerializeField] private float energyCoreFireRadius = 1.5f;
    [SerializeField] private float energyCoreBurstInterval = 0.2f;
    [SerializeField] private float energyCoreAttackDuration = 8f;

    [Header("Weakpoint Settings")]
    [SerializeField] private GameObject weakpointPrefab;
    [SerializeField] private GameObject energyCoreWeakpointPrefab;
    [SerializeField] private GameObject encirlingAssaultWeakpointPrefab;
    [SerializeField] private WeakpointSlot[] weakpointSpawnPoints;
    [SerializeField] private int requiredWeakpointsToDestroy = 2;
    [SerializeField] private float extraWeakpointDelay = 5f;
    [SerializeField] private Transform targetForWeakpoints;

    [SerializeField] private ClearProjectiles clearProjectiles; 

    [System.Serializable]
    public struct WeakpointSlot
    {
        public Transform spawnPoint;
        [Min(0.01f)] public float uniformScale; // single value for X, Y, Z
    }

    [Header("Difficulty Settings")]
    [SerializeField][Range(10, 200)] private float tutorialPercentage = 50f;
    [SerializeField][Range(10, 200)] private float easyPercentage = 75f;

    private BossDifficulty currentDifficulty = BossDifficulty.Normal;
    private bool isAttacking = false;
    private bool isEvading = false;
    private bool isLegAttackActive = false;
    private bool returning = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private float evadeTimer = 0f;
    private BossState currentState = BossState.Idle;
    private int weakpointsDestroyed = 0;
    private bool extraSpawnLoopStarted = false;
    private bool waitingToSpawnNextWeakpoint = false;
    private GameObject currentExtraWeakpoint;
    private Coroutine bossAICoroutine;
    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;
    private GameObject currentCoreWeakpoint;
    private GameObject currentPartWeakpoint;
    private List<GameObject> activeParts = new List<GameObject>();

    // Base value storage for difficulty scaling
    private float baseFlyingPartMoveSpeed;
    private float baseFlyingPartRotationSpeed;
    private float baseFlyingPartScaleDuration;
    private float baseFlyingPartLifetimeOnPath;
    private float baseFlyingPartHomingCount;
    private float baseFlyingPartHomingSpeed;
    private float baseDelayBetweenLaunches;
    private float baseEvasiveMoveSpeed;
    private float baseEvasiveMoveRadius;
    private float baseTimeBetweenRandomMoves;
    private float baseLegProjectileSpeed;
    private float baseLegRegrowTime;
    private float baseTimeBetweenLegLaunches;
    private float baseCoreChargeTime;
    private float baseCoreActiveDuration;
    private int baseEnergyCorePhases;
    private float baseCoreProjectileSpeed;
    private int baseEnergyCoreFirePoints;
    private float baseCoreBurstInterval;
    private float baseAttackCooldown;
    private float baseAnimSpeed;
    private float baseLegAttackDuration;
    private float baseEnergyCoreAttackDuration;
    private float baseEnergyCoreLingerTime;

    [SerializeField] private Transform[] critPositions;
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (bossCollider == null) bossCollider = GetComponent<Collider>();
        if (player == null) player = FindAnyObjectByType<PlayerMovementQuark>()?.transform;
        if (animator == null) animator = GetComponent<Animator>();

        // Store original position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnDisable()
    {
        foreach (GameObject part in activeParts)
        {
            if (part != null)
                Destroy(part);
        }
        activeParts.Clear();
    }

    private void Start()
    {
        currentState = BossState.Idle;
        weakpointsDestroyed = 0;
        extraSpawnLoopStarted = false;
        waitingToSpawnNextWeakpoint = false;

        // Store base values for difficulty scaling
        StoreBaseValues();

        // Placeholder: Wait for weakpoints to be destroyed before starting AI
        //SpawnWeakpoints();
        //StartCoroutine(WaitForWeakpointsDestroyed());
    }

    private void StoreBaseValues()
    {
        baseFlyingPartMoveSpeed = flyingPartMoveSpeed;
        baseFlyingPartRotationSpeed = flyingPartRotationSpeed;
        baseFlyingPartScaleDuration = flyingPartScaleDuration;
        baseFlyingPartLifetimeOnPath = flyingPartLifetimeOnPath;
        baseFlyingPartHomingCount = flyingPartHomingCount;
        baseFlyingPartHomingSpeed = flyingPartHomingSpeed;
        baseDelayBetweenLaunches = delayBetweenLaunches;
        baseEvasiveMoveSpeed = evasiveMoveSpeed;
        baseEvasiveMoveRadius = evasiveMoveRadius;
        baseTimeBetweenRandomMoves = timeBetweenRandomMoves;
        baseLegProjectileSpeed = legProjectileSpeed;
        baseLegRegrowTime = legRegrowTime;
        baseTimeBetweenLegLaunches = legTimeBetweenLaunches;
        baseCoreChargeTime = energyCoreChargeTime;
        baseCoreActiveDuration = energyCoreActiveDuration;
        baseEnergyCorePhases = energyCorePhases;
        baseCoreProjectileSpeed = energyCoreProjectileSpeed;
        baseEnergyCoreFirePoints = energyCoreFirePoints;
        baseCoreBurstInterval = energyCoreBurstInterval;
        baseAttackCooldown = attackCooldown;
        baseAnimSpeed = animator ? animator.speed : 1f;
        baseLegAttackDuration = legAttackDuration;
        baseEnergyCoreAttackDuration = energyCoreAttackDuration;
        baseEnergyCoreLingerTime = energyCoreLingerTime; 
    }

    private void FixedUpdate()
    {
        if (isEvading)
        {
            EvadeMovement();
        }
        else if (returning)
        {
            ReturnToOriginalPositionMovement();
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void SetDifficulty(BossDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        ApplyDifficultySettings();
    }

    private void ApplyDifficultySettings()
    {
        float percent = currentDifficulty switch
        {
            BossDifficulty.Tutorial => tutorialPercentage,
            BossDifficulty.Easy => easyPercentage,
            _ => 100f,
        };

        float multiplier = percent / 100f;
        float inverseMultiplier = 200f / (percent + 100f);

        // Apply difficulty adjustments
        flyingPartMoveSpeed = baseFlyingPartMoveSpeed * multiplier;
        flyingPartRotationSpeed = baseFlyingPartRotationSpeed * multiplier;
        flyingPartScaleDuration = baseFlyingPartScaleDuration * inverseMultiplier;
        flyingPartLifetimeOnPath = baseFlyingPartLifetimeOnPath * multiplier;
        flyingPartHomingCount = baseFlyingPartHomingCount * multiplier;
        flyingPartHomingCount = Mathf.Max(0.1f, flyingPartHomingCount);
        flyingPartHomingSpeed = baseFlyingPartHomingSpeed * multiplier;
        delayBetweenLaunches = baseDelayBetweenLaunches * inverseMultiplier;
        evasiveMoveSpeed = baseEvasiveMoveSpeed * multiplier;
        evasiveMoveRadius = baseEvasiveMoveRadius * multiplier;
        timeBetweenRandomMoves = baseTimeBetweenRandomMoves * inverseMultiplier;
        legProjectileSpeed = baseLegProjectileSpeed * multiplier;
        legRegrowTime = baseLegRegrowTime * inverseMultiplier;
        legTimeBetweenLaunches = baseTimeBetweenLegLaunches * inverseMultiplier;
        energyCoreChargeTime = baseCoreChargeTime * inverseMultiplier;
        energyCoreActiveDuration = baseCoreActiveDuration * multiplier;
        energyCorePhases = Mathf.Max(1, Mathf.RoundToInt(baseEnergyCorePhases * multiplier));
        energyCoreProjectileSpeed = baseCoreProjectileSpeed * multiplier;
        energyCoreFirePoints = Mathf.Max(1, Mathf.RoundToInt(baseEnergyCoreFirePoints * multiplier));
        energyCoreBurstInterval = baseCoreBurstInterval * inverseMultiplier;
        attackCooldown = baseAttackCooldown * inverseMultiplier;
        legAttackDuration = baseLegAttackDuration * multiplier;
        energyCoreAttackDuration = baseEnergyCoreAttackDuration * multiplier;
        energyCoreLingerTime = baseEnergyCoreLingerTime * inverseMultiplier;

        if (animator != null)
        {
            animator.speed = baseAnimSpeed * multiplier;
        }
    }

    // ====================== BossInterface Implementation ======================
    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        Debug.Log("Guitar Boss: Starting phase " + subPhase);
        
        // First, make sure we end any previous phase properly
        PhaseEnded();
        
        // Then set up the new phase
        switch (subPhase)
        {
            case PhaseManager.SubPhase.Tutorial:
                SetDifficulty(BossDifficulty.Tutorial);
                SpawnWeakpoints();
                weakpointsDestroyed = 0;
                extraSpawnLoopStarted = false;
                waitingToSpawnNextWeakpoint = false;
                StartCoroutine(WaitForWeakpointsDestroyed());
                break;
                
            case PhaseManager.SubPhase.Easy:
                health?.ToggleDamageable(true);
                SetDifficulty(BossDifficulty.Easy);
                SpawnNextWeakpointAfterDelay();
                StartCoroutine(StartBossAIWithDelay(attackCooldown));
                break;
                
            case PhaseManager.SubPhase.Normal:
                health?.ToggleDamageable(true);
                SetDifficulty(BossDifficulty.Normal);
                SpawnNextWeakpointAfterDelay();
                StartCoroutine(StartBossAIWithDelay(attackCooldown));
                break;
        }
    }

    private IEnumerator StartBossAIWithDelay(float delay)
    {
        currentState = BossState.Idle;
        animator?.CrossFade("Guitar_idle", 0.2f);
        
        yield return new WaitForSeconds(delay);
        
        bossAICoroutine = StartCoroutine(BossAI());
    }

    public void PhaseEnded()
    {
        Debug.Log("Guitar Boss: Phase ended - performing full reset");
        
        if (bossAICoroutine != null)
        {
            StopCoroutine(bossAICoroutine);
            bossAICoroutine = null;
        }
        
        StopAllCoroutines();
        
        isAttacking = false;
        isEvading = false;
        isLegAttackActive = false;
        returning = false;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        if (clearProjectiles != null)
        {
            clearProjectiles.ClearAllProjectiles();
        }
        
        foreach (var slot in firePointSlots)
        {
            if (slot.visual != null)
                slot.visual.SetActive(true);
        }
        
        foreach (var leg in airborneLegs)
        {
            if (leg.visual != null)
            {
                leg.visual.SetActive(true);
                leg.visual.transform.localScale = leg.visual.transform.localScale;
            }
        }
        
        if (animator != null && animator.enabled)
        {
            animator.enabled = true;
            animator.CrossFade("Guitar_idle", 0.1f);
        }
        
        currentState = BossState.Idle;
        
        if (animator != null)
        {
            animator.enabled = true;
            animator.CrossFade("Guitar_idle", 0.1f);
        }
    }

    // ====================== Boss AI Logic ======================
    private IEnumerator BossAI()
    {
        Debug.Log("Guitar Boss: Starting AI behavior");

        yield return StartCoroutine(EncirclingAssaultSequence());
        yield return new WaitForSeconds(attackCooldown);

        yield return StartCoroutine(LegBarrageSequence());
        yield return new WaitForSeconds(attackCooldown);

        yield return StartCoroutine(EnergyCoreSequence());
        yield return new WaitForSeconds(attackCooldown);

        while (true)
        {
            if (!isAttacking)
            {
                BossState[] availableAttacks = new BossState[]
                {
                    BossState.EncirclingAssault,
                    BossState.LegBarrage,
                    BossState.EnergyCoreAttack
                };

                BossState chosenAttack = availableAttacks[Random.Range(0, availableAttacks.Length)];

                switch (chosenAttack)
                {
                    case BossState.EncirclingAssault:
                        Debug.Log("Guitar Boss: Preparing **Encircling Assault**");
                        yield return StartCoroutine(EncirclingAssaultSequence());
                        break;

                    case BossState.LegBarrage:
                        Debug.Log("Guitar Boss: Preparing **Leg Barrage**");
                        yield return StartCoroutine(LegBarrageSequence());
                        break;

                    case BossState.EnergyCoreAttack:
                        Debug.Log("Guitar Boss: Preparing **Energy Core Attack**");
                        yield return StartCoroutine(EnergyCoreSequence());
                        break;
                }

                yield return new WaitForSeconds(attackCooldown);
            }

            yield return null;
        }
    }

    private IEnumerator EncirclingAssaultSequence()
    {
        isAttacking = true;
        currentState = BossState.EncirclingAssault;

        animator?.CrossFade("Guitar_Assault", 0.2f);

        StartFlyingPartsAttack();

        // Wait for the animation to finish
        yield return new WaitForSeconds(GetAnimationClipLength("Guitar_Assault"));

        // Turn off animator
        animator.enabled = false;

        float launchTime = delayBetweenLaunches * firePointSlots.Count;
        float orbitTime = flyingPartLifetimeOnPath;
        float returnTime = evasiveMoveRadius / flyingPartMoveSpeed;
        float returnToPositionTime = evasiveMoveRadius / evasiveMoveSpeed - timeBetweenRandomMoves;

        float totalDuration = launchTime + orbitTime + returnTime + returnToPositionTime;
        yield return new WaitForSeconds(totalDuration);
        Debug.Log("Guitar Boss: **Encircling Assault** finished");

        // Turn animator back on
        animator.enabled = true;

        animator?.CrossFade("Guitar_idle", 0.2f);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator LegBarrageSequence()
    {
        isAttacking = true;
        currentState = BossState.LegBarrage;

        // Play animation
        animator?.CrossFade("Guitar_LegBarrage", 0.2f);

        yield return new WaitForSeconds(0.2f);

        StartAirborneLegAttack();

        yield return new WaitForSeconds(GetAnimationClipLength("Guitar_LegBarrage"));

        StopAirborneLegAttack();

        currentState = BossState.Idle;
        isAttacking = false;
    }

    private IEnumerator EnergyCoreSequence()
    {
        isAttacking = true;
        currentState = BossState.EnergyCoreAttack;

        // Play animation
        animator?.CrossFade("Guitar_Core", 0.2f);
        yield return new WaitForSeconds(GetAnimationClipLength("Guitar_Core") + 1f);

        animator.enabled = false;

        // Wait for the core to stop charging
        yield return new WaitForSeconds(energyCoreAttackDuration - energyCoreActiveDuration);

        animator.enabled = true;

        animator?.CrossFade("Guitar_CoreToIdle", 0.2f);

        yield return new WaitForSeconds(GetAnimationClipLength("Guitar_CoreToIdle"));

        animator?.CrossFade("Guitar_idle", 0.2f);

        currentState = BossState.Idle;
        isAttacking = false;
    }

    // ====================== Encircling Assault Logic ======================
    private void StartFlyingPartsAttack()
    {
        StartCoroutine(LaunchAndEvadeSequence());
    }

    private IEnumerator LaunchAndEvadeSequence()
    {
        isAttacking = true;
        originalPosition = transform.position;
        StartEvading();

        List<GameObject> launchedParts = new List<GameObject>();
        int nHoming = Mathf.Max(1, Mathf.RoundToInt(flyingPartHomingCount));
        float[] homingTimes = new float[nHoming];
        for (int j = 0; j < nHoming; j++)
            homingTimes[j] = flyingPartLifetimeOnPath * ((j + 1f) / (nHoming + 1f));

        int randomPartIndex = Random.Range(0, firePointSlots.Count);

        for (int i = 0; i < firePointSlots.Count; i++)
        {
            var slot = firePointSlots[i];
            if (availablePaths.Count == 0) break;

            OvalPath chosenPath = availablePaths[Random.Range(0, availablePaths.Count)];
            GameObject part = Instantiate(bodyPartPrefab, slot.firePoint.position, Quaternion.identity);
            part.transform.SetParent(transform);
            activeParts.Add(part);
            clearProjectiles?.AddProjectile(part);
            FlyingBodyPart flyingScript = part.GetComponent<FlyingBodyPart>();

            GameObject flyingVisual = Instantiate(slot.visual);
            flyingVisual.transform.SetParent(part.transform, worldPositionStays: false);
            flyingVisual.transform.localPosition = Vector3.zero;
            flyingVisual.transform.localRotation = Quaternion.identity;
            flyingVisual.transform.localScale = slot.visual.transform.localScale;

            launchedParts.Add(part);

            bool isWeakpointPart = (i == randomPartIndex);
            float partMoveSpeed = isWeakpointPart ? 
                flyingPartMoveSpeed * weakpointPartSpeedMultiplier : 
                flyingPartMoveSpeed;
            
            float partRotationSpeed = isWeakpointPart ? 
                flyingPartRotationSpeed * weakpointPartSpeedMultiplier : 
                flyingPartRotationSpeed;

            if (isWeakpointPart)
            {
                SpawnPartWeakpoint(part);
                flyingScript.SetHasWeakpoint(true);
            }

            flyingScript.Initialize(
                chosenPath,
                slot.firePoint,
                () =>
                {
                    if (slot.visual != null)
                        slot.visual.SetActive(true);
                    Destroy(part);
                },
                partMoveSpeed,
                partRotationSpeed,
                flyingPartScaleDuration,
                flyingPartLifetimeOnPath,
                flyingPartPathDetectionThreshold,
                flyingVisual.transform,
                player,
                homingTimes,
                false,
                flyingPartHomingSpeed
            );

            if (slot.visual != null)
                slot.visual.SetActive(false);

            phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(2.3f, 0.1f);

            yield return new WaitForSeconds(delayBetweenLaunches);
        }

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(MonitorHomingActivation(launchedParts, homingTimes));

        phaseManager?.CurrentCamera.GetComponent<CameraShake>().SmoothShakeCamera(0.6f, flyingPartLifetimeOnPath);

        yield return new WaitForSeconds(flyingPartLifetimeOnPath);
        
        foreach (GameObject part in launchedParts)
        {
            if (part != null)
            {
                FlyingBodyPart flyingScript = part.GetComponent<FlyingBodyPart>();
                if (flyingScript != null)
                {
                    flyingScript.ForceReturn();
                }
            }
        }
        
        StopEvading();

        if (Vector3.Distance(transform.position, originalPosition) < 0.2f)
        {
            rb.linearVelocity = Vector3.zero;
            returning = false;

            currentState = BossState.Idle;
            isAttacking = false;
        }
    }

    private IEnumerator MonitorHomingActivation(List<GameObject> launchedParts, float[] homingTimes)
    {
        int homingIndex = 0;
        float timer = 0f;

        while (homingIndex < homingTimes.Length && timer < flyingPartLifetimeOnPath)
        {
            timer += Time.deltaTime;

            if (timer >= homingTimes[homingIndex])
            {
                List<GameObject> eligibleForHoming = new List<GameObject>();
                foreach (GameObject part in launchedParts)
                {
                    if (part != null)
                    {
                        FlyingBodyPart flyingScript = part.GetComponent<FlyingBodyPart>();
                        if (flyingScript != null && flyingScript.IsOrbiting() && !flyingScript.HasWeakpoint())
                        {
                            eligibleForHoming.Add(part);
                        }
                    }
                }

                GameObject farthestPart = GetFarthestPartFromPlayer(eligibleForHoming);
                if (farthestPart != null)
                {
                    FlyingBodyPart farthestScript = farthestPart.GetComponent<FlyingBodyPart>();
                    if (farthestScript != null)
                    {
                        float[] singleHomingTime = { 0f };
                        farthestScript.EnableHoming(singleHomingTime);
                    }
                }

                homingIndex++;
            }

            yield return null;
        }
    }

    private GameObject GetFarthestPartFromPlayer(List<GameObject> parts)
    {
        if (player == null || parts.Count == 0) return null;

        GameObject farthest = null;
        float maxDistance = 0f;

        foreach (GameObject part in parts)
        {
            if (part != null)
            {
                FlyingBodyPart flyingScript = part.GetComponent<FlyingBodyPart>();
                if (flyingScript != null && flyingScript.IsOrbiting())
                {
                    float distance = Vector3.Distance(part.transform.position, player.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthest = part;
                    }
                }
            }
        }

        return farthest;
    }

    private void StartEvading()
    {
        isEvading = true;
        returning = false;
        PickNewEvadeTarget();
    }

    private void StopEvading()
    {
        isEvading = false;
        returning = true;
    }

    private void EvadeMovement()
    {
        evadeTimer += Time.fixedDeltaTime;

        Vector3 move = new Vector3(0f, moveDirection.y, moveDirection.z).normalized;
        rb.linearVelocity = move * evasiveMoveSpeed;

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f || evadeTimer >= timeBetweenRandomMoves)
        {
            PickNewEvadeTarget();
        }
    }

    private void ReturnToOriginalPositionMovement()
    {
        Vector3 returnDirection = (originalPosition - transform.position).normalized;
        rb.linearVelocity = returnDirection * evasiveMoveSpeed;

        if (Vector3.Distance(transform.position, originalPosition) < 0.2f)
        {
            rb.linearVelocity = Vector3.zero;
            returning = false;
        }
    }

    private void PickNewEvadeTarget()
    {
        evadeTimer = 0f;
        Vector3 randomDirection = (transform.position - player.position).normalized;
        randomDirection += new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.3f, 0.3f), Random.Range(-0.7f, 0.7f));
        randomDirection.Normalize();
        moveDirection = randomDirection;
        targetPosition = transform.position + randomDirection * evasiveMoveRadius;
    }

    // ====================== Leg Barrage Logic ======================
    public void StartAirborneLegAttack()
    {
        if (!isLegAttackActive)
        {
            StartCoroutine(FireLegsSequence());
        }
    }

    public void StopAirborneLegAttack()
    {
        animator?.CrossFade("Guitar_idle", 0.2f);
        isLegAttackActive = false;
    }

    private IEnumerator FireLegsSequence()
    {
        isLegAttackActive = true;
        while (isLegAttackActive)
        {
            foreach (var leg in airborneLegs)
            {
                if (leg.visual != null && leg.visual.activeSelf)
                {
                    FireLeg(leg);
                }
            }
            yield return new WaitForSeconds(legTimeBetweenLaunches);
        }
    }

    private void FireLeg(FirePointSlot leg)
    {
        if (leg.visual != null)
            leg.visual.SetActive(false);
        
        Vector3 forward = new Vector3(0, leg.firePoint.forward.y, leg.firePoint.forward.z).normalized;
        
        if (forward.magnitude < 0.01f)
        {
            forward = Vector3.forward;
        }
        
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        
        // Instantiate the projectile at the fire point position
        GameObject proj = Instantiate(legProjectilePrefab, leg.firePoint.position, rotation);
        clearProjectiles?.AddProjectile(proj);
        LegProjectile lp = proj.GetComponent<LegProjectile>();
        if (lp != null)
            lp.SetSpeed(legProjectileSpeed);

        phaseManager?.CurrentCamera.GetComponent<CameraShake>()?.SmoothShakeCamera(1.3f, 0.15f);
        
        StartCoroutine(RegrowLeg(leg));
    }

    private IEnumerator RegrowLeg(FirePointSlot leg)
    {
        yield return new WaitForSeconds(legTimeBetweenLaunches - 0.3f);
        
        if (leg.visual != null)
        {
            leg.visual.SetActive(true);
            Vector3 originalScale = leg.visual.transform.localScale;
            leg.visual.transform.localScale = Vector3.zero;
            float elapsedTime = 0f;
            while (elapsedTime < legRegrowTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / legRegrowTime);
                leg.visual.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
                yield return null;
            }
        }
    }

    // ====================== Energy Core Attack Logic ======================
    public void StartEnergyCoreAttack()
    {
        if (energyCorePrefab == null || energyCoreSpawnPoint == null) return;

        GameObject core = Instantiate(energyCorePrefab, energyCoreSpawnPoint.position, Quaternion.identity);
        EnergyCore energyCore = core.GetComponent<EnergyCore>();

        if (energyCore != null)
        {
            energyCore.Initialize(
                energyCoreChargeTime,
                energyCoreActiveDuration,
                energyCorePhases,
                energyCoreProjectilePrefab,
                energyCoreProjectileSpeed,
                energyCoreFirePoints,
                energyCoreFireRadius,
                energyCoreBurstInterval,
                energyCoreLingerTime
            );
        }

        SpawnCoreWeakpoint(core);
        StartCoroutine(EnergyCoreShake());
    }

    private IEnumerator EnergyCoreShake()
    {
        phaseManager?.CurrentCamera.GetComponent<CameraShake>()?.SmoothShakeCamera(1.5f, energyCoreChargeTime);

        yield return new WaitForSeconds(energyCoreChargeTime);

        phaseManager?.CurrentCamera.GetComponent<CameraShake>()?.SmoothShakeCamera(0.8f, energyCoreAttackDuration);
    }

    // ====================== Weakpoint Logic ======================
    private void SpawnWeakpoints()
    {
        for (int i = 0; i < requiredWeakpointsToDestroy; i++)
        {
            if (i >= weakpointSpawnPoints.Length) break;

            WeakpointSlot slot = weakpointSpawnPoints[i];
            GameObject wp = Instantiate(weakpointPrefab, slot.spawnPoint.position, Quaternion.identity);

            // Attach to the spawn point to follow it
            wp.transform.SetParent(slot.spawnPoint, worldPositionStays: true);

            // Apply uniform scale
            wp.transform.localScale = Vector3.one * slot.uniformScale;

            WeakPoint wpScript = wp.GetComponent<WeakPoint>();
            wpScript.onDeath.AddListener(OnWeakpointDestroyed);


            if (targetForWeakpoints != null)
                wpScript.SetTarget(targetForWeakpoints);

            // First weakpoints never expire
            wpScript.DisableLifetime();
        }
    }

    private void SpawnCoreWeakpoint(GameObject core)
    {
        if (energyCoreWeakpointPrefab == null) return;

        GameObject wp = Instantiate(energyCoreWeakpointPrefab, core.transform.position, Quaternion.identity);
        wp.transform.SetParent(core.transform, worldPositionStays: true);

        WeakPoint wpScript = wp.GetComponent<WeakPoint>();
        wpScript.onDeath.AddListener(() => health.DealCritDamage());
        wpScript.onDeath.AddListener(() => {
            currentCoreWeakpoint = null;
            OnWeakpointDestroyed();
        });
        wpScript.SetCritPositions(critPositions);

        if (targetForWeakpoints != null)
            wpScript.SetTarget(targetForWeakpoints);

        currentCoreWeakpoint = wp;
    }

    private void SpawnPartWeakpoint(GameObject part)
    {
        if (encirlingAssaultWeakpointPrefab == null) return;

        Transform centerRef = part.GetComponentInChildren<MeshRenderer>()?.transform;

        if (centerRef == null)
        {
            Debug.LogWarning("SpawnPartWeakpoint: MeshRenderer não encontrado. Usando part.position.");
            centerRef = part.transform;
        }

        GameObject wp = Instantiate(encirlingAssaultWeakpointPrefab, centerRef.position, Quaternion.identity);
        wp.transform.SetParent(part.transform, worldPositionStays: true);
        clearProjectiles.AddProjectile(wp);

        WeakPoint wpScript = wp.GetComponent<WeakPoint>();
        wpScript.onDeath.AddListener(() => health.DealCritDamage());
        wpScript.onDeath.AddListener(() => {
            currentPartWeakpoint = null;
            OnWeakpointDestroyed();
        });
        wpScript.SetCritPositions(critPositions);

        if (targetForWeakpoints != null)
            wpScript.SetTarget(targetForWeakpoints);

        currentPartWeakpoint = wp;
    }

    private IEnumerator WaitForWeakpointsDestroyed()
    {
        while (weakpointsDestroyed < requiredWeakpointsToDestroy)
            yield return null;

        Debug.Log("Guitar Boss: **Weakpoints destroyed!**");

        health?.ChangePhase();
    }

    private void OnWeakpointDestroyed()
    {
        weakpointsDestroyed++;
        Debug.Log("Weakpoint destroyed! Total destroyed: " + weakpointsDestroyed);

        phaseManager?.CurrentCamera.GetComponent<CameraShake>()?.SmoothShakeCamera(1.8f, 0.3f);

        if (weakpointsDestroyed == requiredWeakpointsToDestroy && !extraSpawnLoopStarted)
        {
            extraSpawnLoopStarted = true;
            SpawnNextWeakpointAfterDelay();
        }
        else if (weakpointsDestroyed > requiredWeakpointsToDestroy)
        {
            Debug.Log("Spawning new weakpoints");
            currentExtraWeakpoint = null;

            SpawnNextWeakpointAfterDelay();
        }
    }

    private void SpawnNextWeakpointAfterDelay()
    {
        if (!gameObject.activeInHierarchy) return;
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
        health?.DealDamage(30);
    }

    private void SpawnRandomWeakpoint()
    {
        if (weakpointSpawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, weakpointSpawnPoints.Length);
        WeakpointSlot slot = weakpointSpawnPoints[randomIndex];

        GameObject wp = Instantiate(weakpointPrefab, slot.spawnPoint.position, Quaternion.identity);
        wp.transform.SetParent(slot.spawnPoint, worldPositionStays: true);
        wp.transform.localScale = Vector3.one * slot.uniformScale;

        WeakPoint wpScript = wp.GetComponent<WeakPoint>();
        wpScript.onDeath.AddListener(() => health.DealCritDamage());
        wpScript.onDeath.AddListener(OnWeakpointDestroyed);
        wpScript.SetCritPositions(critPositions);

        if (targetForWeakpoints != null)
            wpScript.SetTarget(targetForWeakpoints);

        currentExtraWeakpoint = wp;
    }

    // ====================== Utility Functions ======================
    private float GetAnimationClipLength(string clipName)
    {
        if (animator == null) return 1f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"Animation '{clipName}' not found! Using default duration.");
        return 1f;
    }
}