using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause UI Panels")]
    public GameObject pausePanel;      // Drag your main Pause Panel here
    public GameObject settingsPanel;   // Optional: Drag your Settings sub-panel here

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenu"; // Exact name of your Main Menu scene

    [Header("Options")]
    public bool pauseAudioListener = false; // Check if you want game audio to pause too

    public static bool IsPaused { get; private set; }

    private void Start()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If Settings sub-panel is open, pressing Esc goes back to the main Pause buttons
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                // Only pause if the game isn't already frozen by a cutscene video or open note
                if (Time.timeScale > 0f)
                {
                    PauseGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        if (pauseAudioListener) AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Settings Panel is not assigned on PauseMenuManager.");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        // Always reset Time.timeScale before changing scenes so the Main Menu isn't frozen
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}