using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject quitMessage;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuUI.activeSelf && !settingsMenuUI.activeSelf && !quitMessage.activeSelf)
            {
                ResumeGame();
            }
            else if (quitMessage.activeSelf)
            {
                CancelQuitGame();
            }
            else if (settingsMenuUI.activeSelf)
            {
                CloseSettingsMenu();
                OpenPauseMenuUI();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
    }

    public void OpenSettingsMenu()
    {
        settingsMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
    }

    public void CloseSettingsMenu()
    {
        settingsMenuUI.SetActive(false);
    }

    public void OpenQuitMessage()
    {
        quitMessage.SetActive(true);
    }

    private void CloseQuitMessage()
    {
        quitMessage.SetActive(false);
    }

    public void ConfirmQuitGame()
    {
        QuitGame();
    }

    public void CancelQuitGame()
    {
        quitMessage.SetActive(false);
    }

    public void QuitGame()
    {
        pauseMenuUI.SetActive(false);

        CloseQuitMessage();

        StartCoroutine(LoadSceneAsync("MainMenu"));

        Time.timeScale = 1f;
    }

    public void OpenPauseMenuUI()
    {
        pauseMenuUI.SetActive(true);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);

        if (loadingText != null) loadingText.text = "Loading... 0%";

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f) * 100;

            if (loadingText != null) loadingText.text = $"Loading... {Mathf.RoundToInt(progress)}%";

            yield return null;
        }

        loadingScreen.SetActive(false);
    }
}