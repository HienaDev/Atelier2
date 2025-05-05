using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenMethods : MonoBehaviour
{

    [SerializeField] private string menuScene;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        // Pause the game when the death screen is enabled
        Time.timeScale = 0f;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }


}