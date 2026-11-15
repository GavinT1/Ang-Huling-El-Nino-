using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class DissappearingPlatform : MonoBehaviour
{
    
    [Header("Timing Settings")]
    public float timeBeforeDissapear = 1f;
    public float timeBeforeReappear = 3f;

    [Header("Crumble Warning")]
    public float shakeAmount = 0.06f;
    public Color warningColor = new Color(1f, 0.45f, 0.45f, 1f);

    private Renderer platformRenderer; 

    private Collider2D[] platformColliders;
    private Rigidbody2D playerRb;
    private Tilemap platformTilemap;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalLocalPos;
    private Color originalColor = Color.white;
    private bool isPlayerOnPlatform = false;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        platformTilemap = GetComponent<Tilemap>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(platformTilemap !=null )
        {
            platformRenderer = GetComponent<TilemapRenderer>();
            originalColor = platformTilemap.color;
        }
        else if (spriteRenderer !=null)
        {
            platformRenderer = spriteRenderer;
            originalColor = spriteRenderer.color;
        }
        platformColliders = GetComponents<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isPlayerOnPlatform)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    playerRb = collision.rigidbody;
                    StartCoroutine(DisappearRoutine());
                    break;
                }
            }
        }
    }

    private IEnumerator DisappearRoutine()
    {
        isPlayerOnPlatform = true;

        float elapsed =0f;
        while (elapsed < timeBeforeDissapear)
        {
            elapsed += Time.deltaTime;
            Vector2 randomOffset = Random.insideUnitCircle * shakeAmount;
            transform.localPosition = originalLocalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            float pulse = Mathf.PingPong(elapsed * 10f, 1f);
            SetPlatformColor(Color.Lerp(originalColor, warningColor, pulse));

            yield return null;
        }

        transform.localPosition = originalLocalPos;
        SetPlatformColor(originalColor);

        if (platformRenderer != null) platformRenderer.enabled = false;
        foreach ( Collider2D col in platformColliders)
        {
            if (col !=null) col.enabled = false;
        }
        if (playerRb != null)
        {
            playerRb.WakeUp();
        }
        
        yield return new WaitForSeconds(timeBeforeReappear);

        if (platformRenderer != null) platformRenderer.enabled = true;
        foreach (Collider2D col in platformColliders)
        {
            if(col !=null) col.enabled = true;
        }
        isPlayerOnPlatform = false;
    }

    private void SetPlatformColor(Color newColor)
    {
        if (platformTilemap !=null) platformTilemap.color = newColor;
        else if (spriteRenderer !=null) spriteRenderer.color = newColor;
    }
}