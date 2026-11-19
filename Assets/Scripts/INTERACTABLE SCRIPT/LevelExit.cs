using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "Area2"; 

    [Header("Visual Feedback (Optional)")]
    public SpriteRenderer exitSprite;
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public Color unlockedColor = new Color(0.4f, 1f, 0.95f, 1f);
    public GameObject unlockedVFX; 

    private bool isUnlocked = false;

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

        if (exitSprite != null)
            exitSprite.color = isUnlocked ? unlockedColor : lockedColor;

        if (unlockedVFX != null)
            unlockedVFX.SetActive(isUnlocked);

        if (isUnlocked)
        {
            Debug.Log("EXIT UNLOCKED! Ready for Scene 2.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isUnlocked)
        {
            
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("SUCCESS! Exit triggered and unlocked, but '" + nextSceneName + "' is not created/added to Build Settings yet.");
            }
        }
        else
        {
            int remaining = JarGoalManager.Instance.requiredJars - JarGoalManager.Instance.collectedJars;
            Debug.Log("Exit locked! Collect " + remaining + " more water jars.");
        }
    }
}