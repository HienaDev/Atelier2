using System;
using UnityEngine;
using System.Collections;
using static PhaseManager;
using UnityEditor.ShaderGraph.Internal;
using static UnityEditor.PlayerSettings;

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

    [Header("Spike Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int[] projectileCount = { 3, 4, 5};
    private int currentProjectileCount = 0;
    [SerializeField] private float attackArcAngle = 60f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileScale = 0.7f;
    [SerializeField] private float projectileDelay = 0f;
    [SerializeField] private ClearProjectiles clearProjectiles;
    [SerializeField] private DamageBoss damageBoss;
    private bool fightStarted = false;
    public void PhaseEnded()
    {

    }

    public void StartAttack(PhaseManager.SubPhase subphase)
    {
        Debug.Log(subphase);
        switch (subphase)
        {
            case PhaseManager.SubPhase.Tutorial:
                //StartCoroutine(SpawnTutorialWeakpoints());
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

        collumn.collumn.SetActive(true);
        collumn.collumn.GetComponent<BlowCollumnUp>().Initialize(damageBoss, 50);
    }

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
        StartAttack(PhaseManager.SubPhase.Easy);
    }

    // Update is called once per frame
    void Update()
    {
        if (MoveWithMusic.Instance.bop && fightStarted)
        {
            AttackFromRandomColumn();
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

        // Calculate angle between each projectile
        float angleBetween = spreadAngle / Mathf.Max(1, projectileCount - 1);
        float startAngle = -spreadAngle / 2f;

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

        spikeShot.Initialize(speed, 20f); // 20f is the maxDistance - adjust as needed


    }

    private void AttackFromRandomColumn()
    {
        int randomIndex = normalDifficulty ?
            UnityEngine.Random.Range(0, collumns.Length) :
            UnityEngine.Random.Range(0, 2);

        Collumn collumn = collumns[0];
        foreach (Collumn c in collumns)
        {
            if (c.index == randomIndex)
            {
                collumn = c;
                break;
            }
        }

        if (collumn.scaleWithMusic != null)
        {
            collumn.scaleWithMusic.Pulse();
        }



        // Attack parameters
        ColumnAttack(
            collumn.firePoint,
            projectilePrefab,
            projectileCount[currentProjectileCount],       // Projectile count
            attackArcAngle,   // Spread angle
            projectileSpeed,   // Speed
            projectileDelay,   // Delay
            Vector3.one * projectileScale // Scale
        );

        currentProjectileCount++;
        if (currentProjectileCount >= projectileCount.Length)
        {
            currentProjectileCount = 0;
        }
    }

}
