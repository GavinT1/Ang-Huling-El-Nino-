using UnityEngine;
using UnityEngine.UI;
public class LowHydrationAlert : MonoBehaviour
{
    [Header("References")]
    public PlayerHydration hydrationSystem;
    public Image vignetteImage;
    public GameObject alertBanner;

    [Header("Alert Threshold")]
    [Range(0f, 1f)]
    public float lowHydrationThreshold = 0.20f;

    [Header("pulse Visual Settings")]
    public Color edgeColor = new Color(1f, 0.12f, 0.05f, 1f);
    public float pulseSpeed = 4f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.75f;

    void Start()
    {
        if(vignetteImage !=null)
        {
            if(vignetteImage.sprite == null)
            {
                vignetteImage.sprite = CreateVignetteSprite(256, 256);
            }
            vignetteImage.raycastTarget = false;
            vignetteImage.gameObject.SetActive(false);
        }
        if (alertBanner != null)
        {
            alertBanner.SetActive(false);
        }
    }

    void Update()
    {
        if (hydrationSystem == null ) return;
        float hydrationPct = hydrationSystem.currentHydration / hydrationSystem.maxHydration;
        bool isLow = hydrationPct <= lowHydrationThreshold && hydrationPct >0f;

        if ( alertBanner !=null && alertBanner.activeSelf !=isLow)
        {
            alertBanner.SetActive(isLow);
        }
        if(vignetteImage != null )
        {
            if(isLow)
            {
                if(!vignetteImage.gameObject.activeSelf)
                {
                    vignetteImage.gameObject.SetActive(true);
                }
                
                float wave = (Mathf.Sin(Time.time * pulseSpeed) +1f ) * 0.5f;
                float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

                Color c = edgeColor;
                c.a = currentAlpha;
                vignetteImage.color = c;
            }
            else if (vignetteImage.gameObject.activeSelf)
            {
                vignetteImage.gameObject.SetActive(false);
            }
        }
    }

    private Sprite CreateVignetteSprite(int width, int height)
    {
       Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
       tex.wrapMode = TextureWrapMode.Clamp;

       Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

       for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = ( x - center.x) / center.x;
                float dy = ( y - center.y) / center.y;

                float edgeDist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                float radialDist = Mathf.Sqrt(dx * dx + dy * dy) * 0.75f;
                float combined = Mathf.Lerp(edgeDist, radialDist, 0.5f);

                float alpha = Mathf.InverseLerp(0.42f, 1.0f, combined);
                alpha = Mathf.Pow(alpha, 1.8f); // Smooth curve

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        } 
    
    tex.Apply();
    return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2 (0.5f,0.5f));
    }
}
