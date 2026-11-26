using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PanelTriggerZone : MonoBehaviour
{
    [Header("UI Panel to Open")]
    [Tooltip("Drag the UI Panel GameObject from your Canvas here")]
    public GameObject targetPanel;

    [Header("Trigger Behavior")]
    [Tooltip("Pause the game while this panel is open?")]
    public bool pauseGameOnOpen = true;
    [Tooltip("Trigger only once, or every time the player enters?")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }

        if (pauseGameOnOpen)
        {
            Time.timeScale = 0f;
        }
    }

    // Hook this up to a "Continue" or "Close" button on your panel!
    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }

        if (pauseGameOnOpen)
        {
            Time.timeScale = 1f;
        }
    }
}