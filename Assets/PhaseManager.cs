using UnityEngine;

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

    [SerializeField] private Phase initialPhase;
    private Phase currentPhase;

    [Header("Phase Everhood")]
    [SerializeField] private GameObject everHoodCamera;
    [SerializeField] private GameObject everHoodBoss;

    [Header("Phase Monkey Hell")]
    [SerializeField] private GameObject monkeyHellCamera;
    [SerializeField] private GameObject monkeyHellBoss;

    [Header("Phase Quark")]
    [SerializeField] private GameObject quarkCamera;

    [Header("Phase Rez")]
    [SerializeField] private GameObject rezCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPhase = initialPhase;

        // Initalize just to avoid nulls but will be replaced right after
        currentCamera = everHoodCamera;

        ChangeCamera(currentPhase);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)) ChangeCamera(Phase.MonkeyHell);
        if (Input.GetKeyDown(KeyCode.E)) ChangeCamera(Phase.Everhood);
        if (Input.GetKeyDown(KeyCode.Q)) ChangeCamera(Phase.Quark);
        if (Input.GetKeyDown(KeyCode.R)) ChangeCamera(Phase.Rez);
    }

    public void ChangeCamera(Phase phase)
    {
        switch (phase)
        {
            case Phase.MonkeyHell:
                currentCamera.SetActive(false);
                currentCamera = monkeyHellCamera;
                currentCamera.SetActive(true);
                break;
            case Phase.Everhood:
                currentCamera.SetActive(false);
                currentCamera = everHoodCamera;
                currentCamera.SetActive(true);
                break;
            case Phase.Quark:
                currentCamera.SetActive(false);
                currentCamera = quarkCamera;
                currentCamera.SetActive(true);
                break;
            case Phase.Rez:
                currentCamera.SetActive(false);
                currentCamera = rezCamera;
                currentCamera.SetActive(true);
                break;
            default:
                Debug.LogError("Invalid phase");
                break;
        }
    }


}
