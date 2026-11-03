using UnityEngine;
using System.Collections;

public class MudSpawner : MonoBehaviour
{
    public enum GamePhase { Start, Mid, Climax }
    public GamePhase currentPhase = GamePhase.Start;

    [Header("References")]
    public Transform playerTransform; 
    public GameObject mudPrefab;     

    [Header("Map Position Thresholds (Adjust later when map is built)")]
    public float midPhaseXPosition = 15f;     // X coordinate where Mid phase triggers
    public float climaxPhaseXPosition = 35f;  // X coordinate where Climax phase triggers

    [Header("Spawn Intervals (Seconds Between Drops)")]
    public float startInterval = 3.0f;   
    public float midInterval = 1.5f;     
    public float climaxInterval = 0.75f; 

    private float currentInterval;

    void Start()
    {
        currentPhase = GamePhase.Start;
        currentInterval = startInterval;
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        
        if (playerTransform == null) return;

        float playerX = playerTransform.position.x;

        
        if (playerX >= climaxPhaseXPosition && currentPhase != GamePhase.Climax)
        {
            currentPhase = GamePhase.Climax;
            currentInterval = climaxInterval;
            Debug.Log("Entered Climax Phase!");
        }
        else if (playerX >= midPhaseXPosition && playerX < climaxPhaseXPosition && currentPhase == GamePhase.Start)
        {
            currentPhase = GamePhase.Mid;
            currentInterval = midInterval;
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
        Debug.Log($"Spawning mud | Phase: {currentPhase} | Interval: {currentInterval}s");

        
        if (mudPrefab != null && playerTransform != null)
        {
            Vector3 spawnPosition = new Vector3(
                playerTransform.position.x + Random.Range(-4f, 4f), 
                playerTransform.position.y + 6f, 
                0f
            );
            Instantiate(mudPrefab, spawnPosition, Quaternion.identity);
        }
        else if (mudPrefab == null)
        {
            Debug.LogWarning("Mud Prefab is not assigned in the MudSpawner inspector!");
        }
    }
}