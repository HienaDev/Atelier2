using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class MouthBossAttacks : MonoBehaviour, BossInterface
{
    [Serializable]
    private struct AttackPattern
    {
        public Vector3[] pattern;

    }

    [SerializeField] private PlayerMovementEverHood playerMovement;
    [SerializeField] private Transform shootingPoint;

    private int gridHalfSize;

    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float attackSpeedMultiplier = 2f;
    [SerializeField] private float attackSpeed = 5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float weakpointSpeed = 5f;
    private float justAttacked = 0f;

    [SerializeField] private GameObject weakpointPrefab;

    [SerializeField, Range(0, 100)] private int weakpointChance = 50;

    [SerializeField] private DamageBoss health;

    [SerializeField] private ClearProjectiles clearProjectiles;

    [SerializeField] private Transform targetForWeakpoints;

    [SerializeField] private Transform bottomMouth;
    [SerializeField] private Transform topMouth;

    [SerializeField] private float biteDuration = 0.2f;

    [SerializeField] private MoveWithMusic moveWithMusic;

    [SerializeField] private AttackPattern[] attackPatternsEasy;
    [SerializeField] private AttackPattern[] attackPatterns;
    private AttackPattern[] currentPatterns;
    private AttackPattern currentPattern;
    private int currentPatternRow = 0;

    [SerializeField] private int numberOfWeakspointsToDestroy = 2;
    private int numberOfWeakpointsDestroyed = 0;

    private bool fightStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPatterns = attackPatternsEasy;
        gridHalfSize = (int)Math.Ceiling((float)playerMovement.GridSize / 2f);
        currentPattern = currentPatterns[UnityEngine.Random.Range(0, currentPatterns.Length)];
    }

    public void StartBoss(PhaseManager.SubPhase subPhase)
    {
        StartAttack(subPhase);
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
                StartCoroutine(SpawnTutorialWeakpoints());
                break;
            case PhaseManager.SubPhase.Easy:
                fightStarted = true;
                break;
            case PhaseManager.SubPhase.Normal:
                NormalDifficulty();
                fightStarted = true;
                break;
        }
    }

    public void NormalDifficulty()
    {
        currentPatterns = attackPatterns;
        attackSpeed *= attackSpeedMultiplier;
        weakpointSpeed *= attackSpeedMultiplier;
    }

    void Update()
    {
        if (!fightStarted)
            return;

        if (moveWithMusic.bop)
        {
            SendAttack();
        }
    }

    public void TutorialWeakpointDestroyed()
    {
        numberOfWeakpointsDestroyed++;
    }

    private void DealWeakPointDamage()
    {
        health.DealDamage(30);
    }

    public void SendAttack()
    {

        bottomMouth.DOLocalRotate(new Vector3(35f, 0f, 0f), biteDuration / 2).OnComplete(() => bottomMouth.DOLocalRotate(new Vector3(0f, 0f, 0f), biteDuration / 2));
        topMouth.DOLocalRotate(new Vector3(-15f, 0f, 0f), biteDuration / 2).OnComplete(() => topMouth.DOLocalRotate(new Vector3(0f, 0f, 0f), biteDuration / 2));

        // 2 - 4
        int attackPosition = UnityEngine.Random.Range(2, playerMovement.GridSize);

        bool weakPointActive = UnityEngine.Random.Range(0, 100) < weakpointChance;
        int weakPointPosition = UnityEngine.Random.Range(-1, 2);

        int[] attacks = new int[]
                        {
                            (int)currentPattern.pattern[currentPatternRow].x,
                            (int)currentPattern.pattern[currentPatternRow].y,
                            (int)currentPattern.pattern[currentPatternRow].z
                        };

        for (int i = -1; i < 2; i++)
        {
            if (attacks[i + 1] == 0 && (!weakPointActive))// && weakPointPosition == i))
                continue;

            GameObject attack;

            if (weakPointActive && weakPointPosition != i)
            {
                attack = Instantiate(weakpointPrefab);
                attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - i - gridHalfSize) * new Vector3(0f, 0f, 1f);
                attack.GetComponent<Rigidbody>().linearVelocity = -attack.transform.right * weakpointSpeed;

                attack.GetComponent<WeakPoint>().onDeath.AddListener(DealWeakPointDamage);
                attack.GetComponent<WeakPoint>().SetTarget(targetForWeakpoints);
            }
            else
            {
                attack = Instantiate(attackPrefab);

                attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - i - gridHalfSize) * new Vector3(0f, 0f, 1f);

                attack.GetComponent<Rigidbody>().linearVelocity = attack.transform.up * attackSpeed;
            }

            clearProjectiles.AddProjectile(attack);
        }

        currentPatternRow++;

        if (currentPatternRow >= currentPattern.pattern.Length)
        {
            currentPatternRow = 0;
            currentPattern = currentPatterns[UnityEngine.Random.Range(0, currentPatterns.Length)];
        }
    }

    private IEnumerator SpawnTutorialWeakpoints()
    {
        health.ToggleDamageable(false);

        Debug.Log(numberOfWeakpointsDestroyed + " >= " + numberOfWeakspointsToDestroy + " : " + (numberOfWeakpointsDestroyed >= numberOfWeakspointsToDestroy));
        while (numberOfWeakpointsDestroyed < numberOfWeakspointsToDestroy)
        {
            int attackPosition = UnityEngine.Random.Range(1, playerMovement.GridSize + 1);
            GameObject attack = Instantiate(weakpointPrefab);
            attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - gridHalfSize) * new Vector3(0f, 0f, 1f);
            attack.GetComponent<Rigidbody>().linearVelocity = -attack.transform.right * weakpointSpeed;

            attack.GetComponent<WeakPoint>().onDeath.AddListener(TutorialWeakpointDestroyed);
            attack.GetComponent<WeakPoint>().SetTarget(targetForWeakpoints);
            yield return new WaitForSeconds(3f); // Wait for 5 seconds
        }
        Debug.Log("Leave coroutine");
        health.ChangePhase();
        health.ToggleDamageable(true);
    }
}