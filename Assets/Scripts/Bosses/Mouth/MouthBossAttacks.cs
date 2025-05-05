using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using static MinotaurAttacks;

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

    [SerializeField] private GameObject[] attackPrefab;
    [SerializeField] private float attackSpeedMultiplier = 2f;
    [SerializeField] private float attackSpeed = 5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float weakpointSpeed = 5f;
    private float justAttacked = 0f;

    [SerializeField] private GameObject weakpointPrefab;

    [SerializeField, Range(0, 100)] private int weakpointChance = 50;

    [SerializeField] private DamageBoss[] damageableParts;
    [SerializeField] private DamageBoss mainDamagePart;

    [SerializeField] private ClearProjectiles clearProjectiles;

    [SerializeField] private Transform targetForWeakpoints;

    [SerializeField] private Transform bottomMouth;
    [SerializeField] private Transform topMouth;

    [SerializeField] private float biteDuration = 0.2f;

    [SerializeField] private MoveWithMusic moveWithMusic;

    [SerializeField] private ShotPattern[] attackPatternEasy5;
    [SerializeField] private ShotPattern[] attackPatternNormal5;

    [SerializeField] private ShotPattern[] attackPattern1Easy;
    [SerializeField] private ShotPattern[] attackPattern2Easy;
    [SerializeField] private ShotPattern[] attackPattern3Easy;
    [SerializeField] private ShotPattern[] attackPattern4Easy;

    private int currentPatternIndex = 0;
    private ShotPattern[] currentPatterns;
    private ShotPattern currentPattern;
    private int currentPatternRow;

    [SerializeField] private int numberOfWeakspointsToDestroy = 2;
    private int numberOfWeakpointsDestroyed = 0;

    private bool fightStarted = false;
    private bool justChangedPattern = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        attackPatternEasy5.Shuffle();
        attackPatternNormal5.Shuffle();

        currentPatterns = attackPatternEasy5;
        currentPattern = currentPatterns[currentPatternIndex];

        gridHalfSize = (int)Math.Ceiling((float)playerMovement.GridSize / 2f);

        currentPatternRow = currentPattern.Width - 1;
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
        currentPatternIndex = 0;
        currentPatterns = attackPatternNormal5;
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

        damageableParts[0].DealDamage(30);
    }

    public void SendAttack()
    {

        bottomMouth.DOLocalRotate(new Vector3(35f, 0f, 0f), biteDuration / 2).OnComplete(() => bottomMouth.DOLocalRotate(new Vector3(0f, 0f, 0f), biteDuration / 2));
        topMouth.DOLocalRotate(new Vector3(-15f, 0f, 0f), biteDuration / 2).OnComplete(() => topMouth.DOLocalRotate(new Vector3(0f, 0f, 0f), biteDuration / 2));

        if (justChangedPattern)
        {
            justChangedPattern = false;
            //return;
        }


        // 2 - 4
        int attackPosition = UnityEngine.Random.Range(2, playerMovement.GridSize);

        bool weakPointActive = UnityEngine.Random.Range(0, 100) < weakpointChance;
        int weakPointPosition = UnityEngine.Random.Range(-1, 2);

        int[] attacks = new int[5]
                        {
                            (int)currentPattern[4,currentPatternRow],
                            (int)currentPattern[3,currentPatternRow],
                            (int)currentPattern[2,currentPatternRow],
                            (int)currentPattern[1,currentPatternRow],
                            (int)currentPattern[0,currentPatternRow]
                        };



        Debug.Log("attacks pattern: " + attacks[0] + ", " + attacks[1] + ", " + attacks[2] + ", " + attacks[3] + ", " + attacks[4] + ", ");

        for (int i = -2; i < 3; i++)
        {
            if (attacks[i + 2] == 0 && (!weakPointActive))// && weakPointPosition == i))
                continue;

            GameObject attack;

            if (weakPointActive && weakPointPosition != i)
            {
                attack = Instantiate(weakpointPrefab);

                attack.transform.position = shootingPoint.position + playerMovement.CellDistance * i * new Vector3(0f, 0f, 1f);
                attack.GetComponent<Rigidbody>().linearVelocity = -attack.transform.right * weakpointSpeed;

                attack.GetComponent<WeakPoint>().onDeath.AddListener(DealWeakPointDamage);
                attack.GetComponent<WeakPoint>().SetTarget(targetForWeakpoints);
            }
            else
            {
                attack = Instantiate(attackPrefab[attacks[i + 2] - 1]); // - 1 because 0 in the inspector means "hole" but here the slot 0 of attacks is a prefab

                attack.transform.position = shootingPoint.position + playerMovement.CellDistance * i * new Vector3(0f, 0f, 1f);

                Debug.Log("position:" + attack.transform.position + " i: " + i);

                attack.GetComponent<Rigidbody>().linearVelocity = attack.transform.up * attackSpeed;
            }

            clearProjectiles.AddProjectile(attack);
        }

        currentPatternRow--;

        if (currentPatternRow < 0)
        {
            currentPatternIndex++;
            if (currentPatternIndex >= currentPatterns.Length)
            {
                currentPatterns.Shuffle();
                currentPatternIndex = 0;
            }

            currentPatternRow = currentPattern.Width - 1;
            justChangedPattern = true;


            currentPattern = currentPatterns[currentPatternIndex];
        }
    }



    private IEnumerator SpawnTutorialWeakpoints()
    {
        foreach (DamageBoss damageBoss in damageableParts)
            damageBoss.ToggleDamageable(false);

        mainDamagePart.ToggleDamageable(false);

        List<WeakPoint> weakPoints = new List<WeakPoint>();

        Debug.Log(numberOfWeakpointsDestroyed + " >= " + numberOfWeakspointsToDestroy + " : " + (numberOfWeakpointsDestroyed >= numberOfWeakspointsToDestroy));
        while (numberOfWeakpointsDestroyed < numberOfWeakspointsToDestroy)
        {
            if (!moveWithMusic.bop)
            {
                yield return null; // Wait for 2 seconds
            }
            else
            {
                int attackPosition = UnityEngine.Random.Range(1, playerMovement.GridSize + 1);
                GameObject attack = Instantiate(weakpointPrefab);
                attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - gridHalfSize) * new Vector3(0f, 0f, 1f);
                attack.GetComponent<Rigidbody>().linearVelocity = attack.transform.up * weakpointSpeed;

                WeakPoint weakPoint = attack.GetComponent<WeakPoint>();
                weakPoints.Add(weakPoint);
                weakPoint.onDeath.AddListener(TutorialWeakpointDestroyed);
                weakPoint.SetTarget(targetForWeakpoints);

                attack.GetComponent<DamagePlayer>().dealsDamage = false;
            }


            yield return null; // Wait for 2 seconds
        }
        Debug.Log("Leave coroutine");

        foreach (WeakPoint weakPoint in weakPoints)
            weakPoint.BlowUp();

        foreach (DamageBoss damageBoss in damageableParts)
            damageBoss.ToggleDamageable(true);

        mainDamagePart.ToggleDamageable(true);

        mainDamagePart.ChangePhase();

    }
}