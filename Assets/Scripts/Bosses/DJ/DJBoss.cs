using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        public List<int> targetColumns;
        [Range(0f, 2f)]
        public float delay;
        [Range(1, 10)]
        public int numberOfShots;
        [Range(0f, 5f)]
        public float delayBetweenCommands;
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

    [Header("Audio Settings")]
    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private AudioClip[] columnAttackSounds;
    [SerializeField] private float columnAttackVolume = 1f;
    [SerializeField] private AudioClip wallAttackSound;
    [SerializeField] private float wallAttackVolume = 1f;
    [SerializeField] private float wallAttackPitchVariation = 0.2f; // Random pitch variation range
    
    // LASER SOUND SETTINGS - Procedural sound system
    [Header("Laser Sound Settings")]
    [SerializeField] private AudioClip laserPulseSound; // Base sound for each pulse
    [SerializeField] private float laserBaseVolume = 0.8f;
    [SerializeField] private float laserBasePitch = 1f;
    [SerializeField] private float laserPitchIncrement = 0.15f; // How much pitch increases each pulse
    [SerializeField] private float laserBasePitchVariation = 0.1f; // Random pitch variation for each new laser
    
    [Header("Double Slam Sound Settings")]
    [SerializeField] private AudioClip doubleSlamSound;
    [SerializeField] private float doubleSlamVolume = 1f;
    [SerializeField] private float doubleSlamPitchVariation = 0.2f; // Random pitch variation range
    
    [Header("Button Sound Settings")]
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private float buttonPressVolume = 1f;
    [SerializeField] private float buttonPressPitchVariation = 0.1f; // Random pitch variation range
    
    [SerializeField] private AudioClip[] speakerEffectSounds;
    [SerializeField] private float speakerEffectVolume = 1f;
    [SerializeField] private AudioClip patternChangeSound;
    [SerializeField] private float patternChangeVolume = 1f;

    [Header("Attack Patterns")]
    [SerializeField] private List<AttackPattern> attackPatterns = new List<AttackPattern>();
    [SerializeField] private List<AttackPattern> easyAttackPatterns = new List<AttackPattern>();
    [SerializeField] private List<AttackPattern> normalAttackPatterns = new List<AttackPattern>();
    private List<AttackPattern> currentAttackPatterns = new List<AttackPattern>();
    [SerializeField] private int currentPatternIndex = 0;
    [SerializeField] private bool usePatterns = false;

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

    // Dictionary to track sound parameters for each active laser
    private Dictionary<GameObject, LaserSoundData> activeLaserSounds = new Dictionary<GameObject, LaserSoundData>();

    [Serializable]
    private class LaserSoundData
    {
        public float currentPitch;
        public int pulseCount;
        
        public LaserSoundData(float basePitch)
        {
            currentPitch = basePitch;
            pulseCount = 0;
        }
    }

    public void AddSpeakerToList(int layerIndex) => deactivatedSpeakers.Add(layerIndex);
    public void RemoveSpeakerFromList(int layerIndex) => deactivatedSpeakers.Remove(layerIndex);

    public void DamageAnimation()
    {
        animator.SetTrigger("Damage");
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentAttackPatterns = easyAttackPatterns;
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
                Destroy(damagePlayer);
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
        currentPatternIndex = 0;
        attackDelayHappening = false;
        
        // Play pattern change sound
        PlayPatternChangeSound();
        
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

    void Update()
    {
        if (MoveWithMusic.Instance.bop && fightStarted)
        {
            Debug.Log("Bop happened on DJ");
            if (beginBopsDelay > 0)
            {
                beginBopsDelay--;
            }
            else if (usePatterns)
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

    #region Audio Methods

    private void PlayColumnAttackSound()
    {
        if (columnAttackSounds.Length > 0 && AudioManager.Instance != null)
        {
            AudioClip randomSound = columnAttackSounds[UnityEngine.Random.Range(0, columnAttackSounds.Length)];
            if (randomSound != null)
            {
                AudioManager.Instance.PlaySound(randomSound, audioVolume * columnAttackVolume, 1f, true, 0f);
            }
        }
    }

    private void PlayWallAttackSound()
    {
        if (wallAttackSound != null && AudioManager.Instance != null)
        {
            // Add random pitch variation within the specified range
            float randomPitch = 1f + UnityEngine.Random.Range(-wallAttackPitchVariation, wallAttackPitchVariation);
            AudioManager.Instance.PlaySound(wallAttackSound, audioVolume * wallAttackVolume, randomPitch, true, 0f);
        }
    }

    // NEW LASER SOUND SYSTEM
    /// <summary>
    /// Method to be called by laser when it flashes.
    /// Call this method from the laser script on each pulse/flash.
    /// </summary>
    public void PlayLaserPulseSound(GameObject laserObject)
    {
        if (laserPulseSound == null || AudioManager.Instance == null) return;

        // If this is the first time this laser plays sound, initialize data
        if (!activeLaserSounds.ContainsKey(laserObject))
        {
            // Add random pitch variation to the base pitch for each new laser
            float randomBasePitch = laserBasePitch + UnityEngine.Random.Range(-laserBasePitchVariation, laserBasePitchVariation);
            activeLaserSounds[laserObject] = new LaserSoundData(randomBasePitch);
        }

        LaserSoundData soundData = activeLaserSounds[laserObject];
        
        // Play sound with current pitch and base volume - force 2D sound
        AudioManager.Instance.PlaySound(
            laserPulseSound, 
            audioVolume * laserBaseVolume, 
            soundData.currentPitch, 
            true, // Allow multiple instances to overlap
            0f // Force 2D sound (no spatial audio)
        );

        // Increment pitch for next time
        soundData.pulseCount++;
        soundData.currentPitch += laserPitchIncrement;
    }

    /// <summary>
    /// Clears sound data when laser is destroyed.
    /// Call this method when the laser is destroyed.
    /// </summary>
    public void OnLaserDestroyed(GameObject laserObject)
    {
        if (activeLaserSounds.ContainsKey(laserObject))
        {
            activeLaserSounds.Remove(laserObject);
        }
    }

    private void PlayDoubleSlamSound()
    {
        if (doubleSlamSound != null && AudioManager.Instance != null)
        {
            // Add random pitch variation within the specified range
            float randomPitch = 1f + UnityEngine.Random.Range(-doubleSlamPitchVariation, doubleSlamPitchVariation);
            AudioManager.Instance.PlaySound(doubleSlamSound, audioVolume * doubleSlamVolume, randomPitch, true, 0f);
        }
    }

    private void PlaySpeakerEffectSound()
    {
        if (speakerEffectSounds.Length > 0 && AudioManager.Instance != null)
        {
            AudioClip randomSound = speakerEffectSounds[UnityEngine.Random.Range(0, speakerEffectSounds.Length)];
            if (randomSound != null)
            {
                AudioManager.Instance.PlaySound(randomSound, audioVolume * speakerEffectVolume, 1f, true, 0f);
            }
        }
    }

    private void PlayPatternChangeSound()
    {
        if (patternChangeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(patternChangeSound, audioVolume * patternChangeVolume, 1f, true, 0f);
        }
    }

    /// <summary>
    /// Public method to play button press sound for DJ animations
    /// Call this method from animation events
    /// </summary>
    public void PlayButtonPressSound()
    {
        if (buttonPressSound != null && AudioManager.Instance != null)
        {
            // Add random pitch variation within the specified range
            float randomPitch = 1f + UnityEngine.Random.Range(-buttonPressPitchVariation, buttonPressPitchVariation);
            AudioManager.Instance.PlaySound(buttonPressSound, audioVolume * buttonPressVolume, randomPitch, true, 0f);
        }
    }

    #endregion

    #region Individual Attack Methods

    public void ExecuteLaserAttack(int columnIndex)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();
        PlaySpeakerEffectSound();

        GameObject laser = Instantiate(laserPrefab, column.firePoint.position, column.firePoint.rotation);
        clearProjectiles.AddProjectile(laser);

        if (columnIndex == 0 || columnIndex == 2)
        {
            animator.SetTrigger("LeftArm");
        }
        else if (columnIndex == 1 || columnIndex == 3)
        {
            animator.SetTrigger("RightArm");
        }
    }

    public void ExecuteLaserAttacks(List<int> columnIndices, float sequentialDelay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

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

    public void ExecuteColumnAttack(int columnIndex, float delay = 0f)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();
        PlaySpeakerEffectSound();

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
            Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale),
            true // Play sound for single column attacks
        );

        IncrementProjectileCount();
    }

    public void ExecuteColumnAttacks(List<int> columnIndices, int numberOfShots = -1, float delay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        bool isDoubleSlam = columnIndices.Count > 1;
        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

        if (isDoubleSlam && validColumns.Count > 1)
        {
            animator.SetTrigger("DoubleSlam");
            GameObject doubleSlamTextClone = Instantiate(doubleSlamText, doubleSlamSpawn.position, Quaternion.identity);
            PlayDoubleSlamSound(); // Play double slam sound instead of column attack sound
        }

        foreach (int columnIndex in validColumns)
        {
            ColumnAttack(
                GetColumnByIndex(columnIndex).firePoint,
                projectilePrefab,
                numberOfShots > 0 ? numberOfShots : (normalDifficulty ? projectileCountNormal[currentProjectileCountNormal] : projectileCount[currentProjectileCount]),
                normalDifficulty ? attackArcAngleNormal : attackArcAngle,
                normalDifficulty ? projectileSpeedNormal : projectileSpeed,
                delay,
                Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale),
                !isDoubleSlam // Don't play column sound if it's a double slam
            );

            GetColumnByIndex(columnIndex).speakerEffects.ShootEffect();
            PlaySpeakerEffectSound();
        }

        IncrementProjectileCount();
    }

    public void ExecuteWallAttack(int columnIndex)
    {
        if (!IsColumnValid(columnIndex)) return;

        Collumn column = GetColumnByIndex(columnIndex);
        column.speakerEffects.ShootEffect();
        PlaySpeakerEffectSound();
        PlayWallAttackSound();

        GameObject projectile = Instantiate(wallProjectilePrefab, column.firePoint.position, Quaternion.identity);
        projectile.transform.rotation = column.firePoint.rotation;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(wallProjectileSpeed, wallLifetime);
        }
    }

    public void ExecuteWallAttacks(List<int> columnIndices, float sequentialDelay = 0f)
    {
        if (columnIndices == null || columnIndices.Count == 0) return;

        var validColumns = columnIndices.Where(IsColumnValid).ToList();
        if (validColumns.Count == 0) return;

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

    public IEnumerator ExecuteAttackPattern()
    {
        if (currentAttackPatterns == null || currentAttackPatterns.Count == 0) yield break;

        AttackPattern currentPattern = currentAttackPatterns[currentPatternIndex];

        attackDelayHappening = true;

        if (currentPattern.patternDelay > 0)
        {
            yield return new WaitForSeconds(currentPattern.patternDelay);
        }

        attackDelayHappening = false;

        if (currentPattern.attacks.Count > 0)
        {
            int commandIndex = currentPattern.currentCommandIndex;
            AttackCommand command = currentPattern.attacks[commandIndex];

            ExecuteAttackCommand(command);

            attackDelayHappening = true;

            if (command.delayBetweenCommands > 0)
            {
                yield return new WaitForSeconds(command.delayBetweenCommands);
            }

            attackDelayHappening = false;

            currentPattern.currentCommandIndex = (commandIndex + 1) % currentPattern.attacks.Count;
            currentAttackPatterns[currentPatternIndex] = currentPattern;

            if (currentPattern.currentCommandIndex == 0)
            {
                currentPatternIndex = (currentPatternIndex + 1) % currentAttackPatterns.Count;
                PlayPatternChangeSound();
            }
        }
        else
        {
            currentPatternIndex = (currentPatternIndex + 1) % currentAttackPatterns.Count;
            PlayPatternChangeSound();
        }
    }

    private IEnumerator ExecutePatternWithDelay(AttackPattern pattern)
    {
        if (pattern.patternDelay > 0)
        {
            yield return new WaitForSeconds(pattern.patternDelay);
        }

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
                break;
        }
    }

    public void SetAttackPattern(AttackPattern pattern)
    {
        StartCoroutine(ExecutePatternWithDelay(pattern));
    }

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

        if (!collumns[columnIndex].collumn.activeSelf)
        {
            Debug.LogWarning($"Column {columnIndex} not active");
            return false;
        }

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
        return collumns[0];
    }

    private int GetRandomValidColumnIndex(int excludeColumn = -1)
    {
        int maxColumns = normalDifficulty ? collumns.Length : 2;

        if ((!normalDifficulty && deactivatedSpeakers.Count >= 2) ||
            (normalDifficulty && deactivatedSpeakers.Count >= 4))
        {
            return -1;
        }

        int availableColumns = maxColumns - deactivatedSpeakers.Count;
        if (excludeColumn != -1 && !deactivatedSpeakers.Contains(excludeColumn))
        {
            availableColumns--;
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

    #region Original Methods

    private void ColumnAttack(
        Transform firePoint,
        GameObject projectilePrefab,
        int projectileCount,
        float spreadAngle,
        float projectileSpeed,
        float startDelay = 0f,
        Vector3 scale = default,
        bool playSound = true)
    {
        if (scale == default) scale = Vector3.one;

        // Play column attack sound only if requested (not for double slam)
        if (playSound)
        {
            PlayColumnAttackSound();
        }

        float newSpreadAngle = spreadAngle + UnityEngine.Random.Range(-20, 20);
        float angleBetween = newSpreadAngle / Mathf.Max(1, projectileCount - 1);
        float startAngle = -newSpreadAngle / 2f;

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleBetween * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * firePoint.forward;

            direction.y = 0;
            direction = direction.normalized;

            StartCoroutine(SpawnProjectileWithDelay(
                firePoint.position,
                direction,
                projectilePrefab,
                projectileSpeed,
                startDelay * i,
                scale));
        }
    }

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

        if (scale == default) scale = Vector3.one;

        GameObject projectile = Instantiate(prefab, position, Quaternion.identity);
        clearProjectiles.AddProjectile(projectile);
        projectile.transform.localScale = scale;

        if (direction != Vector3.zero)
        {
            projectile.transform.rotation = Quaternion.LookRotation(direction);
            projectile.transform.rotation *= Quaternion.Euler(90f, 0, 0);
        }

        SpikeShot spikeShot = projectile.GetComponent<SpikeShot>();
        spikeShot.Initialize(speed, 30f);
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

        int randomIndex = GetRandomValidColumnIndex();
        if (randomIndex == -1) return;

        if (doDoubleAttack)
        {
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