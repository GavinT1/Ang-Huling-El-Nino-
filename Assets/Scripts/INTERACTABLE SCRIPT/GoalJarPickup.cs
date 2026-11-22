using UnityEngine;

public class GoalJarPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    public float bobSpeed = 2.5f;
    public float bobHeight = 0.08f;
    public bool alsoRefillHydration = false; // Keep false so it stays strictly a goal item
    public float hydrationRefillPercent = 15f;

    private Vector3 startPos;
    private bool collected = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Subtle floating bob so Goal Jars stand out from survival water pickups
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (QuestObjectiveTracker.Instance != null &&
            QuestObjectiveTracker.Instance.currentStage == QuestObjectiveTracker.QuestStage.ReadBoard)
            return;
            
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        if (JarGoalManager.Instance != null)
        {
            JarGoalManager.Instance.AddJar();
        }

        // Optional: still give a small hydration boost if desired
        if (alsoRefillHydration)
        {
            PlayerHydration hydration = other.GetComponent<PlayerHydration>();
            if (hydration != null)
            {
                hydration.RefillHydration(hydrationRefillPercent);
            }
        }

        // Optional: trigger your existing ring/sparkle juice on pickup!
        HydrationPickupRingEffect ring = other.GetComponent<HydrationPickupRingEffect>();
        if (ring != null)
        {
            ring.PlayPickupRings();
        }

        Destroy(gameObject);
    }
}