using UnityEngine;
using UnityEngine.SceneManagement;


public class Debugger : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private ClearProjectiles clearProjectiles;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            playerHealth.DealDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            playerHealth.DealDamage(-1);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            playerHealth.ToggleInvulnerable();
        }

        if(Input.GetKeyDown(KeyCode.F4))
        {
            bossHealth.SkipPhase();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            clearProjectiles.ClearAllProjectiles(false, true);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }


    }
}
