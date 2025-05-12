using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static PhaseManager;
using NaughtyAttributes;

public class DJBoss : MonoBehaviour, BossInterface
{

    [Serializable]
    public struct Collumn
    {
        public GameObject collumn;
        public ScaleWithMusic scaleWithMusic;
        public int index;
        public Transform firePoint;
    }

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

    [SerializeField] private ClearProjectiles clearProjectiles;
    [SerializeField] private DamageBoss damageBoss;
    private bool fightStarted = false;

    private Animator animator;

    private HashSet<int> deactivatedSpeakers = new HashSet<int>();

    public void AddSpeakerToList(int layerIndex) => deactivatedSpeakers.Add(layerIndex);
    public void RemoveSpeakerFromList(int layerIndex) => deactivatedSpeakers.Remove(layerIndex);

    public void DamageAnimation()
    {
        animator.SetTrigger("Damage");
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
                SpawnCollumn(0);
                SpawnCollumn(1);
                break;
            case PhaseManager.SubPhase.Normal:
                NormalDifficulty();
                fightStarted = true;
                SpawnCollumn(0);
                SpawnCollumn(1);
                SpawnCollumn(2);
                SpawnCollumn(3);
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

    private void SpawnCollumn(int pos)
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

        AddSpeakerToList(pos);

        animator.SetTrigger("Button");
        collumn.collumn.SetActive(true);
        collumn.collumn.GetComponent<BlowCollumnUp>().Initialize(damageBoss, pos, 50);
    }

    public void SpawnCollum0() => SpawnCollumn(0);
    public void SpawnCollum1() => SpawnCollumn(1);
    public void SpawnCollum2() => SpawnCollumn(2);
    public void SpawnCollum3() => SpawnCollumn(3);

    public void NormalDifficulty()
    {
        normalDifficulty = true;
    }

    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        StartAttack(subPhase);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (MoveWithMusic.Instance.bop && fightStarted)
        {
            AttackFromRandomColumn();
        }

        foreach (int i in deactivatedSpeakers)
        {
            Debug.Log(i + " speaker deactivated");
        }
    }

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

        // Add some flutation to avoid safe spots
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

    private IEnumerator SpawnProjectileWithDelay(
        Vector3 position,
        Vector3 direction,
        GameObject prefab,
        float speed,
        float delay = 0f,
        Vector3 scale = default)
    {
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

        if(normalDifficulty)
        {
            doDoubleAttack = UnityEngine.Random.value <= doubleSlamChanceNormal;
        }
        else
        {
            doDoubleAttack = UnityEngine.Random.value <= doubleSlamChance;
        }


        // First column attack
        int randomIndex = normalDifficulty ?
            UnityEngine.Random.Range(0, collumns.Length) :
            UnityEngine.Random.Range(0, 2);

        Collumn collumn = collumns[0];

        if (!normalDifficulty && deactivatedSpeakers.Count >= 2)
        {
            return;
        }
        else if (normalDifficulty && deactivatedSpeakers.Count >= 4)
        {
            return;
        }

        while (deactivatedSpeakers.Contains(randomIndex))
        {
            randomIndex = normalDifficulty ?
            UnityEngine.Random.Range(0, collumns.Length) :
            UnityEngine.Random.Range(0, 2);
        }

        foreach (Collumn c in collumns)
        {
            if (c.index == randomIndex)
            {
                collumn = c;
                break;
            }
        }

        //collumn.scaleWithMusic.Pulse();

        // Handle double attack
        if (doDoubleAttack)
        {
            // Trigger double slam animation
            animator.SetTrigger("DoubleSlam");
            // Spawn the double slam text
            GameObject doubleSlamTextClone = Instantiate(doubleSlamText, doubleSlamSpawn.position, Quaternion.identity);

            // Find a second random column different from the first
            int secondRandomIndex = randomIndex;

            // Only attempt to find a second column if there are at least 2 active columns
            if ((normalDifficulty && deactivatedSpeakers.Count < collumns.Length - 1) ||
                (!normalDifficulty && deactivatedSpeakers.Count < 1))
            {
                while (secondRandomIndex == randomIndex || deactivatedSpeakers.Contains(secondRandomIndex))
                {
                    secondRandomIndex = normalDifficulty ?
                        UnityEngine.Random.Range(0, collumns.Length) :
                        UnityEngine.Random.Range(0, 2);
                }

                // Find the second column
                Collumn secondCollumn = collumns[0];
                foreach (Collumn c in collumns)
                {
                    if (c.index == secondRandomIndex)
                    {
                        secondCollumn = c;
                        break;
                    }
                }

                // Pulse the second column
                //secondCollumn.scaleWithMusic.Pulse();

                // Attack from the second column
                ColumnAttack(
                    secondCollumn.firePoint,
                    projectilePrefab,
                    normalDifficulty ? projectileCountNormal[currentProjectileCountNormal] : projectileCount[currentProjectileCount],
                    normalDifficulty ? attackArcAngleNormal : attackArcAngle,
                    normalDifficulty ? projectileSpeedNormal : projectileSpeed,
                    normalDifficulty ? projectileDelayNormal : projectileDelay,
                    Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale)
                );
            }
        }
        else
        {
            // Regular single column animation
            if (randomIndex == 0 || randomIndex == 2)
            {
                //animator.SetTrigger("LeftArm");
            }
            else if (randomIndex == 1 || randomIndex == 3)
            {
                //animator.SetTrigger("RightArm");
            }
        }

        // Attack from the first column
        ColumnAttack(
            collumn.firePoint,
            projectilePrefab,
            normalDifficulty ? projectileCountNormal[currentProjectileCountNormal] : projectileCount[currentProjectileCount],       // Projectile count
            normalDifficulty ? attackArcAngleNormal : attackArcAngle,   // Spread angle
            normalDifficulty ? projectileSpeedNormal : projectileSpeed,   // Speed
            normalDifficulty ? projectileDelayNormal : projectileDelay,   // Delay
            Vector3.one * (normalDifficulty ? projectileScaleNormal : projectileScale) // Scale
        );

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
}