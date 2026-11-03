using UnityEngine;

public class MudSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject mudPrefab;
    public Transform playerTarget;
    
    public float spawnWidth = 15f;  // Wider for horizontal games
    public float spawnRate = 0.2f;  // Faster to fill the wider screen
    
    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnMud();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnMud()
    {
        if (playerTarget == null || mudPrefab == null) return;

        // Pick a random X position around the player's current location
        float randomX = Random.Range(-spawnWidth, spawnWidth);
        float spawnX = playerTarget.position.x + randomX;
        
        // Keep the Y position locked to this Spawner's height so rain doesn't jump
        float spawnY = transform.position.y;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        Instantiate(mudPrefab, spawnPosition, Quaternion.identity);
    }
}