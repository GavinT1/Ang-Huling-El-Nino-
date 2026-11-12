using UnityEngine;
using UnityEngine.UI;

public class PlayerHydration : MonoBehaviour
{
    [Header("Hydration Settings")]
    public Image hydrationBar;
    public float maxHydration = 100f;
    public float passiveDrain = 1f;
    public float sprintDrain = 5f;
    public float jumpCost = 3f;

    public float timeToDie = 5f;
    private float DeathTimer;
    public float currentHydration { get; private set; } 
    private PlayerHealth health;
    void Start()
    {
        currentHydration = maxHydration;
        DeathTimer = timeToDie;
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if(health !=null && health.isDead) return;
        currentHydration -= passiveDrain * Time.deltaTime;
        currentHydration = Mathf.Clamp(currentHydration, 0, maxHydration);

        if(hydrationBar != null)
        {
            hydrationBar.fillAmount = currentHydration / maxHydration;
        }

        if (currentHydration <= 0)
        {
            DeathTimer -= Time.deltaTime;
            if (DeathTimer <= 0)
            {
                PlayerHealth health = GetComponent<PlayerHealth>();
                if (health != null && !health.isDead)
                {
                   health.InstantDeath(); // Inflict fatal damage
                }
            }
        }
        else
        {
            DeathTimer = timeToDie;
        }

    }


    public void DrainFromSprint(float multiplier = 1f)
    {
        currentHydration -= sprintDrain * multiplier * Time.deltaTime;
        currentHydration = Mathf.Clamp(currentHydration, 0, maxHydration);
    }


    public void DrainFromJump()
    {
        currentHydration -= jumpCost;
    }

    public void RefillHydration(float percentage)
    {

        float amountToRestore = maxHydration * (percentage / 100f);
        currentHydration += amountToRestore;
        currentHydration = Mathf.Clamp(currentHydration, 0, maxHydration);

        Debug.Log("Hydration refilled by " + amountToRestore + ". Current Hydration: " + currentHydration);
    }

    public void ResetHydration()
    {
        currentHydration = maxHydration;
        DeathTimer = timeToDie;

        if(hydrationBar !=null)
        {
            hydrationBar.fillAmount = 1f;
        }
    }
}
