using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject extraPanel;

    private void Update()
    {
        if (extraPanel != null && extraPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseExtraPanel();
        }
    }

    public void StartGame()
    {
        if (MainMenuLoadingScreen.Instance != null)
        {
            MainMenuLoadingScreen.Instance.PlayGameWithLoadingScreen();
        }
        else
        {
            SceneManager.LoadScene("Cutscene_Opening");
        }
    }

    public void OpenExtraPanel()
    {
        if (extraPanel != null)
        {
            extraPanel.SetActive(true);
        }
    }

    public void CloseExtraPanel()
    {
        if (extraPanel != null)
        {
            extraPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}