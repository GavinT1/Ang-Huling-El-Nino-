using UnityEngine;

public class StoryItemPickup : MonoBehaviour
{
    public StorySequenceController storyController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (storyController != null)
            {
                storyController.CollectItem();
            }

            gameObject.SetActive(false); // Hide the box once picked up
        }
    }
}