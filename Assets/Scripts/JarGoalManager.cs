using UnityEngine;
using UnityEngine.UI; 
using TMPro;          

public class JarGoalManager : MonoBehaviour
{
    public static JarGoalManager Instance;

    [Header("Goal Settings")]
    public int requiredJars = 10;
    public int collectedJars { get; private set; }

    [Header("Exit Reference")]
    public LevelExit levelExit; 

    [Header("Optional UI Counter")]
    public TextMeshProUGUI jarCounterTMP; 
    public Text legacyJarCounterText;    

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        if (levelExit != null)
        {
            levelExit.SetUnlocked(false);
        }
    }

    public void AddJar()
    {
        collectedJars++;
        UpdateUI();

        if (collectedJars >= requiredJars && levelExit != null)
        {
            levelExit.SetUnlocked(true);
        }
    }

    public void ResetProgress()
    {
        collectedJars = 0;
        UpdateUI();
        if (levelExit != null)
        {
            levelExit.SetUnlocked(false);
        }
    }

    private void UpdateUI()
    {
        string label = collectedJars + " / " + requiredJars;
        if (jarCounterTMP != null) jarCounterTMP.text = label;
        if (legacyJarCounterText != null) legacyJarCounterText.text = label;
    }
}