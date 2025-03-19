using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;


public class MinotaurAttacks : MonoBehaviour
{

    public enum AttackPattern
    {
        SpikeShots,
        SpikeRow,
        None
    }

    [SerializeField] private float switchAttackPatternTime = 10f;
    private float justChangedPattern = 0f;

    [SerializeField] private AttackPattern attackPattern;

    [Header("Minotaur Spike Shots")]
    [SerializeField] private SplineAnimate splineAnimateScript;
    [SerializeField] private Vector2 splineTimeRangeSpikeShots = new Vector2(0f, 0.99f);
    [SerializeField] private float splineSpeed = 1f;
    private float currentSplineTime = 0f;
    [SerializeField] private float spikeMaxDistance = 10f;
    [SerializeField] private float spikeSpeed = 10f;
    [SerializeField] private float spikeShotCooldown = 0.5f;
    private float justShot = 0f;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Transform firePoint;
    private PlayerMovementMonkeyHell playerMovementScript;

    [Header("Minotaur Spike Row")]
    [SerializeField] private Vector2 splineTimeRangeSpikeRows = new Vector2(0.33f, 0.66f);
    [SerializeField] private GameObject spikeRowPrefab;
    [SerializeField] private Transform[] spikeRowFirePoint;
    [SerializeField] private int numberOfRows = 2;
    [SerializeField] private float spikeRowCooldown = 5f;
    private float justRowShot = 0f;
    private List<int> chosenFirePoints;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovementScript = FindAnyObjectByType<PlayerMovementMonkeyHell>();

        if (numberOfRows > spikeRowFirePoint.Length)
        {
            numberOfRows = spikeRowFirePoint.Length;
        }

        chosenFirePoints = new List<int>();
        for (int i = 0; i < spikeRowFirePoint.Length; i++)
        {
            chosenFirePoints.Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time - justChangedPattern > switchAttackPatternTime)
        {
            justChangedPattern = Time.time;
            if (attackPattern == AttackPattern.SpikeShots)
            {
                attackPattern = AttackPattern.SpikeRow;
            }
            else
            {
                attackPattern = AttackPattern.SpikeShots;
            }
        }

        if (attackPattern == AttackPattern.SpikeShots)
        {

            if (Time.time - justShot > spikeShotCooldown)
            {
                justShot = Time.time;
                GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
                SpikeShot spikeShotScript = spike.GetComponent<SpikeShot>();
                spikeShotScript.Initialize(spikeSpeed, spikeMaxDistance);
            }

            Vector3 directionToPlayer = transform.position - playerMovementScript.transform.position;
            directionToPlayer.y = 0;


            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


                transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            }

            //if (currentSplineTime < splineTimeRangeSpikeShots.y && splineSpeed > 0 || currentSplineTime > splineTimeRangeSpikeShots.x && splineSpeed < 0)
            //{
            //    Vector3 directionToPlayer = transform.position - playerMovementScript.transform.position;
            //    directionToPlayer.y = 0;


            //    if (directionToPlayer != Vector3.zero)
            //    {
            //        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


            //        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            //    }

            //    currentSplineTime += Time.deltaTime * splineSpeed;
            //    currentSplineTime = Mathf.Clamp(currentSplineTime, splineTimeRangeSpikeShots.x, splineTimeRangeSpikeShots.y);
            //    splineAnimateScript.ElapsedTime = currentSplineTime;
            //}
            //else
            //{
            //    splineSpeed *= -1;
            //}
        }

        if (attackPattern == AttackPattern.SpikeRow)
        {
            if (Time.time - justRowShot > spikeRowCooldown)
            {
                justRowShot = Time.time;
                chosenFirePoints.Shuffle();
                for (int i = 0; i < numberOfRows; i++)
                {

                    GameObject spikeRow = Instantiate(spikeRowPrefab, spikeRowFirePoint[chosenFirePoints[i]].position, spikeRowFirePoint[chosenFirePoints[i]].rotation);

                }
            }

            //if (currentSplineTime < splineTimeRangeSpikeRows.y && splineSpeed > 0 || currentSplineTime > splineTimeRangeSpikeRows.x && splineSpeed < 0)
            //{
            //    Vector3 directionToPlayer = transform.position - playerMovementScript.transform.position;
            //    directionToPlayer.y = 0;


            //    if (directionToPlayer != Vector3.zero)
            //    {
            //        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


            //        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            //    }

            //    currentSplineTime += Time.deltaTime * splineSpeed;
            //    currentSplineTime = Mathf.Clamp(currentSplineTime, splineTimeRangeSpikeRows.x, splineTimeRangeSpikeRows.y);
            //    splineAnimateScript.ElapsedTime = currentSplineTime;
            //}
            //else
            //{
            //    splineSpeed *= -1;
            //}
        }

    }


}
