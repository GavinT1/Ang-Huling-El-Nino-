using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MudSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MudDropZone
    {
        public string zoneName = "Mud Zone";
        public Transform centerPoint;
        public float width = 3f;
        public float spawnInterval = 2f;
        public int dropsPerBurst = 2;
    }

    [Header("References")]
    public GameObject mudPrefab;

    [Header("Fixed Mud Drop Zones (Set Size to 3)")]
    public List<MudDropZone> dropZones = new List<MudDropZone>();

    [Header("Optional Performance Optimization")]
    public Transform playerTransform;
    public float activeDistanceFromPlayer = 25f;

    void Start()
    {
        foreach (MudDropZone zone in dropZones)
        {
            if (zone.centerPoint != null)
            {
                StartCoroutine(ZoneSpawnRoutine(zone));
            }
        }
    }

    IEnumerator ZoneSpawnRoutine(MudDropZone zone)
    {
        while (true)
        {
            yield return new WaitForSeconds(zone.spawnInterval);

            if (mudPrefab == null || zone.centerPoint == null) continue;

            if (playerTransform != null &&
                Mathf.Abs(playerTransform.position.x - zone.centerPoint.position.x) > activeDistanceFromPlayer)
            {
                continue;
            }

            TriggerMudDropInZone(zone);
        }
    }

    void TriggerMudDropInZone(MudDropZone zone)
    {
        for (int i = 0; i < zone.dropsPerBurst; i++)
        {
            float randomX = zone.centerPoint.position.x + Random.Range(-zone.width / 2f, zone.width / 2f);
            float randomY = zone.centerPoint.position.y + Random.Range(0f, 1.5f);

            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);
            Instantiate(mudPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        if (dropZones == null) return;

        Gizmos.color = new Color(0.6f, 0.35f, 0.1f, 0.9f);
        foreach (MudDropZone zone in dropZones)
        {
            if (zone.centerPoint != null)
            {
                Gizmos.DrawWireCube(zone.centerPoint.position, new Vector3(zone.width, 1.5f, 0f));
            }
        }
    }
}