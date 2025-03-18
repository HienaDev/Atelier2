using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;

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

    [Serializable]
    public class PhaseData
    {
        public int number;
        public GameObject camera;
        public GameObject boss;
        public MonoBehaviour playerMovement;
        public MonoBehaviour playerShooting;
        public GameObject arena;
        public Transform playerSpawnPoint;
    }

    [SerializeField] private BossMorphing bossMorphing;
    [SerializeField] private Phase initialPhase;
    private Phase currentPhase;
    private GameObject currentBoss;
    private MonoBehaviour currentPlayerMovement;
    private MonoBehaviour currentPlayerShooting;
    private GameObject currentArena;

    [Header("Phase Everhood")]
    [SerializeField] private PhaseData phaseEverhood;
    [Header("Phase Monkey Hell")]
    [SerializeField] private PhaseData phaseMonkeyHell;
    [Header("Phase Quark")]
    [SerializeField] private PhaseData phaseQuark;
    [Header("Phase Rez")]
    [SerializeField] private PhaseData phaseRez;

    private Dictionary<Phase, PhaseData> phaseData = new Dictionary<Phase, PhaseData>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        currentPhase = initialPhase;

        // Initialize just to avoid nulls but will be replaced right after
        currentCamera = phaseMonkeyHell.camera;
        currentBoss = phaseMonkeyHell.boss;
        currentPlayerMovement = phaseMonkeyHell.playerMovement;
        currentPlayerShooting = phaseMonkeyHell.playerShooting;
        currentArena = phaseMonkeyHell.arena;   

        // Populate the dictionary with phase data
        phaseData[Phase.MonkeyHell] = phaseMonkeyHell;

        phaseData[Phase.Everhood] = phaseEverhood;

        phaseData[Phase.Quark] = phaseQuark;

        phaseData[Phase.Rez] = phaseRez;


        foreach(PhaseData data in phaseData.Values)
        {
            if(data.camera != null)
                data.camera.SetActive(false);

            if (data.boss != null)
                data.boss.SetActive(false);

            if (data.arena != null)
                data.arena.SetActive(false);
        }


        yield return new WaitForSeconds(0.1f);

        ChangePhaseDictionary(currentPhase);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) ChangePhaseDictionary(Phase.MonkeyHell);
        if (Input.GetKeyDown(KeyCode.E)) ChangePhaseDictionary(Phase.Everhood);
        if (Input.GetKeyDown(KeyCode.Q)) ChangePhaseDictionary(Phase.Quark);
        if (Input.GetKeyDown(KeyCode.R)) ChangePhaseDictionary(Phase.Rez);
    }

    public void ChangePhaseDictionary(Phase phase)
    {
        if (!phaseData.ContainsKey(phase))
        {
            Debug.LogError("Invalid phase");
            return;
        }

        PhaseData data = phaseData[phase];

        if (!bossMorphing.ChangePhase(data.number, data.boss, data.playerSpawnPoint, data.playerMovement, data.playerShooting))
            return;

        if(data.boss == null)
        {
            Debug.LogError("Boss is null");
            return;
        }

        currentCamera.SetActive(false);
        currentCamera = data.camera;
        currentCamera.SetActive(true);

        currentBoss.SetActive(false);
        currentBoss = data.boss;

        currentArena.SetActive(false);
        currentArena = data.arena;
        currentArena.SetActive(true);

        currentPlayerMovement.enabled = false;
        currentPlayerShooting.enabled = false;

        currentPlayerMovement = data.playerMovement;
        currentPlayerShooting = data.playerShooting;

    }

    
}