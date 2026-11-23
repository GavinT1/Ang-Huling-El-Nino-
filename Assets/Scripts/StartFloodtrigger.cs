using UnityEngine;

public class StartFloodtrigger : MonoBehaviour
{
    public RisingWaterPool floodwater;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (floodwater != null)
            {
                floodwater.StartFilling();
                Debug.Log("Player entered the flood trigger. Floodwater is now rising.");
            }
        }
    }
}