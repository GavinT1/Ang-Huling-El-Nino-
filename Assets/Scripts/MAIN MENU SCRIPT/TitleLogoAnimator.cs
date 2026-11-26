using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleLogoAnimator : MonoBehaviour
{
    [Header("Entrance Impact Slam")]
    public bool playEntrance = true;
    public float entranceDuration = 0.55f;
    public float startScaleMultiplier = 1.35f;
    public float dropOffsetY = 65f;

    [Header("Idle Floating & Breathing")]
    public float floatDistance = 8f;
    public float floatSpeed = 1.6f;
    public float breathScaleAmount = 0.025f;
    public float breathSpeed = 1.9f;
    public float tiltAngle = 1.2f;
    public float tiltSpeed = 1.1f;

    [Header("Fiery / Heat Pulse Glow")]
    public bool enableHeatPulse = true;
    public Color baseColor = Color.white;
    public Color emberFlashColor = new Color(1f, 0.86f, 0.72f, 1f);
    public float heatPulseSpeed = 2.5f;

    private RectTransform rect;
    private Image logoImage;
    private CanvasGroup canvasGroup;

    private Vector2 baseAnchoredPos;
    private Vector3 baseScale;
    private bool isReady = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        logoImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (rect != null)
        {
            baseAnchoredPos = rect.anchoredPosition;
            baseScale = rect.localScale;
        }

        if (playEntrance)
        {
            StartCoroutine(EntranceRoutine());
        }
        else
        {
            isReady = true;
        }
    }

    private IEnumerator EntranceRoutine()
    {
        canvasGroup.alpha = 0f;
        Vector2 startPos = baseAnchoredPos + new Vector2(0f, dropOffsetY);
        Vector3 startScale = baseScale * startScaleMultiplier;

        rect.anchoredPosition = startPos;
        rect.localScale = startScale;

        float elapsed = 0f;
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);

            // Smooth ease-out with a slight heavy punch at the end
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = Mathf.Clamp01(t * 2.2f);
            rect.anchoredPosition = Vector2.Lerp(startPos, baseAnchoredPos, easeOut);
            rect.localScale = Vector3.Lerp(startScale, baseScale, easeOut);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.anchoredPosition = baseAnchoredPos;
        rect.localScale = baseScale;
        isReady = true;
    }

    private void Update()
    {
        if (!isReady || rect == null) return;

        float t = Time.unscaledTime;

        // 1. Smooth vertical hover
        float offsetY = Mathf.Sin(t * floatSpeed) * floatDistance;
        rect.anchoredPosition = baseAnchoredPos + new Vector2(0f, offsetY);

        // 2. Subtle breathing scale pulse
        float scaleOffset = Mathf.Sin(t * breathSpeed) * breathScaleAmount;
        rect.localScale = baseScale * (1f + scaleOffset);

        // 3. Very gentle wind tilt
        float zRot = Mathf.Sin(t * tiltSpeed) * tiltAngle;
        rect.localRotation = Quaternion.Euler(0f, 0f, zRot);

        // 4. Warm ember glow tint pulse
        if (enableHeatPulse && logoImage != null)
        {
            float heat = (Mathf.Sin(t * heatPulseSpeed) + 1f) * 0.5f;
            logoImage.color = Color.Lerp(baseColor, emberFlashColor, heat * 0.6f);
        }
    }
}