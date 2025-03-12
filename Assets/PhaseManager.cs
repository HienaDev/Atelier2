using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PhaseManager : MonoBehaviour
{

    public enum Phase
    {
        MonkeyHell,
        Everhood,
        Quark,
        Rez
    }

    private GameObject currentCamera;

    [SerializeField] private BossMorphing bossMorphing;
    [SerializeField] private Phase initialPhase;
    private Phase currentPhase;
    private GameObject currentBoss;
    private MonoBehaviour currentPlayerMovement;
    private MonoBehaviour currentPlayerShooting;

    [Header("Phase Everhood")]
    [SerializeField] private int everHoodNumber;
    [SerializeField] private GameObject everHoodCamera;
    [SerializeField] private GameObject everHoodBoss;
    [SerializeField] private MonoBehaviour everHoodPlayerMovement;
    [SerializeField] private MonoBehaviour everHoodPlayerShooting;

    [Header("Phase Monkey Hell")]
    [SerializeField] private int monkeyHellNumber;
    [SerializeField] private GameObject monkeyHellCamera;
    [SerializeField] private GameObject monkeyHellBoss;
    [SerializeField] private MonoBehaviour monkeyHellPlayerMovement;
    [SerializeField] private MonoBehaviour monkeyHellPlayerShooting;

    [Header("Phase Quark")]
    [SerializeField] private int quarkNumber;
    [SerializeField] private GameObject quarkCamera;
    [SerializeField] private GameObject quarkBoss;
    [SerializeField] private MonoBehaviour quarkPlayerMovement;
    [SerializeField] private MonoBehaviour quarkPlayerShooting;

    [Header("Phase Rez")]
    [SerializeField] private int rezNumber;
    [SerializeField] private GameObject rezCamera;
    [SerializeField] private GameObject rezBoss;
    [SerializeField] private MonoBehaviour rezPlayerMovement;
    [SerializeField] private MonoBehaviour rezPlayerShooting;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        currentPhase = initialPhase;

        // Initalize just to avoid nulls but will be replaced right after
        currentCamera = monkeyHellCamera;
        currentBoss = monkeyHellBoss;
        currentPlayerMovement = monkeyHellPlayerMovement;
        currentPlayerShooting = monkeyHellPlayerShooting;

        yield return new WaitForSeconds(0.1f);

        ChangePhase(currentPhase);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)) ChangePhase(Phase.MonkeyHell);
        if (Input.GetKeyDown(KeyCode.E)) ChangePhase(Phase.Everhood);
        if (Input.GetKeyDown(KeyCode.Q)) ChangePhase(Phase.Quark);
        if (Input.GetKeyDown(KeyCode.R)) ChangePhase(Phase.Rez);
    }

    public void ChangePhase(Phase phase)
    {
        switch (phase)
        {
            case Phase.MonkeyHell:
                if (!bossMorphing.ChangePhase(monkeyHellNumber))
                    return;

                currentCamera.SetActive(false);
                currentCamera = monkeyHellCamera;
                currentCamera.SetActive(true);

                currentBoss.SetActive(false);
                monkeyHellBoss.SetActive(true);
                currentBoss = monkeyHellBoss;

                currentPlayerMovement.enabled = false;
                currentPlayerShooting.enabled = false;

                currentPlayerMovement = monkeyHellPlayerMovement;
                currentPlayerShooting = monkeyHellPlayerShooting;

                currentPlayerMovement.enabled = true;
                currentPlayerShooting.enabled = true;


                break;
            case Phase.Everhood:

                if (!bossMorphing.ChangePhase(everHoodNumber))
                    return;

                currentCamera.SetActive(false);
                currentCamera = everHoodCamera;
                currentCamera.SetActive(true);

                currentBoss.SetActive(false);
                everHoodBoss.SetActive(true);
                currentBoss = everHoodBoss;

                currentPlayerMovement.enabled = false;
                currentPlayerShooting.enabled = false;

                currentPlayerMovement = everHoodPlayerMovement;
                currentPlayerShooting = everHoodPlayerShooting;

                currentPlayerMovement.enabled = true;
                currentPlayerShooting.enabled = true;
                break;
            case Phase.Quark:

                if (!bossMorphing.ChangePhase(quarkNumber))
                    return;

                currentCamera.SetActive(false);
                currentCamera = quarkCamera;
                currentCamera.SetActive(true);

                currentBoss.SetActive(false);
                quarkBoss.SetActive(true);
                currentBoss = quarkBoss;

                currentPlayerMovement.enabled = false;
                currentPlayerShooting.enabled = false;

                currentPlayerMovement = quarkPlayerMovement;
                currentPlayerShooting = quarkPlayerShooting;

                currentPlayerMovement.enabled = true;
                currentPlayerShooting.enabled = true;

                break;
            case Phase.Rez:

                if (!bossMorphing.ChangePhase(rezNumber))
                    return;

                currentCamera.SetActive(false);
                currentCamera = rezCamera;
                currentCamera.SetActive(true);

                currentBoss.SetActive(false);
                rezBoss.SetActive(true);
                currentBoss = rezBoss;

                currentPlayerMovement.enabled = false;
                currentPlayerShooting.enabled = false;

                currentPlayerMovement = rezPlayerMovement;
                currentPlayerShooting = rezPlayerShooting;

                currentPlayerMovement.enabled = true;
                currentPlayerShooting.enabled = true;
                break;
            default:
                Debug.LogError("Invalid phase");
                break;
        }
    }


}
