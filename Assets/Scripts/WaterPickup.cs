using UnityEngine;

public class WaterPickup : MonoBehaviour
{
    public float refillAmount = 25f; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            PlayerHydration hydration = collision.GetComponent<PlayerHydration>();
            
            if (hydration != null)
            {
                hydration.RefillHydration(refillAmount);
                Destroy(gameObject); 
            }
        }
    }
}