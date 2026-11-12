using UnityEngine;

public class CloudDrift : MonoBehaviour
{
    [Header("Movement Settings")]
    public float driftSpeed = 0.5f;

    [Header("Area 1 Boundaries (Local X)")]
    public float leftLimit = -20f;
    public float rightLimit = 20f;
    public float fadeDistance = 3f; // Distance over which clouds fade in/out at edges

    [Header("Respawn Randomization")]
    public float minY = -1.5f;
    public float maxY = 1.5f;
    [Range(0f, 1f)] public float maxAlpha = 0.85f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.localPosition += Vector3.right * driftSpeed * Time.deltaTime;

        // Respawn at the opposite side with a new height
        if (driftSpeed > 0 && transform.localPosition.x > rightLimit)
        {
            RespawnCloud(leftLimit);
        }
        else if (driftSpeed < 0 && transform.localPosition.x < leftLimit)
        {
            RespawnCloud(rightLimit);
        }

        UpdateEdgeFade();
    }

    private void RespawnCloud(float spawnX)
    {
        float randomY = Random.Range(minY, maxY);
        transform.localPosition = new Vector3(spawnX, randomY, transform.localPosition.z);
    }

    private void UpdateEdgeFade()
    {
        if (spriteRenderer == null || fadeDistance <= 0f) return;

        float distFromLeft = Mathf.Abs(transform.localPosition.x - leftLimit);
        float distFromRight = Mathf.Abs(rightLimit - transform.localPosition.x);
        float edgeDist = Mathf.Min(distFromLeft, distFromRight);

        float alpha = Mathf.Clamp01(edgeDist / fadeDistance) * maxAlpha;

        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 leftLocal = new Vector3(leftLimit, transform.localPosition.y, 0f);
        Vector3 rightLocal = new Vector3(rightLimit, transform.localPosition.y, 0f);

        Vector3 leftWorld = transform.parent != null ? transform.parent.TransformPoint(leftLocal) : leftLocal;
        Vector3 rightWorld = transform.parent != null ? transform.parent.TransformPoint(rightLocal) : rightLocal;

        Gizmos.DrawLine(leftWorld, rightWorld);
    }
}