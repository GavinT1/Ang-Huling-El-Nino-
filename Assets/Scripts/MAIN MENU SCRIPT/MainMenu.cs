using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject extraPanel;

    
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene"); 
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