using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Default Spawn")]
    public Transform defaultSpawnPoint;
    private Vector3 currentCheckpointPosition;
    private bool hasCheckpoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (defaultSpawnPoint !=null)
        {
            currentCheckpointPosition = defaultSpawnPoint.position;
            hasCheckpoint = true;
        }
    }

    public void SetActiveCheckpoint(Vector3 newPosition)
    {
        currentCheckpointPosition = newPosition;
        hasCheckpoint = true;
        Debug.Log("LAST CHECKPOINT UPDATED TO " + currentCheckpointPosition);
    }

    public Vector3 GetCheckPointPosition()
    {
        if (hasCheckpoint)
        {
            return currentCheckpointPosition;
        }
        return defaultSpawnPoint !=null ? defaultSpawnPoint.position: Vector3.zero;
    }

    public bool HasReachedCheckpoint()
    {
        return hasCheckpoint;
    }

    public Vector3 GetDefaultSpawnPosition()
    {
        return defaultSpawnPoint !=null ? defaultSpawnPoint.position : Vector3.zero;
    }
}

