using UnityEngine;

public class MudProjectile : MonoBehaviour
{
    public float lifetime = 5f; // Failsafe: destroy after 5 seconds

    void Start()
    {
        // Automatically destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy the mud instantly if it hits the ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
        
        // We will add the Player damage check here later
    }
}