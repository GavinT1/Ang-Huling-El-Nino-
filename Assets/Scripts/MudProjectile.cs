using UnityEngine;

public class MudProjectile : MonoBehaviour
{
    public float lifetime = 5f; // Failsafe: destroy after 5 seconds

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
        
        // We will add the Player damage logic here later when the player has health implemented.
    }
}