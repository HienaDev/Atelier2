using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PhaseManager;
using NaughtyAttributes;
using Unity.VisualScripting;

public class DJBoss : MonoBehaviour, BossInterface
{
    [Serializable]
    public struct Collumn
    {
        public GameObject collumn;
        public ScaleWithMusic scaleWithMusic;
        public SpeakerEffects speakerEffects;
        public int index;
        public Transform firePoint;
    }

    [Serializable]
    public enum AttackType
    {
        None,
        ColumnAttack,
        WallAttack,
        LaserAttack
    }

    [Serializable]
    public struct AttackCommand
    {
        public AttackType attackType;
        public List<int> targetColumns; // Which columns to attack from
        [Range(0f, 2f)]
        public float delay; // Delay before this specific attack
        [Range(1, 10)]
        public int numberOfShots; // Number of spike shots to fire from each column
        [Range(0f, 5f)]
        public float delayBetweenCommands; // New: Delay after this command before next one
    }

    [Serializable]
    public struct AttackPattern
    {
        [Tooltip("Name of this attack pattern")]
        public string patternName;

        [Tooltip("List of attack commands to execute")]
        public List<AttackCommand> attacks;

        [Tooltip("Delay between each shot")]
        public float patternDelay;

        [Tooltip("Current attack command index")]
        public int currentCommandIndex;
    }

    [Header("Attack Patterns")]
    [SerializeField] private List<AttackPattern> attackPatterns = new List<AttackPattern>();
    [SerializeField] private List<AttackPattern> easyAttackPatterns = new List<AttackPattern>();
    [SerializeField] private List<AttackPattern> normalAttackPatterns = new List<AttackPattern>();
    private List<AttackPattern> currentAttackPatterns = new List<AttackPattern>();
    [SerializeField] private int currentPatternIndex = 0;
    [SerializeField] private bool usePatterns = false; // Toggle between pattern mode and random mode

    [SerializeField] private int numberOfWeakspointsToDestroy = 2;
    private int tutorialWeakpointsDestroyed = 0;

    [SerializeField] private Collumn[] collumns;

    private bool normalDifficulty = false;

    [Header("Weakpoint")]
    [SerializeField] private GameObject weakpoint;

    [SerializeField, Range(0f, 1f)] private float doubleSlamChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float doubleSlamChanceNormal = 0.35f;

    [Header("Spike Attacks 1")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int[] projectileCount = { 3, 4, 5 };
    private int currentProjectileCount = 0;
    [SerializeField] private float attackArcAngle = 60f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileScale = 0.7f;
    [SerializeField] private float projectileDelay = 0f;

    [Header("Spike Attacks Normal Difficulty")]
    [SerializeField] private int[] projectileCountNormal = { 3, 4, 5 };
    private int currentProjectileCountNormal = 0;
    [SerializeField] private float attackArcAngleNormal = 60f;
    [SerializeField] private float projectileSpeedNormal = 10f;
    [SerializeField] private float projectileScaleNormal = 0.7f;
    [SerializeField] private float projectileDelayNormal = 0f;

    [SerializeField] private GameObject doubleSlamText;
    [SerializeField] private Transform doubleSlamSpawn;

    [Header("Wall Attack")]
    [SerializeField] private GameObject wallProjectilePrefab;
    [SerializeField] private float wallProjectileSpeed = 15f;
    [SerializeField] private float wallLifetime = 15f;
    [SerializeField] private int bopsToSpawn = 20;
    private int bopCounter = 0;

    [Header("Laser Attack")]
    [SerializeField] private GameObject laserPrefab;

    [SerializeField] private ClearProjectiles clearProjectiles;
    [SerializeField] private DamageBoss damageBoss;
    private bool fightStarted = false;

    [SerializeField] private Transform[] critPositions;

    private Animator animator;
    private HashSet<int> deactivatedSpeakers = new HashSet<int>();

    private bool attackDelayHappening = false;

    [SerializeField] private int beginBopsDelay = 10;

    public void AddSpeakerToList(int layerIndex) => deactivatedSpeakers.Add(layerIndex);
    public void RemoveSpeakerFromList(int layerIndex) => deactivatedSpeakers.Remove(layerIndex);

    public void DamageAnimation()
    {
        animator.SetTrigger("Damage");
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentAttackPatterns = easyAttackPatterns; // Start with easy patterns
    }

    public void PhaseEnded()
    {

    }

    public void StartAttack(PhaseManager.SubPhase subphase)
    {
        Debug.Log(subphase);
        switch (subphase)
        {
            case PhaseManager.SubPhase.Tutorial:
                SpawnTutorialWeakpoints();
                break;
            case PhaseManager.SubPhase.Easy:
                fightStarted = true;
                SpawnCollumn(0, false);
                SpawnCollumn(1, false);
                SpawnCollumn(2, false);
                SpawnCollumn(3, false);
                break;
            case PhaseManager.SubPhase.Normal:
                NormalDifficulty();
                fightStarted = true;
                SpawnCollumn(0, false);
                SpawnCollumn(1, false);
                SpawnCollumn(2, false);
                SpawnCollumn(3, false);
                break;
        }
    }

    private void SpawnTutorialWeakpoints()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject weakpointClone = Instantiate(weakpoint, collumns[i].collumn.transform.position, Quaternion.identity);
            clearProjectiles.AddProjectile(weakpointClone);
            weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(WeakpointTutorialDestroyed);
            weakpointClone.GetComponent<WeakPoint>().onDeath.AddListener(DamageAnimation);
            weakpointClone.GetComponent<WeakPoint>().SetTarget(damageBoss.gameObject.transform);

            DamagePlayer damagePlayer = weakpointClone.GetComponent<DamagePlayer>();
            if (damagePlayer != null)
                damagePlayer.enabled = false;
        }
    }

    public void WeakpointTutorialDestroyed()
    {
        tutorialWeakpointsDestroyed++;
        if (tutorialWeakpointsDestroyed >= numberOfWeakspointsToDestroy)
        {
            damageBoss.ChangePhase();
        }
    }

    public void SpawnCollumn(int pos, bool immuneToDamage = true)
    {
        Collumn collumn = collumns[0];
        foreach (Collumn c in collumns)
        {
            if (c.index == pos)
            {
                collumn = c;
                break;
            }
        }
        collumn.collumn.GetComponent<BlowCollumnUp>().SetCritPoints(critPositions);
        AddSpeakerToList(pos);

        animator.SetTrigger("Button");
        collumn.collumn.SetActive(true);
        collumn.collumn.GetComponent<BlowCollumnUp>().Initialize(damageBoss, pos, 50, immuneToDamage);
    }

    public void SpawnCollum0ImmuneToDamage() => SpawnCollumn(0, true);
    public void SpawnCollum1ImmuneToDamage() => SpawnCollumn(1, true);
    public void SpawnCollum2ImmuneToDamage() => SpawnCollumn(2, true);
    public void SpawnCollum3ImmuneToDamage() => SpawnCollumn(3, true);
    public void NormalDifficulty()
    {
        normalDifficulty = true;
        currentAttackPatterns = normalAttackPatterns;
        currentPatternIndex = 0; // Reset pattern index when switching difficulties

        // Reset all pattern command indices
        for (int i = 0; i < currentAttackPatterns.Count; i++)
        {
            AttackPattern pattern = currentAttackPatterns[i];
            pattern.currentCommandIndex = 0;
            currentAttackPatterns[i] = pattern;
        }
    }

    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        StartAttack(subPhase);
    }

    void Start()
    {

    }

    void Update()
    {
        if (MoveWithMusic.Instance.bop && fightStarted)
        {
            Debug.Log("Bop happened on DJ");
            if (beginBopsDelay > 0)
            {
                beginBopsDelay--;
            }
            else
                if (usePatterns)
            {
                if (!attackDelayHappening)
                    StartCoroutine(ExecuteAttackPattern());
            }
            else
            {
                AttackFromRandomColumn();
                bopCounter++;
                if (bopCounter >= bopsToSpawn)
                {
                    ExecuteWallAttack();
                    bopCounter = 0;
                }
            }
        }

        foreach (int i in deactivatedSpeakers)
        {
            Debug.Log(i + " speaker deactivated");
        }
    }

    #region Individual Attack Methods

    /// <summary>
    /// Executes a laser attack from the specified column index
    /// </summary>
    public void ExecuteLaserAttack(int columnIndex)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();

        // Spawn laser at the firepoint
        GameObject laser = Instantiate(laserPrefab, column.firePoint.position, column.firePoint.rotation);
        clearProjectiles.AddProjectile(laser);

        // Set animation trigger if needed
        if (columnIndex == 0 || columnIndex == 2)
        {
            animator.SetTrigger("LeftArm");
        }
        else if (columnIndex == 1 || columnIndex == 3)
        {
            animator.SetTrigger("RightArm");
        }

    }

    /// <summary>
    /// Executes laser attacks from multiple columns with sequential delay
    /// </summary>
    public void ExecuteLaserAttacks(List<int> columnIndices, float sequentialDelay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        // Filter out invalid columns
        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

        // Execute attack from each valid column with sequential delay
        StartCoroutine(ExecuteLaserAttacksSequentially(validColumns, sequentialDelay));
    }

    private IEnumerator ExecuteLaserAttacksSequentially(List<int> columnIndices, float delay)
    {
        for (int i = 0; i < columnIndices.Count; i++)
        {
            if (i > 0 && delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            ExecuteLaserAttack(columnIndices[i]);
        }
    }

    /// <summary>
    /// Executes a column attack from the specified column index
    /// </summary>
    public void ExecuteColumnAttack(int columnIndex, float delay = 0f)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();

        // Set animation trigger
        if (columnIndex == 0 || columnIndex == 2)
        {
            //animator.SetTrigger("LeftArm");
        }
        else if (columnIndex == 1 || columnIndex == 3)
        {
            //animator.SetTrigger("RightArm");
        }

        ColumnAttack(
            column.firePoint,
            projectilePrefab,
            normalDifficulty ? projectileCountNormal[currentProjectileCountNormal] : projectileCount[currentProjectileCount],
            normalDifficulty ? attackArcAngleNormal : attackArcAngle,
            normalDifficulty ? projectileSpeedNormal : projectileSpeed,
            delay,
            Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale)
        );

        IncrementProjectileCount();
    }

    /// <summary>
    /// Executes column attacks from multiple columns
    /// </summary>
    public void ExecuteColumnAttacks(List<int> columnIndices, int numberOfShots = -1, float delay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        // Check if this is a double slam (multiple column attacks)
        bool isDoubleSlam = columnIndices.Count > 1;

        // Filter out invalid columns
        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

        // Show double slam UI if attacking from multiple columns
        if (isDoubleSlam && validColumns.Count > 1)
        {
            animator.SetTrigger("DoubleSlam");
            GameObject doubleSlamTextClone = Instantiate(doubleSlamText, doubleSlamSpawn.position, Quaternion.identity);
        }

        // Execute attack from each valid column
        foreach (int columnIndex in validColumns)
        {
            ColumnAttack(
                GetColumnByIndex(columnIndex).firePoint,
                projectilePrefab,
                numberOfShots > 0 ? numberOfShots : (normalDifficulty ? projectileCountNormal[currentProjectileCountNormal] : projectileCount[currentProjectileCount]),
                normalDifficulty ? attackArcAngleNormal : attackArcAngle,
                normalDifficulty ? projectileSpeedNormal : projectileSpeed,
                delay,
                Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale)
            );

            GetColumnByIndex(columnIndex).speakerEffects.ShootEffect();
        }

        IncrementProjectileCount();
    }

    /// <summary>
    /// Executes a wall attack from the specified column index
    /// </summary>
    public void ExecuteWallAttack(int columnIndex)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();

        GameObject projectile = Instantiate(wallProjectilePrefab, column.firePoint.position, Quaternion.identity);
        projectile.transform.rotation = column.firePoint.rotation;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(wallProjectileSpeed, wallLifetime);
        }

        //clearProjectiles.AddProjectile(projectile);
    }

    /// <summary>
    /// Executes wall attacks from multiple columns with sequential delay
    /// </summary>
    public void ExecuteWallAttacks(List<int> columnIndices, float sequentialDelay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        // Filter out invalid columns
        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

        // Execute attack from each valid column with sequential delay
        StartCoroutine(ExecuteWallAttacksSequentially(validColumns, sequentialDelay));
    }

    private IEnumerator ExecuteWallAttacksSequentially(List<int> columnIndices, float delay)
    {
        for (int i = 0; i < columnIndices.Count; i++)
        {
            if (i > 0 && delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            ExecuteWallAttack(columnIndices[i]);
        }
    }

    /// <summary>
    /// Executes a wall attack from a random valid column (for backwards compatibility)
    /// </summary>
    public void ExecuteWallAttack()
    {
        int randomIndex = GetRandomValidColumnIndex();
        if (randomIndex != -1)
        {
            ExecuteWallAttack(randomIndex);
        }
    }

    #endregion

    #region Pattern System
    /// <summary>
    /// Executes the current attack pattern
    /// </summary>
    public IEnumerator ExecuteAttackPattern()
    {
        if (currentAttackPatterns == null || currentAttackPatterns.Count == 0) yield break;

        // Get current pattern
        AttackPattern currentPattern = currentAttackPatterns[currentPatternIndex];

        attackDelayHappening = true;

        if (currentPattern.patternDelay > 0)
        {
            yield return new WaitForSeconds(currentPattern.patternDelay);
        }

        attackDelayHappening = false;

        // Execute current command
        if (currentPattern.attacks.Count > 0)
        {
            int commandIndex = currentPattern.currentCommandIndex;
            AttackCommand command = currentPattern.attacks[commandIndex];

            // Execute the command
            ExecuteAttackCommand(command);

            attackDelayHappening = true;

            // Use the command's delayBetweenCommands instead of the fixed delay
            if (command.delayBetweenCommands > 0)
            {
                yield return new WaitForSeconds(command.delayBetweenCommands);
            }

            attackDelayHappening = false;

            // Move to next command
            currentPattern.currentCommandIndex = (commandIndex + 1) % currentPattern.attacks.Count;
            currentAttackPatterns[currentPatternIndex] = currentPattern; // Update the struct in the list

            // If we've looped back to start, move to next pattern
            if (currentPattern.currentCommandIndex == 0)
            {
                currentPatternIndex = (currentPatternIndex + 1) % currentAttackPatterns.Count;
            }
        }
        else
        {
            // If pattern has no commands, just move to next pattern
            currentPatternIndex = (currentPatternIndex + 1) % currentAttackPatterns.Count;
        }
    }

    private IEnumerator ExecutePatternWithDelay(AttackPattern pattern)
    {

        if (pattern.patternDelay > 0)
        {
            yield return new WaitForSeconds(pattern.patternDelay);
        }


        // Execute each attack command in the pattern
        foreach (var attackCommand in pattern.attacks)
        {
            if (attackCommand.delay > 0)
            {
                yield return new WaitForSeconds(attackCommand.delay);
            }

            ExecuteAttackCommand(attackCommand);
        }
    }

    private void ExecuteAttackCommand(AttackCommand command)
    {
        switch (command.attackType)
        {
            case AttackType.ColumnAttack:
                ExecuteColumnAttacks(command.targetColumns, command.numberOfShots, command.delay);
                break;
            case AttackType.WallAttack:
                ExecuteWallAttacks(command.targetColumns, command.delay);
                break;
            case AttackType.LaserAttack:
                ExecuteLaserAttacks(command.targetColumns, command.delay);
                break;
            case AttackType.None:
                // Do nothing
                break;
        }
    }

    /// <summary>
    /// Sets a specific attack pattern to be executed
    /// </summary>
    public void SetAttackPattern(AttackPattern pattern)
    {
        StartCoroutine(ExecutePatternWithDelay(pattern));
    }

    /// <summary>
    /// Toggles between pattern mode and random mode
    /// </summary>
    public void SetPatternMode(bool usePatternMode)
    {
        usePatterns = usePatternMode;
    }

    #endregion

    #region Helper Methods

    private bool IsColumnValid(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= collumns.Length)
        {
            Debug.LogWarning($"Invalid column index: {columnIndex}");
            return false;
        }

        if (deactivatedSpeakers.Contains(columnIndex))
        {
            Debug.LogWarning($"Column {columnIndex} is deactivated");
            return false;
        }

        // Check difficulty-based column limits
        if (!collumns[columnIndex].collumn.activeSelf)
        {
            Debug.LogWarning($"Column {columnIndex} not active");
            return false;
        }

        // Check if too many speakers are deactivated
        if (!normalDifficulty && deactivatedSpeakers.Count >= 2)
        {
            return false;
        }
        else if (normalDifficulty && deactivatedSpeakers.Count >= 4)
        {
            return false;
        }

        return true;
    }

    private Collumn GetColumnByIndex(int columnIndex)
    {
        foreach (Collumn c in collumns)
        {
            if (c.index == columnIndex)
            {
                return c;
            }
        }
        return collumns[0]; // Fallback
    }

    private int GetRandomValidColumnIndex(int excludeColumn = -1)
    {
        int maxColumns = normalDifficulty ? collumns.Length : 2;

        // Check if any columns are available
        if ((!normalDifficulty && deactivatedSpeakers.Count >= 2) ||
            (normalDifficulty && deactivatedSpeakers.Count >= 4))
        {
            return -1;
        }

        // If we need to exclude a column, check if there are enough available columns
        int availableColumns = maxColumns - deactivatedSpeakers.Count;
        if (excludeColumn != -1 && !deactivatedSpeakers.Contains(excludeColumn))
        {
            availableColumns--; // One less available since we're excluding it
        }

        if (availableColumns <= 0)
        {
            return -1;
        }

        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, maxColumns);
        }
        while (deactivatedSpeakers.Contains(randomIndex) || randomIndex == excludeColumn);

        return randomIndex;
    }

    private void IncrementProjectileCount()
    {
        if (normalDifficulty)
        {
            currentProjectileCountNormal++;
            if (currentProjectileCountNormal >= projectileCountNormal.Length)
            {
                currentProjectileCountNormal = 0;
            }
        }
        else
        {
            currentProjectileCount++;
            if (currentProjectileCount >= projectileCount.Length)
            {
                currentProjectileCount = 0;
            }
        }
    }

    #endregion

    #region Original Methods (for backwards compatibility)

    private void ColumnAttack(
        Transform firePoint,
        GameObject projectilePrefab,
        int projectileCount,
        float spreadAngle,
        float projectileSpeed,
        float startDelay = 0f,
        Vector3 scale = default)
    {
        // Use default scale if none provided
        if (scale == default) scale = Vector3.one;

        // Add some fluctuation to avoid safe spots
        float newSpreadAngle = spreadAngle + UnityEngine.Random.Range(-20, 20);

        // Calculate angle between each projectile
        float angleBetween = newSpreadAngle / Mathf.Max(1, projectileCount - 1);
        float startAngle = -newSpreadAngle / 2f;

        for (int i = 0; i < projectileCount; i++)
        {
            // Calculate direction for this projectile
            float currentAngle = startAngle + (angleBetween * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * firePoint.forward;

            // Flatten to X-Z plane
            direction.y = 0;
            direction = direction.normalized;

            // Create projectile with parameters
            StartCoroutine(SpawnProjectileWithDelay(
                firePoint.position,
                direction,
                projectilePrefab,
                projectileSpeed,
                startDelay * i,
                scale));
        }
    }
    //
    private IEnumerator SpawnProjectileWithDelay(
        Vector3 position,
        Vector3 direction,
        GameObject prefab,
        float speed,
        float delay = 0f,
        Vector3 scale = default)
    {
        Debug.Log("projectile delay: " + delay);    
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        // Use default scale if none provided
        if (scale == default) scale = Vector3.one;

        // Instantiate the projectile
        GameObject projectile = Instantiate(prefab, position, Quaternion.identity);

        clearProjectiles.AddProjectile(projectile);

        // Apply scaling
        projectile.transform.localScale = scale;

        // Rotate the projectile to face the movement direction while keeping its up vector
        if (direction != Vector3.zero)
        {
            projectile.transform.rotation = Quaternion.LookRotation(direction);
            projectile.transform.rotation *= Quaternion.Euler(90f, 0, 0);
        }

        // Initialize the spike shot
        SpikeShot spikeShot = projectile.GetComponent<SpikeShot>();
        spikeShot.Initialize(speed, 30f); // 20f is the maxDistance - adjust as needed
    }

    private void AttackFromRandomColumn()
    {
        bool doDoubleAttack = false;

        if (normalDifficulty)
        {
            doDoubleAttack = UnityEngine.Random.value <= doubleSlamChanceNormal;
        }
        else
        {
            doDoubleAttack = UnityEngine.Random.value <= doubleSlamChance;
        }

        // Get random valid column
        int randomIndex = GetRandomValidColumnIndex();
        if (randomIndex == -1) return;

        if (doDoubleAttack)
        {
            // Find a second column for double slam
            int secondColumnIndex = GetRandomValidColumnIndex(randomIndex);
            if (secondColumnIndex != -1)
            {
                ExecuteColumnAttacks(new List<int> { randomIndex, secondColumnIndex });
            }
            else
            {
                ExecuteColumnAttack(randomIndex);
            }
        }
        else
        {
            ExecuteColumnAttack(randomIndex);
        }
    }

    #endregion
}