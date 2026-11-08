using UnityEngine;
using System.Collections;

public class DissappearingPlatform : MonoBehaviour
{
    [Header("Timing Settings")]
    public float timeBeforeDissapear = 1f;
    public float timeBeforeReappear = 3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private bool isPlayerOnPlatform = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
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
        spriteRenderer.enabled = false;
        platformCollider.enabled = false;

        yield return new WaitForSeconds(timeBeforeDissapear);
        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        isPlayerOnPlatform = false;
    }
}
