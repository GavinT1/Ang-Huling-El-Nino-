using UnityEngine;
using System.Collections;

public class MudSpawner : MonoBehaviour
{
    public enum GamePhase { Start, Mid, Climax }
    
    [Header("Game Phase state")]
    public GamePhase currentPhase = GamePhase.Start;

    [Header("References")]
    public Transform playerTransform; 
    public GameObject mudPrefab;      

    [Header("Area 1 Boundaries")]
    public float area1MinX = -15f;
    public float area1MaxX = 102.77f;

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
        if (playerX >= climaxPhaseXPosition)
        {
            if(currentPhase != GamePhase.Climax)
            {
            currentPhase = GamePhase.Climax;
            currentInterval = climaxInterval;
            currentDropCount = climaxDropCount;
            Debug.Log("Entered Climax Phase - Mud Storm!");
            }
        }
        else if (playerX >= midPhaseXPosition)
        {
            if(currentPhase != GamePhase.Mid)
            {
            currentPhase = GamePhase.Mid;
            currentInterval = midInterval;
            currentDropCount = midDropCount;
            Debug.Log("Entered Mid Phase!");
            }
        }
        else
        {
            if(currentPhase != GamePhase.Start)
            {
                currentPhase = GamePhase.Start;
                currentInterval = startInterval;
                currentDropCount = startDropCount;
                Debug.Log("Returned to Start Phase.");
            }
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentInterval);

            if(playerTransform != null)
            {
                float playerX = playerTransform.position.x;
                if(playerX >= area1MinX && playerX <= area1MaxX)
                {
                    TriggerMudDrop();
                }
            }
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

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;
        Gizmos.color = Color.green;
        Vector3 boxCenter = new Vector3(playerTransform.position.x, playerTransform.position.y + spawnHeight / 1f, 0f);
        Gizmos.DrawWireCube(boxCenter, new Vector3(spawnWidth, 2f, 0));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(area1MinX, playerTransform.position.y - 10f, 0), new Vector3(area1MinX, playerTransform.position.y + 10f, 0));
        Gizmos.DrawLine(new Vector3(area1MaxX, playerTransform.position.y - 10f, 0), new Vector3(area1MaxX, playerTransform.position.y + 10f, 0));
    }
}