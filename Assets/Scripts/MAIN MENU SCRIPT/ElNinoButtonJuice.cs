using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ElNinoButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Pop & Wind Tilt")]
    public float hoverScale = 1.08f;
    public float clickScale = 0.94f;
    public float hoverTiltAngle = 2.2f;
    public float smoothSpeed = 16f;

    [Header("Hover Glow Tint")]
    public Color normalTint = Color.white;
    public Color hoverTint = new Color(1.15f, 1.08f, 0.95f, 1f);

    [Header("Animated Pixel FX (Fire / Dust Sprite Frames)")]
    public Image hoverEffectImage;     // Drag PlayFireFX here
    public Sprite[] hoverEffectFrames; // Drag Fire_0, Fire_1, Fire_2... here
    public float effectFPS = 12f;

    private RectTransform rect;
    private Image buttonImage;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private Quaternion targetRotation;
    private Color targetColor;
    private bool isHovered = false;
    private float frameTimer = 0f;
    private int currentFrame = 0;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        baseScale = rect.localScale;
        targetScale = baseScale;
        targetRotation = Quaternion.identity;
        targetColor = normalTint;

        if (hoverEffectImage != null)
            hoverEffectImage.gameObject.SetActive(false);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime * smoothSpeed;
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, dt);
        rect.localRotation = Quaternion.Lerp(rect.localRotation, targetRotation, dt);

        if (buttonImage != null)
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, dt);

        // Cycle pixel-art fire/dust frames while hovered
        if (isHovered && hoverEffectImage != null && hoverEffectFrames != null && hoverEffectFrames.Length > 0)
        {
            frameTimer += Time.unscaledDeltaTime;
            if (frameTimer >= 1f / effectFPS)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % hoverEffectFrames.Length;
                hoverEffectImage.sprite = hoverEffectFrames[currentFrame];
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = baseScale * hoverScale;
        targetRotation = Quaternion.Euler(0f, 0f, hoverTiltAngle);
        targetColor = hoverTint;

        if (hoverEffectImage != null && hoverEffectFrames != null && hoverEffectFrames.Length > 0)
        {
            currentFrame = 0;
            frameTimer = 0f;
            hoverEffectImage.sprite = hoverEffectFrames[0];
            hoverEffectImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = baseScale;
        targetRotation = Quaternion.identity;
        targetColor = normalTint;

        if (hoverEffectImage != null)
            hoverEffectImage.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = baseScale * clickScale;
        targetRotation = Quaternion.identity;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isHovered ? baseScale * hoverScale : baseScale;
        targetRotation = isHovered ? Quaternion.Euler(0f, 0f, hoverTiltAngle) : Quaternion.identity;
    }
}