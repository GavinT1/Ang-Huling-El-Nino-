using System;
using UnityEngine;

public class StartFloodtrigger : MonoBehaviour
{
    public RisingWaterPool floodwater;
    public FloodAlertUI floodAlerUI;
    [SerializeField] private bool triggerOnce =true;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(triggerOnce && hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;

            if (floodwater != null)
            {
                floodwater.StartFilling();
                Debug.Log("Player entered the flood trigger. Floodwater is now rising.");
            }

            if(floodAlerUI !=null)
            {
                floodAlerUI.TriggerAlert();
            }
        }
    }
    public void ResetTrigger()
    {
        hasTriggered = false;
        if(floodAlerUI !=null)
        {
            floodAlerUI.HideAlertImmediately();
        }
    }

}