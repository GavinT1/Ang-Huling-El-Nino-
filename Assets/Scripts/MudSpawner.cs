using UnityEngine;
using System.Collections;

public class MudSpawner : MonoBehaviour
{
    public enum GamePhase { Start, Mid, Climax }
    public GamePhase currentPhase = GamePhase.Start;

    [Header("References")]
    public Transform playerTransform; 
    public GameObject mudPrefab;      

    [Header("Map Position Thresholds")]
    public float midPhaseXPosition = 15f;     
    public float climaxPhaseXPosition = 35f;  

    [Header("Spawn Intervals (Delay Between Bursts)")]
    public float startInterval = 2.5f;   
    public float midInterval = 1.5f;     
    public float climaxInterval = 0.8f; 

    [Header("Drops Per Burst (How many drop at once!)")]
    public int startDropCount = 2;   
    public int midDropCount = 3;     
    public int climaxDropCount = 4; 

    [Header("Spawn Area Setup")]
    public float spawnWidth = 8f;   // Horizontal area around player (Left to Right)
    public float spawnHeight = 7f;  // Height above player where mud drops from

    private float currentInterval;
    private int currentDropCount;

    void Start()
    {
        currentPhase = GamePhase.Start;
        currentInterval = startInterval;
        currentDropCount = startDropCount;
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (playerTransform == null) return;

        float playerX = playerTransform.position.x;

        // Dynamic difficulty phase changes based on player position
        if (playerX >= climaxPhaseXPosition && currentPhase != GamePhase.Climax)
        {
            currentPhase = GamePhase.Climax;
            currentInterval = climaxInterval;
            currentDropCount = climaxDropCount;
            Debug.Log("Entered Climax Phase - Mud Storm!");
        }
        else if (playerX >= midPhaseXPosition && playerX < climaxPhaseXPosition && currentPhase == GamePhase.Start)
        {
            currentPhase = GamePhase.Mid;
            currentInterval = midInterval;
            currentDropCount = midDropCount;
            Debug.Log("Entered Mid Phase!");
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentInterval);
            TriggerMudDrop();
        }
    }

    void TriggerMudDrop()
    {
        if (mudPrefab == null || playerTransform == null)
        {
            Debug.LogWarning("Missing MudPrefab or PlayerTransform!");
            return;
        }

        // Spawn a batch/burst of multiple drops across the area
        for (int i = 0; i < currentDropCount; i++)
        {
            // Pick a random X position around the player within spawnWidth
            float randomX = playerTransform.position.x + Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
            
            // Slightly stagger the Y starting height so they don't fall in a perfectly flat line
            float randomY = playerTransform.position.y + spawnHeight + Random.Range(0f, 2f);

            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

            Instantiate(mudPrefab, spawnPosition, Quaternion.identity);
        }

        Debug.Log($"Spawned {currentDropCount} mud drops | Phase: {currentPhase}");
    }
}