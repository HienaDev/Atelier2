using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenMethods : MonoBehaviour
{
    [SerializeField] private string menuScene;
    [SerializeField] private string gameScene;

    [SerializeField] private bool restartWithKey = false;

    private void OnEnable()
    {
        // Pause the game when the death screen is enabled
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U) && restartWithKey)
        {
            RestartGame();
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    public void BackToGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}