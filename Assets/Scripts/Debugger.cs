using UnityEngine;
using static ScorpionBoss;

public class Debugger : MonoBehaviour
{

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private ScorpionBoss scorpionBoss;
    [SerializeField] private MouthBossAttacks mouthBoss;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            playerHealth.DealDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            playerHealth.ToggleInvulnerable();
        }

        if(Input.GetKeyDown(KeyCode.F3))
        {
            bossHealth.DealDamage(250);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Tutorial);
            Debug.Log("Difficulty set to Tutorial");
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Easy);
            Debug.Log("Difficulty set to Easy");
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Normal);
            Debug.Log("Difficulty set to Normal");
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            mouthBoss.NormalDifficulty();
            Debug.Log("Difficulty set to Normal");
        }

        
    }
}
