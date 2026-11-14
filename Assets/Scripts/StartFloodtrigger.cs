using UnityEngine;


public class StartFloodtrigger : MonoBehaviour
{
    public RisingWater floodwater;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            floodwater.isRising = true;
            Debug.Log("Player entered the flood trigger. Floodwater is now rising.");    
        }
    }
}
