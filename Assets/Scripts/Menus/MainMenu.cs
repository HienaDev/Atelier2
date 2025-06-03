using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject exitMessage;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private PlayableDirector introCutscene;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;

    private bool hasStartedGame = false;

    private void Start()
    {
        // Remove the continue button if there's no save system
        continueButton.interactable = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenu.activeSelf)
            {
                CloseSettingsMenu();
            }
            else if (creditsMenu.activeSelf)
            {
                CloseCreditsMenu();
            }
            else if (exitMessage.activeSelf)
            {
                CancelExitGame();
            }
            else
            {
                ExitGame();
            }
        }
    }

    public void PlayGame()
    {
        if (!hasStartedGame && introCutscene != null)
        {
            hasStartedGame = true;
            introCutscene.stopped += OnCutsceneFinished;
            introCutscene.Play();
        }
        else
        {
            StartCoroutine(LoadSceneAsync(sceneToLoad));
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        introCutscene.stopped -= OnCutsceneFinished;
        StartCoroutine(StartGameAfterFrame());
    }

    private IEnumerator StartGameAfterFrame()
    {
        loadingScreen.SetActive(true);
        if (loadingText != null) loadingText.text = "Loading... 0%";
        
        yield return null;

        yield return LoadSceneAsync(sceneToLoad);
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
    }

    public void OpenCreditsMenu()
    {
        creditsMenu.SetActive(true);
    }

    public void CloseCreditsMenu()
    {
        creditsMenu.SetActive(false);
    }

    public void ExitGame()
    {
        exitMessage.SetActive(true);
    }

    public void ConfirmExitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CancelExitGame()
    {
        exitMessage.SetActive(false);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f) * 100f;

            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress)}%";

            yield return null;
        }
    }
}