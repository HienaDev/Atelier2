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

    [Header("Attack Settings")]
    [SerializeField] private float delayBetweenLaunches = 0.3f;

    private bool isAttacking = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartFlyingPartsAttack();
        }
    }

    private void StartFlyingPartsAttack()
    {
        if (!isAttacking)
            StartCoroutine(LaunchAndReturnSequence());
    }

    private IEnumerator LaunchAndReturnSequence()
    {
        isAttacking = true;

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

        isAttacking = false;
    }
}