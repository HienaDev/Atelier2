using Unity.Cinemachine;
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

    [SerializeField] private AttackPattern attackPattern;

    [Header("Minotaur Spike Shots")]
    [SerializeField] private SplineAnimate splineAnimateScript;
    [SerializeField] private float splineSpeed = 1f;
    private float currentSplineTime = 0f;
    [SerializeField] private float spikeMaxDistance = 10f;  
    [SerializeField] private float spikeSpeed = 10f;
    [SerializeField] private float spikeShotCooldown = 0.5f;
    private float justShot = 0f;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private Transform firePoint;
    private PlayerMovement playerMovementScript;

    [Header("Minotaur Spike Row")]
    [SerializeField] private GameObject spikeRowPrefab;
    [SerializeField] private Transform[] spikeRowFirePoint;
    [SerializeField] private int numberOfRows = 2;
    [SerializeField] private float spikeRowCooldown = 5f;
    private float justRowShot = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovementScript = FindAnyObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(attackPattern == AttackPattern.SpikeShots)
        {

            if(Time.time - justShot > spikeShotCooldown)
            {
                justShot = Time.time;
                GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
                SpikeShot spikeShotScript = spike.GetComponent<SpikeShot>();
                spikeShotScript.Initialize(spikeSpeed, spikeMaxDistance);
            }

            if (currentSplineTime < 0.99f && splineSpeed > 0 || currentSplineTime > 0f && splineSpeed < 0)
            {
                Vector3 directionToPlayer = transform.position - playerMovementScript.transform.position;
                directionToPlayer.y = 0;


                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


                    transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                }

                currentSplineTime += Time.deltaTime * splineSpeed;
                currentSplineTime = Mathf.Clamp(currentSplineTime, 0f, 0.99f);
                splineAnimateScript.ElapsedTime = currentSplineTime;
            }
            else
            {
                splineSpeed *= -1;
            }
        }

        if(attackPattern == AttackPattern.SpikeRow)
        {
            if(Time.time - justRowShot > spikeRowCooldown)
            {
                justRowShot = Time.time;
                for (int i = 0; i < numberOfRows; i++)
                {
                    int spikeChoice = Random.Range(0, spikeRowFirePoint.Length);
                    GameObject spikeRow = Instantiate(spikeRowPrefab, spikeRowFirePoint[spikeChoice].position, spikeRowFirePoint[spikeChoice].rotation);

                }
            }
        }

    }
}
