using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float climbSpeed = 3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if(player != null)
            {
                player.SetNearLadder(true, climbSpeed);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if(player != null)
            {
                player.SetNearLadder(false, 0f);
            }
        }
    }
}
