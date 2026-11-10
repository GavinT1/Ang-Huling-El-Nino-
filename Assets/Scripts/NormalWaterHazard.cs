using UnityEngine;

public class NormalWaterHazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth health = collision.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.InstantDeath();
        }
    }
}
