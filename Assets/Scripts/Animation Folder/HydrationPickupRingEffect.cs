using System.Collections;
using UnityEngine;

public class HydrationPickupRingEffect : MonoBehaviour
{
    [Header("Ring Visual Settings")]
    public Color ringColor = new Color(0.4f, 0.92f, 1f, 1f); // Bright cyan
    public Vector3 feetOffset = new Vector3(0f, -0.25f, 0f);
    public float duration = 0.55f;
    public float maxScaleX = 0.55f;
    public float maxScaleY = 0.14f;
    public float secondRingDelay = 0.1f;

    [Header("Sorting")]
    public string sortingLayerName = "Player";
    public int sortingOrder = 5;

    private Sprite ringSprite;
    private Material unlitMat;

    void Awake()
    {
        ringSprite = CreatePixelRingSprite(64);
        Shader unlitShader = Shader.Find("Sprites/Default");
        if (unlitShader != null)
        {
            unlitMat = new Material(unlitShader);
        }
    }

    public void PlayPickupRings()
    {
        StartCoroutine(SpawnRingRoutine(0f));
        StartCoroutine(SpawnRingRoutine(secondRingDelay));
    }

    private IEnumerator SpawnRingRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        GameObject ringObj = new GameObject("GroundRing");
        ringObj.transform.SetParent(transform);
        ringObj.transform.localPosition = new Vector3(feetOffset.x, feetOffset.y, 0f);

        SpriteRenderer sr = ringObj.AddComponent<SpriteRenderer>();
        sr.sprite = ringSprite;
        if (unlitMat != null) sr.material = unlitMat;
        sr.color = ringColor;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        float elapsed = 0f;
        Vector3 startScale = new Vector3(0.25f, 0.08f, 1f);
        Vector3 endScale = new Vector3(maxScaleX, maxScaleY, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            ringObj.transform.localScale = Vector3.Lerp(startScale, endScale, easeOut);

            Color c = ringColor;
            c.a = Mathf.Lerp(ringColor.a, 0f, t * t);
            sr.color = c;

            yield return null;
        }

        Destroy(ringObj);
    }

    private Sprite CreatePixelRingSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1) * 0.5f;
        float outerRadius = size * 0.47f;
        float innerRadius = size * 0.445f; // Thicker, much more visible band

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                bool inRing = dist <= outerRadius && dist >= innerRadius;
                tex.SetPixel(x, y, inRing ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}