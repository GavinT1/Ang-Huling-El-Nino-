using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuLoadingScreen : MonoBehaviour
{
    public static MainMenuLoadingScreen Instance { get; private set; }

    [Header("1. Font & Real Game Art (Drag your assets here!)")]
    public TMP_FontAsset customFont;
    [Tooltip("Drag your Main Menu wasteland background or Prologue key art here!")]
    public Sprite backdropArtwork;
    [Tooltip("Optional: Drop 1 sprite or multiple animation frames (e.g. Player walking, Water Jar, Logo)")]
    public List<Sprite> animatedFeatureSprites = new List<Sprite>();
    public float spriteAnimationFps = 8f;

    [Header("2. Scene & Cinematic Flow")]
    public string cutsceneSceneName = "Cutscene_Opening";
    public float minimumLoadTime = 3.4f;
    [Tooltip("If true, waits for player input at 100% so they can finish reading lore")]
    public bool waitForInputWhenReady = false;

    [Header("3. Chapter Dossier & Lore")]
    public string actLabel = "ACT I  •  PROLOGUE";
    public string chapterTitle = "BEFORE THE LAST DROP";
    public string locationTag = "BARANGAY SAN ROQUE  —  DAY 1,462 WITHOUT RAIN";

    [TextArea(2, 4)]
    public List<string> loadingLoreEntries = new List<string>
    {
        "THE SCORCHING HEAT: Prolonged exposure to the sun drains your hydration. Seek shade beneath ruined arches and broken roofs.",
        "SCAVENGING: Clay water jars and rusted canteens hidden across the valley hold the last clean drops left in the province.",
        "NOTICE BOARDS: Townspeople left warnings and directions nailed to wooden posts before abandoning their homes.",
        "SANCTUARIES: Rest beside glowing hearth-stones to record your journey before crossing into deeper territory."
    };
    public float autoCycleInterval = 5.0f;
    public float typewriterCharsPerSec = 58f;

    [Header("4. Palette (Matches Ang Huling El Nino)")]
    public Color bgDarkTint = new Color(0.14f, 0.06f, 0.03f, 1f);
    public Color frameOuterIron = new Color(0.14f, 0.11f, 0.10f, 0.98f);
    public Color frameInnerRust = new Color(0.52f, 0.24f, 0.10f, 1f);
    public Color emberOrange = new Color(1f, 0.45f, 0.08f, 1f);
    public Color sunGold = new Color(1f, 0.86f, 0.42f, 1f);
    public Color parchmentText = new Color(0.96f, 0.92f, 0.85f, 1f);

    private Canvas loadingCanvas;
    private CanvasGroup rootGroup;

    // Shutter Exit Panels
    private RectTransform topShutterRect;
    private RectTransform bottomShutterRect;

    // Backdrop & Volumetric Lighting
    private RectTransform backdropArtRect;
    private Image backdropArtImg;
    private Image sunBeamImg1;
    private Image sunBeamImg2;
    private Image vignetteImg;

    // Header Choreography
    private CanvasGroup contentGroup;
    private RectTransform headerRect;
    private TMP_Text chapterTitleTmp;
    private RectTransform dividerRect;
    private RectTransform dividerSparkRect;

    // Journal Card & Animated Feature Box
    private RectTransform journalCardRect;
    private RectTransform featureBoxRect;
    private Image featureSpriteImg;
    private RectTransform featureSpriteRect;
    private RectTransform featureGlintRect;
    private TMP_Text loreBodyText;
    private TMP_Text pageIndicatorText;

    // Segmented Progress Gauge
    private RectTransform barFrameRect;
    private RectTransform progressFillRect;
    private RectTransform barShimmerRect;
    private RectTransform progressGlowRect;
    private Image barFlashOverlay;
    private TMP_Text statusText;
    private TMP_Text percentText;
    private TMP_Text continuePromptText;

    // Multi-Depth Particles & Sparks
    private class EmberParticle
    {
        public RectTransform rect;
        public Image img;
        public Vector2 velocity;
        public float phase;
        public float baseAlpha;
        public bool isBurstSpark;
        public float life;
        public float maxLife;
    }

    private readonly List<EmberParticle> ambientEmbers = new List<EmberParticle>();
    private readonly List<EmberParticle> burstSparks = new List<EmberParticle>();
    private readonly List<RectTransform> dustStreaks = new List<RectTransform>();
    private readonly List<float> dustSpeeds = new List<float>();

    private Sprite softCircleSprite;
    private Sprite streakSprite;

    private bool isLoading = false;
    private int currentLoreIndex = 0;
    private float loreTimer = 0f;
    private float spriteFrameTimer = 0f;
    private int spriteFrameIndex = 0;
    private int lastMilestone = 0;
    private float barFlashAlpha = 0f;

    private Coroutine typewriterRoutine;
    private bool isTyping = false;

    private const float BAR_WIDTH = 1660f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    public void PlayGameWithLoadingScreen()
    {
        LoadSceneWithScreen(cutsceneSceneName);
    }

    public void LoadSceneWithScreen(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("MainMenuLoadingCanvas");
        canvasObj.transform.SetParent(transform, false);

        loadingCanvas = canvasObj.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Root Container
        GameObject rootObj = new GameObject("LoadingRoot", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        rootObj.transform.SetParent(canvasObj.transform, false);
        StretchFull(rootObj.GetComponent<RectTransform>());
        rootObj.GetComponent<Image>().color = new Color(0.035f, 0.018f, 0.012f, 1f);

        rootGroup = rootObj.GetComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = false;
        rootObj.SetActive(false);

        softCircleSprite = CreateSoftCircleSprite(64);
        streakSprite = CreateStreakSprite(64, 8);
        Sprite vignetteSprite = CreateVignetteSprite(256);
        Sprite beamSprite = CreateLightBeamSprite(128, 256);

        // 1. DEEP BACKDROP ARTWORK (Parallax + Ken Burns)
        GameObject artObj = new GameObject("BackdropGameArt", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(rootObj.transform, false);
        backdropArtRect = artObj.GetComponent<RectTransform>();
        StretchFull(backdropArtRect);
        backdropArtRect.localScale = Vector3.one * 1.15f;

        backdropArtImg = artObj.GetComponent<Image>();
        backdropArtImg.preserveAspect = false;
        backdropArtImg.raycastTarget = false;
        if (backdropArtwork != null)
        {
            backdropArtImg.sprite = backdropArtwork;
            backdropArtImg.color = new Color(0.75f, 0.48f, 0.34f, 0.48f);
        }
        else
        {
            backdropArtImg.color = bgDarkTint;
        }

        // 2. VOLUMETRIC GOD-RAYS
        sunBeamImg1 = CreateSunBeam(rootObj.transform, beamSprite, new Vector2(-460f, 190f), -25f, new Vector2(560f, 1500f), 0.13f);
        sunBeamImg2 = CreateSunBeam(rootObj.transform, beamSprite, new Vector2(310f, 230f), -18f, new Vector2(720f, 1550f), 0.09f);

        // 3. BACKGROUND EMBER LAYER (Small, sharp, drifting)
        for (int i = 0; i < 35; i++)
            SpawnAmbientEmber(rootObj.transform, isForeground: false);

        // 4. CINEMATIC VIGNETTE
        GameObject vigObj = new GameObject("CinematicVignette", typeof(RectTransform), typeof(Image));
        vigObj.transform.SetParent(rootObj.transform, false);
        StretchFull(vigObj.GetComponent<RectTransform>());
        vignetteImg = vigObj.GetComponent<Image>();
        vignetteImg.sprite = vignetteSprite;
        vignetteImg.color = new Color(0.015f, 0.008f, 0.006f, 0.92f);
        vignetteImg.raycastTarget = false;

        // 5. FOREGROUND UI LAYER
        GameObject contentObj = new GameObject("ForegroundUI", typeof(RectTransform), typeof(CanvasGroup));
        contentObj.transform.SetParent(rootObj.transform, false);
        StretchFull(contentObj.GetComponent<RectTransform>());
        contentGroup = contentObj.GetComponent<CanvasGroup>();

        // 6. TOP-LEFT CHAPTER DOSSIER HEADER
        GameObject headerObj = new GameObject("ChapterHeader", typeof(RectTransform));
        headerObj.transform.SetParent(contentObj.transform, false);
        headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = headerRect.anchorMax = new Vector2(0f, 1f);
        headerRect.pivot = new Vector2(0f, 1f);
        headerRect.anchoredPosition = new Vector2(115f, -96f);
        headerRect.sizeDelta = new Vector2(1200f, 180f);

        TMP_Text actTxt = CreateText(headerObj.transform, "ActLabel", actLabel, 20f, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(800f, 32f),
            TextAlignmentOptions.Left, emberOrange);
        actTxt.characterSpacing = 9f;

        chapterTitleTmp = CreateText(headerObj.transform, "ChapterTitle", chapterTitle, 58f, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -34f), new Vector2(1200f, 70f),
            TextAlignmentOptions.Left, Color.white);
        chapterTitleTmp.characterSpacing = 12f;

        GameObject divObj = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
        divObj.transform.SetParent(headerObj.transform, false);
        dividerRect = divObj.GetComponent<RectTransform>();
        dividerRect.anchorMin = dividerRect.anchorMax = new Vector2(0f, 1f);
        dividerRect.pivot = new Vector2(0f, 1f);
        dividerRect.anchoredPosition = new Vector2(0f, -112f);
        dividerRect.sizeDelta = new Vector2(580f, 3f);
        divObj.GetComponent<Image>().color = emberOrange;

        // Glowing tip on divider line
        GameObject divSpark = new GameObject("DividerSpark", typeof(RectTransform), typeof(Image));
        divSpark.transform.SetParent(dividerRect, false);
        dividerSparkRect = divSpark.GetComponent<RectTransform>();
        dividerSparkRect.anchorMin = dividerSparkRect.anchorMax = new Vector2(1f, 0.5f);
        dividerSparkRect.sizeDelta = new Vector2(28f, 28f);
        Image dsImg = divSpark.GetComponent<Image>();
        dsImg.sprite = softCircleSprite;
        dsImg.color = sunGold;

        TMP_Text locTxt = CreateText(headerObj.transform, "LocationTag", locationTag, 19f, FontStyles.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -126f), new Vector2(1050f, 34f),
            TextAlignmentOptions.Left, sunGold);
        locTxt.characterSpacing = 5f;

        // 7. FIELD JOURNAL CARD (With Corner Rivets)
        bool hasFeatureArt = animatedFeatureSprites != null && animatedFeatureSprites.Count > 0 && animatedFeatureSprites[0] != null;
        float cardWidth = hasFeatureArt ? 1240f : 1400f;
        float cardX = hasFeatureArt ? -185f : 0f;

        GameObject cardOuter = CreateFramedPanel(contentObj.transform, "FieldJournalCard",
            new Vector2(0.5f, 0.43f), new Vector2(cardX, 0f), new Vector2(cardWidth, 240f), out journalCardRect);

        TMP_Text journalHeader = CreateText(cardOuter.transform, "JournalHeader", "FIELD JOURNAL  •  SURVIVAL ARCHIVE", 18f, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(38f, -24f), new Vector2(700f, 30f),
            TextAlignmentOptions.Left, emberOrange);
        journalHeader.characterSpacing = 5f;

        pageIndicatorText = CreateText(cardOuter.transform, "PageIndicator", "1 / 4  [CLICK OR SPACE]", 17f, FontStyles.Bold,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-38f, -24f), new Vector2(440f, 30f),
            TextAlignmentOptions.Right, new Color(1f, 1f, 1f, 0.55f));
        pageIndicatorText.characterSpacing = 3f;

        GameObject cardRule = new GameObject("CardRule", typeof(RectTransform), typeof(Image));
        cardRule.transform.SetParent(cardOuter.transform, false);
        RectTransform crRect = cardRule.GetComponent<RectTransform>();
        crRect.anchorMin = new Vector2(0f, 1f);
        crRect.anchorMax = new Vector2(1f, 1f);
        crRect.pivot = new Vector2(0.5f, 1f);
        crRect.anchoredPosition = new Vector2(0f, -62f);
        crRect.sizeDelta = new Vector2(-76f, 2f);
        cardRule.GetComponent<Image>().color = new Color(1f, 0.72f, 0.42f, 0.20f);

        GameObject loreHolder = new GameObject("LoreTextHolder", typeof(RectTransform));
        loreHolder.transform.SetParent(cardOuter.transform, false);
        RectTransform lhRect = loreHolder.GetComponent<RectTransform>();
        lhRect.anchorMin = Vector2.zero;
        lhRect.anchorMax = Vector2.one;
        lhRect.offsetMin = new Vector2(38f, 24f);
        lhRect.offsetMax = new Vector2(-38f, -74f);

        loreBodyText = CreateText(loreHolder.transform, "LoreText", "", 27f, FontStyles.Normal,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            TextAlignmentOptions.MidlineLeft, parchmentText);
        loreBodyText.lineSpacing = 8f;

        // 8. OPTIONAL ANIMATED PIXEL-ART SHOWCASE BOX (With Light Glint Sweep)
        if (hasFeatureArt)
        {
            GameObject featInner = CreateFramedPanel(contentObj.transform, "FeatureSpriteFrame",
                new Vector2(0.5f, 0.43f), new Vector2(615f, 0f), new Vector2(280f, 240f), out featureBoxRect);

            GameObject maskArea = new GameObject("SpriteMaskArea", typeof(RectTransform), typeof(RectMask2D));
            maskArea.transform.SetParent(featInner.transform, false);
            StretchFull(maskArea.GetComponent<RectTransform>());

            GameObject sprObj = new GameObject("AnimatedGameSprite", typeof(RectTransform), typeof(Image));
            sprObj.transform.SetParent(maskArea.transform, false);
            featureSpriteRect = sprObj.GetComponent<RectTransform>();
            featureSpriteRect.sizeDelta = new Vector2(184f, 184f);
            featureSpriteImg = sprObj.GetComponent<Image>();
            featureSpriteImg.sprite = animatedFeatureSprites[0];
            featureSpriteImg.preserveAspect = true;
            featureSpriteImg.raycastTarget = false;

            // Diagonal sheen sweeping across showcase box
            GameObject glintObj = new GameObject("ShowcaseGlint", typeof(RectTransform), typeof(Image));
            glintObj.transform.SetParent(maskArea.transform, false);
            featureGlintRect = glintObj.GetComponent<RectTransform>();
            featureGlintRect.sizeDelta = new Vector2(65f, 340f);
            featureGlintRect.localRotation = Quaternion.Euler(0f, 0f, -25f);
            Image gImg = glintObj.GetComponent<Image>();
            gImg.sprite = streakSprite;
            gImg.color = new Color(1f, 0.92f, 0.72f, 0.14f);
            gImg.raycastTarget = false;
        }

        // 9. SEGMENTED MOLTEN PROGRESS GAUGE
        GameObject barInner = CreateFramedPanel(contentObj.transform, "ProgressBarFrame",
            new Vector2(0.5f, 0f), new Vector2(0f, 96f), new Vector2(BAR_WIDTH + 16f, 36f), out barFrameRect);

        GameObject barTrack = new GameObject("BarTrack", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        barTrack.transform.SetParent(barInner.transform, false);
        RectTransform trackRect = barTrack.GetComponent<RectTransform>();
        StretchFull(trackRect);
        trackRect.offsetMin = new Vector2(4f, 4f);
        trackRect.offsetMax = new Vector2(-4f, -4f);
        barTrack.GetComponent<Image>().color = new Color(0.05f, 0.025f, 0.015f, 1f);

        // Ember Orange Fill
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(barTrack.transform, false);
        progressFillRect = fillObj.GetComponent<RectTransform>();
        progressFillRect.anchorMin = new Vector2(0f, 0f);
        progressFillRect.anchorMax = new Vector2(0f, 1f);
        progressFillRect.pivot = new Vector2(0f, 0.5f);
        progressFillRect.anchoredPosition = Vector2.zero;
        progressFillRect.sizeDelta = new Vector2(0f, 0f);
        fillObj.GetComponent<Image>().color = emberOrange;

        // Top Gold Highlight Strip
        GameObject topHighlight = new GameObject("FillTopGold", typeof(RectTransform), typeof(Image));
        topHighlight.transform.SetParent(fillObj.transform, false);
        RectTransform thRect = topHighlight.GetComponent<RectTransform>();
        thRect.anchorMin = new Vector2(0f, 1f);
        thRect.anchorMax = new Vector2(1f, 1f);
        thRect.pivot = new Vector2(0.5f, 1f);
        thRect.anchoredPosition = Vector2.zero;
        thRect.sizeDelta = new Vector2(0f, 4f);
        topHighlight.GetComponent<Image>().color = sunGold;

        // Animated Shimmer Sweep inside Bar
        GameObject shimObj = new GameObject("BarShimmer", typeof(RectTransform), typeof(Image));
        shimObj.transform.SetParent(fillObj.transform, false);
        barShimmerRect = shimObj.GetComponent<RectTransform>();
        barShimmerRect.anchorMin = new Vector2(0f, 0f);
        barShimmerRect.anchorMax = new Vector2(0f, 1f);
        barShimmerRect.sizeDelta = new Vector2(200f, 0f);
        Image shimImg = shimObj.GetComponent<Image>();
        shimImg.sprite = streakSprite;
        shimImg.color = new Color(1f, 0.96f, 0.82f, 0.55f);

        // Segment Notch Dividers
        int segments = 20;
        for (int s = 1; s < segments; s++)
        {
            GameObject notch = new GameObject("Notch", typeof(RectTransform), typeof(Image));
            notch.transform.SetParent(barTrack.transform, false);
            RectTransform nRect = notch.GetComponent<RectTransform>();
            float nx = (BAR_WIDTH / segments) * s;
            nRect.anchorMin = new Vector2(0f, 0f);
            nRect.anchorMax = new Vector2(0f, 1f);
            nRect.anchoredPosition = new Vector2(nx, 0f);
            nRect.sizeDelta = new Vector2(3f, 0f);
            notch.GetComponent<Image>().color = new Color(0.04f, 0.02f, 0.01f, 0.8f);
        }

        // Milestone Shockwave Flash Overlay
        GameObject flashObj = new GameObject("BarMilestoneFlash", typeof(RectTransform), typeof(Image));
        flashObj.transform.SetParent(barTrack.transform, false);
        StretchFull(flashObj.GetComponent<RectTransform>());
        barFlashOverlay = flashObj.GetComponent<Image>();
        barFlashOverlay.color = new Color(1f, 0.9f, 0.6f, 0f);
        barFlashOverlay.raycastTarget = false;

        // Leading Glow Spark
        GameObject glowDot = new GameObject("LeadingSpark", typeof(RectTransform), typeof(Image));
        glowDot.transform.SetParent(barTrack.transform, false);
        progressGlowRect = glowDot.GetComponent<RectTransform>();
        progressGlowRect.anchorMin = progressGlowRect.anchorMax = new Vector2(0f, 0.5f);
        progressGlowRect.sizeDelta = new Vector2(54f, 54f);
        Image gdImg = glowDot.GetComponent<Image>();
        gdImg.sprite = softCircleSprite;
        gdImg.color = sunGold;

        // Status & Percentage Labels
        statusText = CreateText(contentObj.transform, "StatusText", "PREPARING PROLOGUE...", 22f, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-BAR_WIDTH * 0.5f, 142f), new Vector2(900f, 36f),
            TextAlignmentOptions.BottomLeft, Color.white);
        statusText.characterSpacing = 6f;

        percentText = CreateText(contentObj.transform, "PercentText", "0%", 28f, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(BAR_WIDTH * 0.5f, 140f), new Vector2(300f, 40f),
            TextAlignmentOptions.BottomRight, sunGold);

        continuePromptText = CreateText(contentObj.transform, "ContinuePrompt", "PRESS ANY KEY OR CLICK TO BEGIN", 22f, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 142f), new Vector2(800f, 36f),
            TextAlignmentOptions.Bottom, sunGold);
        continuePromptText.characterSpacing = 6f;
        continuePromptText.gameObject.SetActive(false);

        // 10. FOREGROUND WIND STREAKS & LARGE DEPTH-BLUR EMBERS
        for (int i = 0; i < 14; i++)
        {
            GameObject dObj = new GameObject("ForegroundWind", typeof(RectTransform), typeof(Image));
            dObj.transform.SetParent(rootObj.transform, false);
            RectTransform dRect = dObj.GetComponent<RectTransform>();
            dRect.sizeDelta = new Vector2(Random.Range(260f, 560f), Random.Range(2f, 5f));
            dRect.localRotation = Quaternion.Euler(0f, 0f, 3.5f);
            dRect.anchoredPosition = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-480f, 480f));
            Image dImg = dObj.GetComponent<Image>();
            dImg.sprite = streakSprite;
            dImg.color = new Color(1f, 0.78f, 0.52f, Random.Range(0.06f, 0.14f));
            dImg.raycastTarget = false;
            dustStreaks.Add(dRect);
            dustSpeeds.Add(Random.Range(420f, 760f));
        }

        for (int i = 0; i < 18; i++)
            SpawnAmbientEmber(rootObj.transform, isForeground: true);

        // 11. TOP & BOTTOM CINEMA SHUTTER BARS (Used for dramatic exit wipe!)
        topShutterRect = CreateShutterBar(rootObj.transform, true, 56f);
        bottomShutterRect = CreateShutterBar(rootObj.transform, false, 56f);
    }

    private void Update()
    {
        if (!isLoading) return;

        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;

        // Interactive Lore Control: If currently typing, snap full text; otherwise flip to next entry
        if (loadingLoreEntries.Count > 0 && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                loreBodyText.maxVisibleCharacters = 99999;
                isTyping = false;
            }
            else if (loadingLoreEntries.Count > 1)
            {
                NextLoreEntry();
            }
        }

        // 1. Deep Parallax + Subtle Mouse Tilt on Backdrop Artwork
        if (backdropArtRect != null)
        {
            float scale = 1.15f + Mathf.Sin(t * 0.22f) * 0.03f;
            backdropArtRect.localScale = new Vector3(scale, scale, 1f);

            Vector2 mouseShift = Vector2.zero;
            if (Screen.width > 0 && Screen.height > 0)
            {
                float nx = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
                float ny = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;
                mouseShift = new Vector2(-nx * 22f, -ny * 12f);
            }

            Vector2 drift = new Vector2(Mathf.Sin(t * 0.18f) * 26f, Mathf.Cos(t * 0.15f) * 14f);
            backdropArtRect.anchoredPosition = Vector2.Lerp(backdropArtRect.anchoredPosition, drift + mouseShift, dt * 3f);
        }

        // 2. Volumetric Sunbeam Pulse
        if (sunBeamImg1 != null)
        {
            Color c = sunBeamImg1.color;
            c.a = 0.09f + Mathf.Sin(t * 1.1f) * 0.035f;
            sunBeamImg1.color = c;
        }
        if (sunBeamImg2 != null)
        {
            Color c = sunBeamImg2.color;
            c.a = 0.07f + Mathf.Cos(t * 0.9f) * 0.03f;
            sunBeamImg2.color = c;
        }

        // 3. Feature Pixel Sprite Animation + Glint Sweep
        if (featureSpriteImg != null && animatedFeatureSprites.Count > 0)
        {
            if (animatedFeatureSprites.Count > 1)
            {
                spriteFrameTimer += dt;
                if (spriteFrameTimer >= 1f / Mathf.Max(1f, spriteAnimationFps))
                {
                    spriteFrameTimer = 0f;
                    spriteFrameIndex = (spriteFrameIndex + 1) % animatedFeatureSprites.Count;
                    if (animatedFeatureSprites[spriteFrameIndex] != null)
                        featureSpriteImg.sprite = animatedFeatureSprites[spriteFrameIndex];
                }
            }
            else if (featureSpriteRect != null)
            {
                featureSpriteRect.anchoredPosition = new Vector2(0f, Mathf.Sin(t * 2.4f) * 6f);
            }
        }

        if (featureGlintRect != null)
        {
            float gx = Mathf.Repeat(t * 190f, 520f) - 260f;
            featureGlintRect.anchoredPosition = new Vector2(gx, 0f);
        }

        // 4. Progress Bar Shimmer & Milestone Flash Decay
        if (barShimmerRect != null)
        {
            float sweepX = Mathf.Repeat(t * 720f, BAR_WIDTH + 400f) - 200f;
            barShimmerRect.anchoredPosition = new Vector2(sweepX, 0f);
        }

        if (barFlashOverlay != null && barFlashAlpha > 0f)
        {
            barFlashAlpha = Mathf.MoveTowards(barFlashAlpha, 0f, dt * 1.8f);
            Color fc = barFlashOverlay.color;
            fc.a = barFlashAlpha;
            barFlashOverlay.color = fc;
        }

        // 5. Wind Streaks, Ambient Embers & Burst Sparks
        for (int i = 0; i < dustStreaks.Count; i++)
        {
            RectTransform d = dustStreaks[i];
            if (d == null) continue;
            Vector2 pos = d.anchoredPosition;
            pos.x += dustSpeeds[i] * dt;
            if (pos.x > 1250f) pos.x = -1250f;
            d.anchoredPosition = pos;
        }

        for (int i = 0; i < ambientEmbers.Count; i++)
        {
            EmberParticle p = ambientEmbers[i];
            Vector2 pos = p.rect.anchoredPosition + p.velocity * dt;
            pos.x += Mathf.Sin(t * 2.4f + p.phase) * 24f * dt;
            if (pos.x > 1020f) pos.x = -1020f;
            if (pos.y > 580f) pos.y = -580f;
            p.rect.anchoredPosition = pos;

            Color c = p.img.color;
            c.a = p.baseAlpha * (0.65f + 0.35f * Mathf.Sin(t * 5f + p.phase));
            p.img.color = c;
        }

        for (int i = burstSparks.Count - 1; i >= 0; i--)
        {
            EmberParticle sp = burstSparks[i];
            sp.life += dt;
            if (sp.life >= sp.maxLife)
            {
                Destroy(sp.rect.gameObject);
                burstSparks.RemoveAt(i);
                continue;
            }
            sp.velocity.y -= 260f * dt; // Gravity arc
            sp.rect.anchoredPosition += sp.velocity * dt;
            Color sc = sp.img.color;
            sc.a = 1f - (sp.life / sp.maxLife);
            sp.img.color = sc;
        }

        if (continuePromptText != null && continuePromptText.gameObject.activeSelf)
        {
            Color cc = continuePromptText.color;
            cc.a = 0.55f + 0.45f * Mathf.Sin(t * 4.5f);
            continuePromptText.color = cc;
        }
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;
        Time.timeScale = 1f;
        float startVol = AudioListener.volume;

        rootGroup.gameObject.SetActive(true);
        rootGroup.blocksRaycasts = true;
        rootGroup.alpha = 0f;
        continuePromptText.gameObject.SetActive(false);

        lastMilestone = 0;
        barFlashAlpha = 0f;
        SetProgress(0f);

        // Reset Shutter & Entrance Positions
        topShutterRect.sizeDelta = new Vector2(0f, 56f);
        bottomShutterRect.sizeDelta = new Vector2(0f, 56f);
        contentGroup.alpha = 0f;
        headerRect.anchoredPosition = new Vector2(75f, -96f);
        dividerRect.localScale = new Vector3(0f, 1f, 1f);
        chapterTitleTmp.characterSpacing = 26f;

        currentLoreIndex = 0;
        loreTimer = 0f;
        StartLoreEntry(0);

        // 1. Fade in Loading Screen & fade out Menu Audio
        float fadeIn = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / fadeIn);
            rootGroup.alpha = k;
            AudioListener.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }
        rootGroup.alpha = 1f;
        AudioListener.volume = 0f;

        // 2. Staggered Title Tracking & Card Entrance
        StartCoroutine(RevealContentRoutine());

        // 3. Async Scene Load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float loadTimer = 0f;
        float displayed = 0f;

        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            loadTimer += dt;
            loreTimer += dt;

            if (!isTyping && loadingLoreEntries.Count > 1 && loreTimer >= autoCycleInterval)
            {
                NextLoreEntry();
            }

            float realProg = Mathf.Clamp01(op.progress / 0.9f);
            float timeProg = Mathf.Clamp01(loadTimer / minimumLoadTime);
            float target = Mathf.Min(realProg, timeProg);

            displayed = Mathf.MoveTowards(displayed, target, dt * 1.1f);
            SetProgress(displayed);

            // Spawn subtle continuous sparks off leading edge
            if (displayed > 0.02f && displayed < 0.99f && Random.value < dt * 18f)
            {
                SpawnGaugeSparks(displayed, 1);
            }

            // Milestone bursts at 25%, 50%, 75%
            int milestone = Mathf.FloorToInt(displayed * 4f);
            if (milestone > lastMilestone && milestone <= 3)
            {
                lastMilestone = milestone;
                barFlashAlpha = 0.55f;
                SpawnGaugeSparks(displayed, 10);
            }

            if (displayed > 0.6f)
                statusText.text = "LOADING ENVIRONMENT & CUTSCENE...";

            if (op.progress >= 0.9f && loadTimer >= minimumLoadTime && displayed >= 0.99f)
            {
                SetProgress(1f);
                barFlashAlpha = 0.85f;
                SpawnGaugeSparks(1f, 16);
                statusText.text = "READY";
                break;
            }

            yield return null;
        }

        if (waitForInputWhenReady)
        {
            continuePromptText.gameObject.SetActive(true);
            yield return null;
            while (!Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            {
                yield return null;
            }
            continuePromptText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.4f);
        }

        // 4. Activate Cutscene Scene
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
        yield return null;

        // 5. Cinematic Shutter Wipe & Fade Out into Opening Cutscene
        float fadeOut = 0.7f;
        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / fadeOut);
            contentGroup.alpha = 1f - Mathf.Clamp01(k * 1.6f);
            rootGroup.alpha = 1f - k;
            AudioListener.volume = Mathf.Lerp(0f, startVol, k);
            yield return null;
        }

        AudioListener.volume = startVol;
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = false;
        rootGroup.gameObject.SetActive(false);
        isLoading = false;
    }

    private IEnumerator RevealContentRoutine()
    {
        float dur = 0.65f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / dur), 3f);
            contentGroup.alpha = ease;
            headerRect.anchoredPosition = Vector2.Lerp(new Vector2(75f, -96f), new Vector2(115f, -96f), ease);
            dividerRect.localScale = new Vector3(ease, 1f, 1f);
            chapterTitleTmp.characterSpacing = Mathf.Lerp(26f, 12f, ease);
            yield return null;
        }
        contentGroup.alpha = 1f;
        chapterTitleTmp.characterSpacing = 12f;
    }

    private void NextLoreEntry()
    {
        int next = (currentLoreIndex + 1) % loadingLoreEntries.Count;
        StartLoreEntry(next);
    }

    private void StartLoreEntry(int index)
    {
        if (loadingLoreEntries.Count == 0) return;
        currentLoreIndex = index;
        loreTimer = 0f;
        pageIndicatorText.text = $"{currentLoreIndex + 1} / {loadingLoreEntries.Count}   [CLICK OR SPACE]";

        if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
        typewriterRoutine = StartCoroutine(TypewriterRoutine(loadingLoreEntries[currentLoreIndex]));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        loreBodyText.text = fullText;
        loreBodyText.ForceMeshUpdate();

        int totalChars = loreBodyText.textInfo.characterCount;
        loreBodyText.maxVisibleCharacters = 0;

        float visible = 0f;
        while (visible < totalChars)
        {
            visible += Time.unscaledDeltaTime * typewriterCharsPerSec;
            loreBodyText.maxVisibleCharacters = Mathf.FloorToInt(visible);
            yield return null;
        }

        loreBodyText.maxVisibleCharacters = totalChars;
        isTyping = false;
    }

    private void SetProgress(float val)
    {
        val = Mathf.Clamp01(val);
        float w = BAR_WIDTH * val;
        if (progressFillRect != null) progressFillRect.sizeDelta = new Vector2(w, 0f);
        if (progressGlowRect != null) progressGlowRect.anchoredPosition = new Vector2(w, 0f);
        if (percentText != null) percentText.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    // --- PARTICLE & UI BUILDERS ---

    private void SpawnAmbientEmber(Transform parent, bool isForeground)
    {
        GameObject eObj = new GameObject(isForeground ? "FgEmber" : "BgEmber", typeof(RectTransform), typeof(Image));
        eObj.transform.SetParent(parent, false);
        RectTransform rect = eObj.GetComponent<RectTransform>();
        float sz = isForeground ? Random.Range(10f, 20f) : Random.Range(3f, 7f);
        rect.sizeDelta = new Vector2(sz, sz);
        rect.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));

        Image img = eObj.GetComponent<Image>();
        img.sprite = softCircleSprite;
        float alpha = isForeground ? Random.Range(0.22f, 0.45f) : Random.Range(0.45f, 0.85f);
        img.color = Random.value > 0.25f
            ? new Color(1f, Random.Range(0.48f, 0.78f), 0.14f, alpha)
            : new Color(1f, 0.94f, 0.68f, alpha);
        img.raycastTarget = false;

        float speedMult = isForeground ? 1.5f : 0.85f;
        ambientEmbers.Add(new EmberParticle
        {
            rect = rect,
            img = img,
            velocity = new Vector2(Random.Range(45f, 110f) * speedMult, Random.Range(40f, 105f) * speedMult),
            phase = Random.Range(0f, 100f),
            baseAlpha = alpha
        });
    }

    private void SpawnGaugeSparks(float normalizedX, int count)
    {
        if (barFrameRect == null) return;
        float spawnX = -BAR_WIDTH * 0.5f + (BAR_WIDTH * normalizedX);

        for (int i = 0; i < count; i++)
        {
            GameObject spObj = new GameObject("GaugeSpark", typeof(RectTransform), typeof(Image));
            spObj.transform.SetParent(barFrameRect.parent, false);
            RectTransform r = spObj.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(spawnX, 96f);
            float sz = Random.Range(4f, 9f);
            r.sizeDelta = new Vector2(sz, sz);

            Image img = spObj.GetComponent<Image>();
            img.sprite = softCircleSprite;
            img.color = Random.value > 0.4f ? sunGold : emberOrange;
            img.raycastTarget = false;

            burstSparks.Add(new EmberParticle
            {
                rect = r,
                img = img,
                velocity = new Vector2(Random.Range(-110f, 140f), Random.Range(90f, 220f)),
                life = 0f,
                maxLife = Random.Range(0.45f, 0.85f)
            });
        }
    }

    private GameObject CreateFramedPanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, out RectTransform outerRect)
    {
        GameObject outer = new GameObject(name, typeof(RectTransform), typeof(Image));
        outer.transform.SetParent(parent, false);
        outerRect = outer.GetComponent<RectTransform>();
        outerRect.anchorMin = outerRect.anchorMax = anchor;
        outerRect.anchoredPosition = pos;
        outerRect.sizeDelta = size;
        outer.GetComponent<Image>().color = frameOuterIron;

        GameObject mid = new GameObject("RustTrim", typeof(RectTransform), typeof(Image));
        mid.transform.SetParent(outer.transform, false);
        RectTransform mRect = mid.GetComponent<RectTransform>();
        StretchFull(mRect);
        mRect.offsetMin = new Vector2(4f, 4f);
        mRect.offsetMax = new Vector2(-4f, -4f);
        mid.GetComponent<Image>().color = frameInnerRust;

        GameObject inner = new GameObject("InnerBody", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(mid.transform, false);
        RectTransform iRect = inner.GetComponent<RectTransform>();
        StretchFull(iRect);
        iRect.offsetMin = new Vector2(3f, 3f);
        iRect.offsetMax = new Vector2(-3f, -3f);
        inner.GetComponent<Image>().color = new Color(0.065f, 0.04f, 0.03f, 0.96f);

        // Decorative Corner Rivets
        Vector2[] rivetAnchors = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
        Vector2[] rivetOffsets = { new Vector2(12f, -12f), new Vector2(-12f, -12f), new Vector2(12f, 12f), new Vector2(-12f, 12f) };
        for (int i = 0; i < 4; i++)
        {
            GameObject riv = new GameObject("Rivet", typeof(RectTransform), typeof(Image));
            riv.transform.SetParent(inner.transform, false);
            RectTransform rr = riv.GetComponent<RectTransform>();
            rr.anchorMin = rr.anchorMax = rivetAnchors[i];
            rr.anchoredPosition = rivetOffsets[i];
            rr.sizeDelta = new Vector2(6f, 6f);
            riv.GetComponent<Image>().color = new Color(0.75f, 0.45f, 0.22f, 0.75f);
        }

        return inner;
    }

    private Image CreateSunBeam(Transform parent, Sprite sprite, Vector2 pos, float angle, Vector2 size, float alpha)
    {
        GameObject obj = new GameObject("SunBeam", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        r.localRotation = Quaternion.Euler(0f, 0f, angle);
        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.color = new Color(sunGold.r, sunGold.g, sunGold.b, alpha);
        img.raycastTarget = false;
        return img;
    }

    private RectTransform CreateShutterBar(Transform parent, bool isTop, float height)
    {
        GameObject bar = new GameObject(isTop ? "TopShutter" : "BottomShutter", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform r = bar.GetComponent<RectTransform>();
        r.anchorMin = isTop ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
        r.anchorMax = isTop ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
        r.pivot = new Vector2(0.5f, isTop ? 1f : 0f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(0f, height);
        bar.GetComponent<Image>().color = new Color(0.012f, 0.008f, 0.006f, 0.99f);
        return r;
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private TMP_Text CreateText(Transform parent, string name, string content, float fontSize, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size,
        TextAlignmentOptions align, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TMP_Text tmp = obj.GetComponent<TextMeshProUGUI>();
        if (customFont != null) tmp.font = customFont;
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Sprite CreateSoftCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(1f - d, 1.8f)));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateVignetteSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / c;
                float dy = (y - c) / c;
                float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 0.88f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(d, 1.75f)));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateLightBeamSprite(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            float ny = 1f - (y / (float)(h - 1));
            for (int x = 0; x < w; x++)
            {
                float nx = 1f - Mathf.Abs((x / (float)(w - 1)) * 2f - 1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(nx, 1.8f) * Mathf.Sin(ny * Mathf.PI)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
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