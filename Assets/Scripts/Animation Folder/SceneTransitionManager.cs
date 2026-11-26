using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneTransitionManager : MonoBehaviour
{
    public enum TransitionStyle
    {
        CinematicLetterboxTitle,
        CircleIrisWipe,
        DiagonalCurtainWipe,
        SmoothFadeWithTitle
    }

    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Style")]
    public TransitionStyle style = TransitionStyle.CinematicLetterboxTitle;
    public Color transitionColor = Color.black;

    [Header("Timing")]
    public float closeDuration = 0.85f;
    public float holdDuration = 0.75f;
    public float openDuration = 0.85f;

    [Header("Chapter / Area Title Card")]
    public bool showTitleCard = true;
    public string defaultTitle = "AREA 1";
    public string defaultSubtitle = "The Cave Entrance";
    public bool fadeAudioDuringTransition = true;

    private Canvas transitionCanvas;
    private RectTransform topBar;
    private RectTransform bottomBar;
    private RectTransform wipePanel;
    private Image wipeImage;
    private CanvasGroup overlayGroup;
    private CanvasGroup titleGroup;
    private TMP_Text titleText;
    private TMP_Text subtitleText;

    private bool isTransitioning = false;
    private float originalVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildTransitionUI();
    }

    private void BuildTransitionUI()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform, false);

        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Fullscreen Fade / Wipe Layer
        GameObject overlayObj = new GameObject("Overlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        overlayObj.transform.SetParent(canvasObj.transform, false);
        wipePanel = overlayObj.GetComponent<RectTransform>();
        wipePanel.anchorMin = Vector2.zero;
        wipePanel.anchorMax = Vector2.one;
        wipePanel.offsetMin = Vector2.zero;
        wipePanel.offsetMax = Vector2.zero;

        wipeImage = overlayObj.GetComponent<Image>();
        wipeImage.color = transitionColor;
        wipeImage.raycastTarget = false;

        overlayGroup = overlayObj.GetComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;

        // Top & Bottom Cinema Bars (for CinematicLetterboxTitle)
        topBar = CreateBar(canvasObj.transform, "TopCinemaBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        bottomBar = CreateBar(canvasObj.transform, "BottomCinemaBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));

        // Center Title Card (e.g., "AREA 1")
        GameObject titleHolder = new GameObject("TitleCard", typeof(RectTransform), typeof(CanvasGroup));
        titleHolder.transform.SetParent(canvasObj.transform, false);
        RectTransform titleHolderRect = titleHolder.GetComponent<RectTransform>();
        titleHolderRect.anchorMin = Vector2.zero;
        titleHolderRect.anchorMax = Vector2.one;
        titleHolderRect.offsetMin = Vector2.zero;
        titleHolderRect.offsetMax = Vector2.zero;

        titleGroup = titleHolder.GetComponent<CanvasGroup>();
        titleGroup.alpha = 0f;
        titleGroup.blocksRaycasts = false;

        titleText = CreateCenteredText(titleHolder.transform, "MainTitle", defaultTitle, 64f, FontStyles.Bold, new Vector2(0f, 24f), Color.white);
        subtitleText = CreateCenteredText(titleHolder.transform, "SubTitle", defaultSubtitle, 28f, FontStyles.Normal, new Vector2(0f, -34f), new Color(1f, 1f, 1f, 0.75f));
    }

    private RectTransform CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 0f);

        Image img = bar.GetComponent<Image>();
        img.color = transitionColor;
        img.raycastTarget = false;
        return rect;
    }

    private TMP_Text CreateCenteredText(Transform parent, string name, string content, float fontSize, FontStyles fontStyle, Vector2 offset, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(1200f, 100f);

        TMP_Text tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.characterSpacing = 12f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    // Call with just sceneName (uses defaultTitle) or pass a custom area title!
    public void TransitionToScene(string sceneName, string customTitle = null, string customSubtitle = null)
    {
        if (isTransitioning) return;

        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(customTitle) ? defaultTitle : customTitle;
        if (subtitleText != null)
            subtitleText.text = customSubtitle != null ? customSubtitle : defaultSubtitle;

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        overlayGroup.blocksRaycasts = true;
        Time.timeScale = 1f;
        originalVolume = AudioListener.volume;

        PrepareVisualState();

        // 1. CLOSE TRANSITION + FADE OUT AUDIO
        float elapsed = 0f;
        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            ApplyTransitionProgress(t, true);
            if (fadeAudioDuringTransition) AudioListener.volume = Mathf.Lerp(originalVolume, 0f, t);
            yield return null;
        }
        ApplyTransitionProgress(1f, true);
        if (fadeAudioDuringTransition) AudioListener.volume = 0f;

        // 2. FADE IN AREA TITLE CARD WHILE LOADING IN BACKGROUND
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        if (showTitleCard && titleGroup != null)
        {
            float titleFade = 0f;
            while (titleFade < 0.35f)
            {
                titleFade += Time.unscaledDeltaTime;
                titleGroup.alpha = Mathf.SmoothStep(0f, 1f, titleFade / 0.35f);
                yield return null;
            }
            titleGroup.alpha = 1f;
        }

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(holdDuration);

        // Activate next scene & wait 1 frame for lights/tilemaps to settle
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;

        // Fade out title card before opening curtains
        if (showTitleCard && titleGroup != null && titleGroup.alpha > 0f)
        {
            float titleFade = 0f;
            while (titleFade < 0.3f)
            {
                titleFade += Time.unscaledDeltaTime;
                titleGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, titleFade / 0.3f);
                yield return null;
            }
            titleGroup.alpha = 0f;
        }

        // 3. OPEN TRANSITION + FADE IN AUDIO
        elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            ApplyTransitionProgress(1f - t, false);
            if (fadeAudioDuringTransition) AudioListener.volume = Mathf.Lerp(0f, originalVolume, t);
            yield return null;
        }

        ResetVisualState();
        if (fadeAudioDuringTransition) AudioListener.volume = originalVolume;
        overlayGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private void PrepareVisualState()
    {
        ResetVisualState();

        if (style == TransitionStyle.CircleIrisWipe)
        {
            wipeImage.sprite = CreateIrisSprite();
            wipeImage.type = Image.Type.Simple;
            overlayGroup.alpha = 1f;
        }
        else if (style == TransitionStyle.DiagonalCurtainWipe)
        {
            wipeImage.sprite = null;
            overlayGroup.alpha = 1f;
            wipePanel.localRotation = Quaternion.Euler(0f, 0f, -12f);
            wipePanel.localScale = new Vector3(1.4f, 1.4f, 1f);
        }
        else
        {
            wipeImage.sprite = null;
        }
    }

    private void ApplyTransitionProgress(float closedAmount, bool isClosing)
    {
        switch (style)
        {
            case TransitionStyle.CinematicLetterboxTitle:
                float barHeight = Mathf.Lerp(0f, 560f, closedAmount);
                topBar.sizeDelta = new Vector2(0f, barHeight);
                bottomBar.sizeDelta = new Vector2(0f, barHeight);
                overlayGroup.alpha = closedAmount > 0.98f ? 1f : 0f;
                break;

            case TransitionStyle.CircleIrisWipe:
                // Scales huge ring down to 1x so the black border closes to a pinhole
                float scale = Mathf.Lerp(28f, 1f, closedAmount);
                wipePanel.localScale = new Vector3(scale, scale, 1f);
                break;

            case TransitionStyle.DiagonalCurtainWipe:
                float startX = isClosing ? -2400f : 2400f;
                float x = Mathf.Lerp(startX, 0f, closedAmount);
                wipePanel.anchoredPosition = new Vector2(x, 0f);
                break;

            case TransitionStyle.SmoothFadeWithTitle:
                overlayGroup.alpha = closedAmount;
                break;
        }
    }

    private void ResetVisualState()
    {
        overlayGroup.alpha = 0f;
        wipePanel.anchoredPosition = Vector2.zero;
        wipePanel.localRotation = Quaternion.identity;
        wipePanel.localScale = Vector3.one;
        topBar.sizeDelta = Vector2.zero;
        bottomBar.sizeDelta = Vector2.zero;
        if (titleGroup != null) titleGroup.alpha = 0f;
    }

    // Generates a crisp circular iris texture with a soft feathered edge in memory
    private Sprite CreateIrisSprite()
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float holeRadius = size * 0.03f;
        float feather = size * 0.015f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01((dist - holeRadius) / feather);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}