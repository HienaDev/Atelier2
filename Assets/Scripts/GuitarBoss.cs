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
    [SerializeField] private float delayBetweenLaunches = 0.3f;
    [SerializeField] private float evasiveMoveSpeed = 3f;
    [SerializeField] private float evasiveMoveRadius = 5f;
    [SerializeField] private float timeBetweenRandomMoves = 1.5f;
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Leg Barrage")]
    [SerializeField] private GameObject legProjectilePrefab;
    [SerializeField] private float legFireInterval = 0.4f;
    [SerializeField] private float legRegrowTime = 2f;
    [SerializeField] private List<FirePointSlot> airborneLegs;

    private bool isAttacking = false;
    private bool isEvading = false;
    private bool isLegAttackActive = false;
    private bool returning = false;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private float evadeTimer = 0f;

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
            flyingScript.Initialize(chosenPath, slot.firePoint, () =>
            {
                if (slot.visual != null)
                    slot.visual.SetActive(true);
            });

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

    public void StartAirborneLegAttack()
    {
        if (!isLegAttackActive)
            StartCoroutine(FireLegsLoop());
    }

    public void StopAirborneLegAttack()
    {
        isLegAttackActive = false;
    }

    private IEnumerator FireLegsLoop()
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

        Instantiate(legProjectilePrefab, leg.firePoint.position, rotation);
        StartCoroutine(RegrowLeg(leg));
    }

    private IEnumerator RegrowLeg(FirePointSlot leg)
    {
        yield return new WaitForSeconds(legRegrowTime);

        if (leg.visual != null)
            leg.visual.SetActive(true);
    }
}