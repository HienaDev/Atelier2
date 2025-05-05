using UnityEngine;
using UnityEngine.SceneManagement;


public class Debugger : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BossHealth bossHealth;


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
            bossHealth.SkipPhase();
        }


        if (Input.GetKeyDown(KeyCode.F4))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
