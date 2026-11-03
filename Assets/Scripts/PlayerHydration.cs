using UnityEngine;
using UnityEngine.UI;

public class PlayerHydration : MonoBehaviour
{
    [Header("Hydration Settings")]
    public Image hydrationBar;
    public float maxHydration = 100f;
    public float passiveDrain = 1f;
    public float sprintDrain = 5f;
    public float jumpCost = 10f;

    
    public float currentHydration { get; private set; } 

    void Start()
    {
        currentHydration = maxHydration;
    }

    void Update()
    {
        // Passive drain happens every frame
        currentHydration -= passiveDrain * Time.deltaTime;

        // Keep hydration above 0 and update the UI
        currentHydration = Mathf.Clamp(currentHydration, 0, maxHydration);
        if (hydrationBar != null)
        {
            hydrationBar.fillAmount = currentHydration / maxHydration;
        }
    }


    public void DrainFromSprint()
    {
        currentHydration -= sprintDrain * Time.deltaTime;
    }


    public void DrainFromJump()
    {
        currentHydration -= jumpCost;
    }

    public void RefillHydration(float amount)
    {
        currentHydration += amount;
        currentHydration = Mathf.Clamp(currentHydration, 0, maxHydration);
    }
}
