using UnityEngine;

public class WaterPickup : MonoBehaviour
{
    public float refillAmount = 25f; // Gives back 25% of the bar

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object touching the water has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            PlayerHydration hydration = collision.GetComponent<PlayerHydration>();
            
            if (hydration != null)
            {
                hydration.RefillHydration(refillAmount);
                Destroy(gameObject); // Destroy the water object after drinking
            }
        }
    }
}