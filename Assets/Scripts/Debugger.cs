using UnityEngine;
using static ScorpionBoss;

public class Debugger : MonoBehaviour
{

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private ScorpionBoss scorpionBoss;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Tutorial);
            Debug.Log("Difficulty set to Tutorial");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Easy);
            Debug.Log("Difficulty set to Easy");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            scorpionBoss.SetDifficulty(BossDifficulty.Normal);
            Debug.Log("Difficulty set to Normal");
        }
    }
}
