using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenMethods : MonoBehaviour
{
    [SerializeField] private string menuScene;
    [SerializeField] private string gameScene;

    private void OnEnable()
    {
        // Pause the game when the death screen is enabled
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            //RestartGame();
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