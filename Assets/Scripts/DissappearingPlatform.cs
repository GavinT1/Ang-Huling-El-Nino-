using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class DissappearingPlatform : MonoBehaviour
{
    [Header("Timing Settings")]
    public float timeBeforeDissapear = 1f;
    public float timeBeforeReappear = 3f;

    private Renderer platformRenderer; 
    private Collider2D platformCollider;
    private bool isPlayerOnPlatform = false;

    void Start()
    {
        // Automatically finds TilemapRenderer OR SpriteRenderer
        platformRenderer = GetComponent<TilemapRenderer>();
        if (platformRenderer == null)
        {
            platformRenderer = GetComponent<SpriteRenderer>();
        }

        platformCollider = GetComponent<CompositeCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider2D>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isPlayerOnPlatform)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(DisappearRoutine());
                    break;
                }
            }
        }
    }

    private IEnumerator DisappearRoutine()
    {
        isPlayerOnPlatform = true;

        yield return new WaitForSeconds(timeBeforeDissapear);

        if (platformRenderer != null) platformRenderer.enabled = false;
        if (platformCollider != null) platformCollider.enabled = false;

        yield return new WaitForSeconds(timeBeforeReappear);

        if (platformRenderer != null) platformRenderer.enabled = true;
        if (platformCollider != null) platformCollider.enabled = true;
        isPlayerOnPlatform = false;
    }
}