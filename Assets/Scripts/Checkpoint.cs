using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Custom Spawn Offset")]
    public Transform respawnPoint;

    private Animator anim;
    private bool isActivated = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("FlagTrigger"+other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player tag detected! Updating checkpoint");
            if (!isActivated)
            {
                isActivated = true;
                if(anim!=null)
                {
                    anim.SetTrigger("Activate");
                }
            }
            if (CheckpointManager.Instance !=null)
            {
                CheckpointManager.Instance.SetActiveCheckpoint(respawnPoint.position);
            }
            else
            {
                Debug.LogError("CheckpointManager Instance is MISSING in the scene!");
            }
        }
    }
}