using UnityEngine;

public class BossHealth : MonoBehaviour
{

    [SerializeField] private int lives = 1000;
    private int currentLives;

    [SerializeField] private int livesToSwapPhase = 900;
    private int currentPhase = 0;

    [SerializeField] private PhaseManager phaseManager;
    
    [SerializeField] private ClearProjectiles projectiles;

    private bool alreadySwapped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLives = lives;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(int damage)
    {
        currentLives -= damage;

        Debug.Log("currentLives = " + currentLives);

        if (currentLives < livesToSwapPhase && !alreadySwapped)
        {
            alreadySwapped = true;
            projectiles.ClearAllProjectiles();
            phaseManager.ChangeToMinotaur();
        }
    }

    public void DealCritDamage()
    {
        DealDamage(30);
    }
}
