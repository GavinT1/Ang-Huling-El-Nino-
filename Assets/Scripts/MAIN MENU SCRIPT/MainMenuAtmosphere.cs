using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAtmosphere : MonoBehaviour
{
    [Header("Optional: Drag your BACKGROUND UI object here")]
    public RectTransform backgroundRect;

    [Header("Visible Effects")]
    public int emberCount = 55;
    public int windStreakCount = 14;
    public int ashFlakeCount = 25;

    private class ParticleItem
    {
        public RectTransform rect;
        public Image img;
        public float speedX;
        public float speedY;
        public float wobbleFreq;
        public float wobbleAmp;
        public float phase;
        public float baseAlpha;
        public int type; // 0 = ember, 1 = wind streak, 2 = dark ash
    }

    private readonly List<ParticleItem> items = new List<ParticleItem>();
    private RectTransform fxRoot;
    private Sprite circleSprite;
    private Sprite streakSprite;

    private void Start()
    {
        if (backgroundRect != null)
        {
            backgroundRect.localScale = Vector3.one;
            backgroundRect.anchoredPosition = Vector2.zero;
        }

        Canvas targetCanvas = GetComponentInParent<Canvas>();
        if (targetCanvas == null) targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogError("MainMenuAtmosphere: No Canvas found in the scene!");
            return;
        }

        circleSprite = CreateCircleSprite(32);
        streakSprite = CreateStreakSprite(64, 8);

        // Build FX layer inside the Canvas, right above BACKGROUND
        GameObject rootObj = new GameObject("MenuAtmosphereFX", typeof(RectTransform));
        rootObj.transform.SetParent(targetCanvas.transform, false);

        if (backgroundRect != null && backgroundRect.parent == targetCanvas.transform)
        {
            rootObj.transform.SetSiblingIndex(backgroundRect.GetSiblingIndex() + 1);
        }
        else
        {
            // Place right after the first child (usually BACKGROUND) so it sits behind Title & Buttons
            int index = targetCanvas.transform.childCount > 1 ? 1 : 0;
            rootObj.transform.SetSiblingIndex(index);
        }

        fxRoot = rootObj.GetComponent<RectTransform>();
        fxRoot.anchorMin = Vector2.zero;
        fxRoot.anchorMax = Vector2.one;
        fxRoot.offsetMin = Vector2.zero;
        fxRoot.offsetMax = Vector2.zero;

        for (int i = 0; i < windStreakCount; i++) SpawnItem(1);
        for (int i = 0; i < ashFlakeCount; i++) SpawnItem(2);
        for (int i = 0; i < emberCount; i++) SpawnItem(0);

        Debug.Log($"MainMenuAtmosphere active! Spawned {items.Count} particles inside {targetCanvas.name}.");
    }

    private void SpawnItem(int type)
    {
        GameObject obj = new GameObject($"FX_{type}", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(fxRoot, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        Image img = obj.GetComponent<Image>();
        img.raycastTarget = false;

        ParticleItem p = new ParticleItem
        {
            rect = rect,
            img = img,
            type = type,
            phase = Random.Range(0f, 100f)
        };

        if (type == 0) // Bright glowing fire embers
        {
            float size = Random.Range(6f, 15f);
            rect.sizeDelta = new Vector2(size, size);
            img.sprite = circleSprite;
            p.speedX = Random.Range(45f, 130f);
            p.speedY = Random.Range(55f, 150f);
            p.wobbleFreq = Random.Range(2f, 4.5f);
            p.wobbleAmp = Random.Range(25f, 65f);
            p.baseAlpha = Random.Range(0.75f, 1f);
            img.color = Random.value > 0.3f
                ? new Color(1f, 0.72f, 0.2f, p.baseAlpha)   // Bright gold-orange spark
                : new Color(1f, 0.95f, 0.65f, p.baseAlpha); // Hot white-yellow spark
        }
        else if (type == 1) // Desert wind dust streaks
        {
            rect.sizeDelta = new Vector2(Random.Range(180f, 420f), Random.Range(3f, 7f));
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(3f, 9f));
            img.sprite = streakSprite;
            p.speedX = Random.Range(320f, 620f);
            p.speedY = Random.Range(15f, 45f);
            p.wobbleFreq = 0.8f;
            p.wobbleAmp = 10f;
            p.baseAlpha = Random.Range(0.14f, 0.28f);
            img.color = new Color(1f, 0.82f, 0.58f, p.baseAlpha);
        }
        else // Dark burnt ash flakes
        {
            float size = Random.Range(7f, 14f);
            rect.sizeDelta = new Vector2(size, size * 0.6f);
            img.sprite = circleSprite;
            p.speedX = Random.Range(60f, 150f);
            p.speedY = Random.Range(-25f, 35f);
            p.wobbleFreq = Random.Range(1.5f, 3f);
            p.wobbleAmp = Random.Range(20f, 50f);
            p.baseAlpha = Random.Range(0.45f, 0.75f);
            img.color = new Color(0.16f, 0.08f, 0.05f, p.baseAlpha);
        }

        rect.anchoredPosition = new Vector2(Random.Range(-1050f, 1050f), Random.Range(-560f, 560f));
        items.Add(p);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;

        for (int i = 0; i < items.Count; i++)
        {
            ParticleItem p = items[i];
            Vector2 pos = p.rect.anchoredPosition;

            pos.x += (p.speedX + Mathf.Sin(t * p.wobbleFreq + p.phase) * p.wobbleAmp) * dt;
            pos.y += p.speedY * dt;

            if (pos.x > 1180f) pos.x = -1180f;
            if (pos.y > 600f)
            {
                pos.y = -600f;
                pos.x = Random.Range(-1050f, 1050f);
            }
            else if (pos.y < -600f)
            {
                pos.y = 600f;
            }

            p.rect.anchoredPosition = pos;

            if (p.type == 0)
            {
                float twinkle = 0.6f + 0.4f * Mathf.Sin(t * 6f + p.phase);
                Color c = p.img.color;
                c.a = p.baseAlpha * twinkle;
                p.img.color = c;
            }
        }
    }

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(1f - d, 1.6f)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateStreakSprite(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            float ny = 1f - Mathf.Abs((y / (float)(h - 1)) * 2f - 1f);
            for (int x = 0; x < w; x++)
            {
                float nx = Mathf.Sin((x / (float)(w - 1)) * Mathf.PI);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, nx * ny));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
    }
}