using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuitarBoss : MonoBehaviour
{
    [System.Serializable]
    private class FirePointSlot
    {
        public Transform firePoint;
        public GameObject visual;
    }

    [Header("References")]
    [SerializeField] private GameObject bodyPartPrefab;
    [SerializeField] private List<FirePointSlot> firePointSlots;
    [SerializeField] private List<OvalPath> availablePaths;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider bossCollider;

    [Header("Encircling Assault")]
    [SerializeField] private float flyingPartMoveSpeed = 5f;
    [SerializeField] private float flyingPartRotationSpeed = 180f;
    [SerializeField] private float flyingPartScaleDuration = 1f;
    [SerializeField] private float flyingPartLifetimeOnPath = 4f;
    [SerializeField] private float flyingPartPathDetectionThreshold = 0.3f;
    [SerializeField] private float delayBetweenLaunches = 0.3f;
    [SerializeField] private float evasiveMoveSpeed = 3f;
    [SerializeField] private float evasiveMoveRadius = 5f;
    [SerializeField] private float timeBetweenRandomMoves = 1.5f;
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Leg Barrage")]
    [SerializeField] private GameObject legProjectilePrefab;
    [SerializeField] private float legProjectileSpeed = 10f;
    [SerializeField] private float legFireInterval = 0.4f;
    [SerializeField] private float legRegrowTime = 2f;
    [SerializeField] private List<FirePointSlot> airborneLegs;

    [Header("Energy Core Attack")]
    [SerializeField] private GameObject energyCorePrefab;
    [SerializeField] private Transform energyCoreSpawnPoint;

    [Header("Energy Core Attack Parameters")]
    [SerializeField] private float energyCoreChargeTime = 3f;
    [SerializeField] private float energyCoreActiveDuration = 4f;
    [SerializeField] private int energyCorePhases = 2;
    [SerializeField] private GameObject energyCoreProjectilePrefab;
    [SerializeField] private float energyCoreProjectileSpeed = 10f;
    [SerializeField] private int energyCoreFirePoints = 32;
    [SerializeField] private float energyCoreFireRadius = 1.5f;
    [SerializeField] private float energyCoreBurstInterval = 0.2f;

    [Header("Difficulty Settings")]
    [SerializeField] [Range(10, 200)] private float tutorialPercentage = 50f;
    [SerializeField] [Range(10, 200)] private float easyPercentage = 75f;

    public enum BossDifficulty { Tutorial, Easy, Normal }

    private BossDifficulty currentDifficulty = BossDifficulty.Normal;

    private bool isAttacking = false;
    private bool isEvading = false;
    private bool isLegAttackActive = false;
    private bool returning = false;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private float evadeTimer = 0f;

    private float baseFlyingPartMoveSpeed;
    private float baseFlyingPartRotationSpeed;
    private float baseFlyingPartScaleDuration;
    private float baseFlyingPartLifetimeOnPath;
    private float baseDelayBetweenLaunches;
    private float baseEvasiveMoveSpeed;
    private float baseEvasiveMoveRadius;
    private float baseTimeBetweenRandomMoves;
    private float baseLegProjectileSpeed;
    private float baseLegFireInterval;
    private float baseLegRegrowTime;
    private float baseCoreChargeTime;
    private float baseCoreActiveDuration;
    private int baseEnergyCorePhases;
    private float baseCoreProjectileSpeed;
    private int baseEnergyCoreFirePoints;
    private float baseCoreBurstInterval;

    private void Start()
    {
        baseFlyingPartMoveSpeed = flyingPartMoveSpeed;
        baseFlyingPartRotationSpeed = flyingPartRotationSpeed;
        baseFlyingPartScaleDuration = flyingPartScaleDuration;
        baseFlyingPartLifetimeOnPath = flyingPartLifetimeOnPath;
        baseDelayBetweenLaunches = delayBetweenLaunches;
        baseEvasiveMoveSpeed = evasiveMoveSpeed;
        baseEvasiveMoveRadius = evasiveMoveRadius;
        baseTimeBetweenRandomMoves = timeBetweenRandomMoves;
        baseLegProjectileSpeed = legProjectileSpeed;
        baseLegFireInterval = legFireInterval;
        baseLegRegrowTime = legRegrowTime;
        baseCoreChargeTime = energyCoreChargeTime;
        baseCoreActiveDuration = energyCoreActiveDuration;
        baseEnergyCorePhases = energyCorePhases;
        baseCoreProjectileSpeed = energyCoreProjectileSpeed;
        baseEnergyCoreFirePoints = energyCoreFirePoints;
        baseCoreBurstInterval = energyCoreBurstInterval;
    }


    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (bossCollider == null) bossCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartFlyingPartsAttack();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartAirborneLegAttack();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartEnergyCoreAttack();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetDifficulty(BossDifficulty.Tutorial);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetDifficulty(BossDifficulty.Easy);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetDifficulty(BossDifficulty.Normal);
        }
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

        flyingPartMoveSpeed = baseFlyingPartMoveSpeed * multiplier;
        flyingPartRotationSpeed = baseFlyingPartRotationSpeed * multiplier;
        flyingPartScaleDuration = baseFlyingPartScaleDuration * inverseMultiplier;
        flyingPartLifetimeOnPath = baseFlyingPartLifetimeOnPath * multiplier;
        delayBetweenLaunches = baseDelayBetweenLaunches * inverseMultiplier;
        evasiveMoveSpeed = baseEvasiveMoveSpeed * multiplier;
        evasiveMoveRadius = baseEvasiveMoveRadius * multiplier;
        timeBetweenRandomMoves = baseTimeBetweenRandomMoves * inverseMultiplier;
        legProjectileSpeed = baseLegProjectileSpeed * multiplier;
        legFireInterval = baseLegFireInterval * inverseMultiplier;
        legRegrowTime = baseLegRegrowTime * inverseMultiplier;
        energyCoreChargeTime = baseCoreChargeTime * inverseMultiplier;
        energyCoreActiveDuration = baseCoreActiveDuration * multiplier;
        energyCorePhases = Mathf.Max(1, Mathf.RoundToInt(baseEnergyCorePhases * multiplier));
        energyCoreProjectileSpeed = baseCoreProjectileSpeed * multiplier;
        energyCoreFirePoints = Mathf.Max(1, Mathf.RoundToInt(baseEnergyCoreFirePoints * multiplier));
        energyCoreBurstInterval = baseCoreBurstInterval * inverseMultiplier;
    }

    // ========== Encircling Assault ==========

    private void StartFlyingPartsAttack()
    {
        if (!isAttacking)
            StartCoroutine(LaunchAndEvadeSequence());
    }

    private IEnumerator LaunchAndEvadeSequence()
    {
        isAttacking = true;
        originalPosition = transform.position;
        StartEvading();

        foreach (FirePointSlot slot in firePointSlots)
        {
            if (availablePaths.Count == 0) break;

            OvalPath chosenPath = availablePaths[Random.Range(0, availablePaths.Count)];

            GameObject part = Instantiate(bodyPartPrefab, slot.firePoint.position, Quaternion.identity);
            FlyingBodyPart flyingScript = part.GetComponent<FlyingBodyPart>();

            flyingScript.Initialize(
                chosenPath,
                slot.firePoint,
                () =>
                {
                    if (slot.visual != null)
                        slot.visual.SetActive(true);
                },
                flyingPartMoveSpeed,
                flyingPartRotationSpeed,
                flyingPartScaleDuration,
                flyingPartLifetimeOnPath,
                flyingPartPathDetectionThreshold,
                slot.visual?.transform
            );

            if (slot.visual != null)
                slot.visual.SetActive(false);

            yield return new WaitForSeconds(delayBetweenLaunches);
        }

        yield return new WaitForSeconds(4f);

        StopEvading();
        isAttacking = false;
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

        rb.linearVelocity = moveDirection * evasiveMoveSpeed;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!isEvading) return;

        if (((1 << collision.gameObject.layer) & wallLayerMask) != 0)
        {
            rb.linearVelocity = Vector3.zero;
            PickNewEvadeTarget();
        }
    }

    // ========== Leg Barrage ==========

    public void StartAirborneLegAttack()
    {
        if (!isLegAttackActive)
            StartCoroutine(FireLegsOnce());
    }

    public void StopAirborneLegAttack()
    {
        isLegAttackActive = false;
    }

    private IEnumerator FireLegsOnce()
    {
        isLegAttackActive = true;

        foreach (var leg in airborneLegs)
        {
            if (leg.visual != null && leg.visual.activeSelf)
            {
                FireLeg(leg);
            }
        }

        yield return new WaitForSeconds(legRegrowTime);

        isLegAttackActive = false;
    }

    private void FireLeg(FirePointSlot leg)
    {
        if (leg.visual != null)
            leg.visual.SetActive(false);

        Quaternion rotation = Quaternion.LookRotation(leg.firePoint.up);
        GameObject proj = Instantiate(legProjectilePrefab, leg.firePoint.position, rotation);

        LegProjectile lp = proj.GetComponent<LegProjectile>();
        if (lp != null)
            lp.SetSpeed(legProjectileSpeed);

        StartCoroutine(RegrowLeg(leg));
    }

    private IEnumerator RegrowLeg(FirePointSlot leg)
    {
        yield return new WaitForSeconds(legRegrowTime);

        if (leg.visual != null)
            leg.visual.SetActive(true);
    }

    // ========== Energy Core Attack ==========

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
                energyCoreBurstInterval
            );
        }
    }
}