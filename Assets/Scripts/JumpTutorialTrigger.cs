using UnityEngine;
using UnityEngine.Video;

public class JumpTutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialVideoPopup tutorialPopup;
    [Tooltip("Optional: assign a specific clip here so you can reuse the same UI popup for different jumps")]
    [SerializeField] private VideoClip jumpClip;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnlyOnce && hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            if (tutorialPopup != null)
            {
                tutorialPopup.OpenPopup(jumpClip);
            }
        }
    }
}