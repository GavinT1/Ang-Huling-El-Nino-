using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HydrationPickupAnimation : MonoBehaviour
{
    [Header("1. Sparkle Cross Flash")]
    public Color sparkleColor = new Color(0.75f, 0.96f, 1f, 1f); // Bright cyan-white
    public Vector3 spawnOffset = new Vector3(0.25f, 0.15f, 0f);
    public float sparkleDuration = 0.2f;
    public float sparkleMaxSize = 0.75f;

    [Header("2. Floating +Amount Popup")]
    public Color popupTextColor = new Color(0.45f, 0.92f, 1f, 1f);
    public float popupFloatDistance = 0.7f;
    public float popupDuration = 0.6f;
    public Font customPixelFont; // Optional: drag your pixel font here if you have one

    private Sprite crossSprite;
    private SpriteRenderer playerSR;

    void Awake()
    {
        crossSprite = CreatePixelCrossSprite(32);
        playerSR = GetComponent<SpriteRenderer>();
    }

    public void PlayCollectEffect(float amountRestored)
    {
        Vector3 worldPos = transform.position + spawnOffset;
        StartCoroutine(SparkleCrossRoutine(worldPos));
        StartCoroutine(FloatingNumberRoutine(worldPos + new Vector3(0f, 0.25f, 0f), "+" + Mathf.RoundToInt(amountRestored)));
    }

    // 1. Crisp 4-point pixel sparkle cross at pickup location
    private IEnumerator SparkleCrossRoutine(Vector3 pos)
    {
        GameObject crossObj = new GameObject("PickupSparkle");
        crossObj.transform.position = pos;

        SpriteRenderer sr = crossObj.AddComponent<SpriteRenderer>();
        sr.sprite = crossSprite;

        if (playerSR != null)
        {
            sr.sharedMaterial = playerSR.sharedMaterial;
            sr.sortingLayerID = playerSR.sortingLayerID;
            sr.sortingOrder = playerSR.sortingOrder + 5;
        }
        else
        {
            sr.sortingOrder = 50;
        }

        sr.color = sparkleColor;

        float elapsed = 0f;
        while (elapsed < sparkleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sparkleDuration);

            // Fast pop-out then shrink
            float scale = Mathf.Sin(t * Mathf.PI) * sparkleMaxSize;
            crossObj.transform.localScale = new Vector3(scale, scale, 1f);

            Color c = sparkleColor;
            c.a = 1f - (t * t);
            sr.color = c;

            yield return null;
        }

        Destroy(crossObj);
    }

    // 2. Floating "+25" pixel popup in world space
    private IEnumerator FloatingNumberRoutine(Vector3 startPos, string textValue)
    {
        GameObject textObj = new GameObject("HydrationPopup");
        textObj.transform.position = startPos;
        textObj.transform.localScale = new Vector3(0.03f, 0.03f, 1f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = textValue;
        tm.fontSize = 64;
        tm.fontStyle = FontStyle.Bold;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = popupTextColor;

        if (customPixelFont != null)
        {
            tm.font = customPixelFont;
            textObj.GetComponent<MeshRenderer>().sharedMaterial = customPixelFont.material;
        }

        MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
        if (playerSR != null)
        {
            mr.sortingLayerID = playerSR.sortingLayerID;
            mr.sortingOrder = playerSR.sortingOrder + 6;
        }

        float elapsed = 0f;
        Vector3 endPos = startPos + new Vector3(0f, popupFloatDistance, 0f);

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popupDuration);

            // Ease-out upward float
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            textObj.transform.position = Vector3.Lerp(startPos, endPos, easeOut);

            Color c = popupTextColor;
            c.a = Mathf.Lerp(1f, 0f, t * t);
            tm.color = c;

            yield return null;
        }

        Destroy(textObj);
    }

    // Generates a sharp 4-point pixel star/cross (+) sprite
    private Sprite CreatePixelCrossSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        int mid = size / 2;
        int armThickness = 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool onHorizontal = Mathf.Abs(y - mid) < armThickness && x > 2 && x < size - 3;
                bool onVertical = Mathf.Abs(x - mid) < armThickness && y > 2 && y < size - 3;
                bool centerGlow = Mathf.Abs(x - mid) < armThickness + 1 && Mathf.Abs(y - mid) < armThickness + 1;

                tex.SetPixel(x, y, (onHorizontal || onVertical || centerGlow) ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }
}