using JetBrains.Annotations;
using UnityEngine;

public class WaterPickup : MonoBehaviour
{

    public enum PickupType
    {
        Small,
        Medium,
        Large
    }
    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Small;//20%

    [Header("Audio / fx ")]
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Player"))
        {
            PlayerHydration hydration = collision.GetComponent<PlayerHydration>();
            
            if (hydration != null)
            {
                float restorePercent = GetRestorePercentage();
                hydration.RefillHydration(restorePercent);
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                Destroy(gameObject);
            }
        }
    }

    private float GetRestorePercentage()
    {
        switch (pickupType)
        {
            case PickupType.Large:
            return 100f;

            case PickupType.Medium:
            return 50f;
            
            case PickupType.Small:
            default:
            return 20f;

        }
    }
}