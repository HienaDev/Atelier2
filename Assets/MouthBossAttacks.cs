using UnityEngine;
using System;
using DG.Tweening;

public class MouthBossAttacks : MonoBehaviour
{
    [SerializeField] private PlayerMovementEverHood playerMovement;
    [SerializeField] private Transform shootingPoint;

    private int gridHalfSize;

    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private float attackSpeed = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float weakpointSpeed = 10f;
    private float justAttacked = 0f;

    [SerializeField] private GameObject weakpointPrefab;

    [SerializeField, Range(0, 100)] private int weakpointChance = 50;

    [SerializeField] private BossHealth health;

    [SerializeField] private ClearProjectiles clearProjectiles;

    [SerializeField] private Transform targetForWeakpoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridHalfSize = (int)Math.Ceiling((float)playerMovement.GridSize / 2f);


    }

    private void OnEnable()
    {
        //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, playerMovement.StartPosition.z);


    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - justAttacked >= attackCooldown)
        {
            SendAttack();
            justAttacked = Time.time;
        }
    }

    public void SendAttack()
    {
        int attackPosition = UnityEngine.Random.Range(1, playerMovement.GridSize + 1);

        bool weakPointActive = UnityEngine.Random.Range(0, 100) < weakpointChance;


        GameObject attack;
        
        if(weakPointActive)
        {
            attack = Instantiate(weakpointPrefab);
            attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - gridHalfSize) * new Vector3(0f, 0f, 1f);
            attack.GetComponent<Rigidbody>().linearVelocity = -attack.transform.right * attackSpeed / 2;

            attack.GetComponent<WeakPoint>().onDeath.AddListener(health.DealCritDamage);
            attack.GetComponent<WeakPoint>().SetTarget(targetForWeakpoints);
        }

        else
        {
            attack = Instantiate(attackPrefab);
            attack.transform.position = shootingPoint.position + playerMovement.CellDistance * (attackPosition - gridHalfSize) * new Vector3(0f, 0f, 1f);
            Debug.Log(playerMovement.CellDistance * (attackPosition - gridHalfSize) * new Vector3(0f, 0f, 1f));
            attack.GetComponent<Rigidbody>().linearVelocity = attack.transform.up * attackSpeed;
        }

        clearProjectiles.AddProjectile(attack);


    }
}
