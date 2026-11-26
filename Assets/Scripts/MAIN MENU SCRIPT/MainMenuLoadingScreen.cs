using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuLoadingScreen : MonoBehaviour
{
    public static MainMenuLoadingScreen Instance { get; private set; }

    [Header("1. Font & Real Game Art")]
    public TMP_FontAsset customFont;
    [Tooltip("Drag your Main Menu wasteland background or Prologue key art here")]
    public Sprite backdropArtwork;
    [Tooltip("Optional: Drag your 'ANG HULING EL NINO' title logo sprite")]
    public Sprite gameLogoSprite;

    [Header("2. Animated Character Runner (Optional)")]
    [Tooltip("Drop your Player Walk/Run frames here so they run across the horizon above the loading bar!")]
    public List<Sprite> animatedFeatureSprites = new List<Sprite>();
    public float spriteAnimationFps = 10f;
    [Range(0.5f, 3.0f)]
    public float characterZoom = 1.6f;

    [Header("3. Scene & Cinematic Flow")]
    public string cutsceneSceneName = "Cutscene_Opening";
    public float minimumLoadTime = 3.2f;

    [Header("4. Chapter Title Card")]
    public string actLabel = "ACT I  •  PROLOGUE";
    public string chapterTitle = "BEFORE THE LAST DROP";
    public string locationTag = "BARANGAY SAN ROQUE  —  DAY 1,462 WITHOUT RAIN";

    [Header("5. Palette (Ang Huling El Nino)")]
    public Color bgDarkTint = new Color(0.14f, 0.06f, 0.03f, 1f);
    public Color frameOuterIron = new Color(0.13f, 0.10f, 0.09f, 0.98f);
    public Color frameInnerRust = new Color(0.58f, 0.26f, 0.10f, 1f);
    public Color emberOrange = new Color(1f, 0.44f, 0.07f, 1f);
    public Color sunGold = new Color(1f, 0.86f, 0.42f, 1f);

    private Canvas loadingCanvas;
    private CanvasGroup rootGroup;
    private Image rootBgImg;

    // Cinematic Shutters
    private RectTransform topShutterRect;
    private RectTransform bottomShutterRect;

    // Backdrop & Volumetric Lighting
    private RectTransform backdropArtRect;
    private Image backdropArtImg;
    private Image sunBeam1;
    private Image sunBeam2;
    private Image sunBeam3;
    private Image heatPulseOverlay;
    private Image vignetteImg;

    // Centerpiece Title Choreography
    private CanvasGroup contentGroup;
    private RectTransform centerTitleRect;
    private TMP_Text chapterTitleTmp;
    private RectTransform leftWingLine;
    private RectTransform rightWingLine;

    // Animated Character Runner on Horizon
    private Image runnerImg;
    private RectTransform runnerRect;
    private RectTransform runnerShadowRect;

    // Molten Progress Gauge
    private RectTransform barOuterRect;
    private Vector2 barBaseAnchoredPos;
    private float barRecoilOffset = 0f;
    private RectTransform progressFillRect;
    private RectTransform barShimmerRect;
    private RectTransform progressGlowRect;
    private Image barFlashOverlay;
    private TMP_Text statusText;
    private TMP_Text percentText;

    // Multi-Depth Particles
    private class EmberParticle
    {
        public RectTransform rect;
        public Image img;
        public Vector2 velocity;
        public float phase;
        public float baseAlpha;
        public float life;
        public float maxLife;
    }

    private readonly List<EmberParticle> ambientEmbers = new List<EmberParticle>();
    private readonly List<EmberParticle> burstSparks = new List<EmberParticle>();
    private readonly List<RectTransform> dustStreaks = new List<RectTransform>();
    private readonly List<float> dustSpeeds = new List<float>();
    private readonly List<RectTransform> heatMirageBands = new List<RectTransform>();

    private Sprite softCircleSprite;
    private Sprite streakSprite;

    private bool isLoading = false;
    private float spriteFrameTimer = 0f;
    private int spriteFrameIndex = 0;
    private int lastMilestone = 0;
    private float barFlashAlpha = 0f;

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
        rootBgImg = rootObj.GetComponent<Image>();
        rootBgImg.color = new Color(0.03f, 0.015f, 0.01f, 1f);

        rootGroup = rootObj.GetComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = false;
        rootObj.SetActive(false);

        softCircleSprite = CreateSoftCircleSprite(64);
        streakSprite = CreateStreakSprite(64, 8);
        Sprite vignetteSprite = CreateVignetteSprite(256);
        Sprite beamSprite = CreateLightBeamSprite(128, 256);

        // 1. DEEP BACKDROP ARTWORK
        GameObject artObj = new GameObject("BackdropGameArt", typeof(RectTransform), typeof(Image));
        artObj.transform.SetParent(rootObj.transform, false);
        backdropArtRect = artObj.GetComponent<RectTransform>();
        StretchFull(backdropArtRect);
        backdropArtRect.localScale = Vector3.one * 1.16f;

        backdropArtImg = artObj.GetComponent<Image>();
        backdropArtImg.preserveAspect = false;
        backdropArtImg.raycastTarget = false;
        if (backdropArtwork != null)
        {
            backdropArtImg.sprite = backdropArtwork;
            backdropArtImg.color = new Color(0.84f, 0.56f, 0.40f, 0.58f);
        }
        else
        {
            backdropArtImg.color = bgDarkTint;
        }

        // 2. RISING HEAT-MIRAGE WAVES
        for (int i = 0; i < 5; i++)
        {
            GameObject mObj = new GameObject("HeatMirage", typeof(RectTransform), typeof(Image));
            mObj.transform.SetParent(rootObj.transform, false);
            RectTransform mr = mObj.GetComponent<RectTransform>();
            mr.sizeDelta = new Vector2(Random.Range(1100f, 1750f), Random.Range(14f, 26f));
            mr.anchoredPosition = new Vector2(Random.Range(-150f, 150f), -120f + i * 65f);
            Image mi = mObj.GetComponent<Image>();
            mi.sprite = streakSprite;
            mi.color = new Color(1f, 0.72f, 0.35f, 0.045f);
            mi.raycastTarget = false;
            heatMirageBands.Add(mr);
        }

        // 3. THREE VOLUMETRIC GOD-RAYS
        sunBeam1 = CreateSunBeam(rootObj.transform, beamSprite, new Vector2(-520f, 200f), -26f, new Vector2(540f, 1550f), 0.14f);
        sunBeam2 = CreateSunBeam(rootObj.transform, beamSprite, new Vector2(140f, 240f), -20f, new Vector2(740f, 1600f), 0.10f);
        sunBeam3 = CreateSunBeam(rootObj.transform, beamSprite, new Vector2(680f, 180f), -15f, new Vector2(460f, 1450f), 0.08f);

        // 4. WARM AMBIENT HEAT PULSE
        GameObject heatObj = new GameObject("AmbientHeatPulse", typeof(RectTransform), typeof(Image));
        heatObj.transform.SetParent(rootObj.transform, false);
        heatObj.GetComponent<RectTransform>().sizeDelta = new Vector2(2200f, 1200f);
        heatPulseOverlay = heatObj.GetComponent<Image>();
        heatPulseOverlay.sprite = softCircleSprite;
        heatPulseOverlay.color = new Color(emberOrange.r, emberOrange.g, emberOrange.b, 0.13f);
        heatPulseOverlay.raycastTarget = false;

        // 5. DISTANT EMBERS
        for (int i = 0; i < 42; i++)
            SpawnAmbientEmber(rootObj.transform, isForeground: false);

        // 6. CINEMATIC VIGNETTE
        GameObject vigObj = new GameObject("CinematicVignette", typeof(RectTransform), typeof(Image));
        vigObj.transform.SetParent(rootObj.transform, false);
        StretchFull(vigObj.GetComponent<RectTransform>());
        vignetteImg = vigObj.GetComponent<Image>();
        vignetteImg.sprite = vignetteSprite;
        vignetteImg.color = new Color(0.015f, 0.008f, 0.006f, 0.88f);
        vignetteImg.raycastTarget = false;

        // 7. FOREGROUND UI LAYER
        GameObject contentObj = new GameObject("ForegroundUI", typeof(RectTransform), typeof(CanvasGroup));
        contentObj.transform.SetParent(rootObj.transform, false);
        StretchFull(contentObj.GetComponent<RectTransform>());
        contentGroup = contentObj.GetComponent<CanvasGroup>();

        // Optional Top Game Logo Watermark
        if (gameLogoSprite != null)
        {
            GameObject logoObj = new GameObject("GameLogoCrest", typeof(RectTransform), typeof(Image));
            logoObj.transform.SetParent(contentObj.transform, false);
            RectTransform lr = logoObj.GetComponent<RectTransform>();
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 1f);
            lr.pivot = new Vector2(0.5f, 1f);
            lr.anchoredPosition = new Vector2(0f, -86f);
            lr.sizeDelta = new Vector2(420f, 140f);
            Image lImg = logoObj.GetComponent<Image>();
            lImg.sprite = gameLogoSprite;
            lImg.preserveAspect = true;
            lImg.color = new Color(1f, 1f, 1f, 0.92f);
            lImg.raycastTarget = false;
        }

        // 8. CENTERPIECE CINEMATIC CHAPTER TITLE (No Tip Box!)
        GameObject centerObj = new GameObject("CenterpieceChapterTitle", typeof(RectTransform));
        centerObj.transform.SetParent(contentObj.transform, false);
        centerTitleRect = centerObj.GetComponent<RectTransform>();
        centerTitleRect.anchorMin = centerTitleRect.anchorMax = new Vector2(0.5f, 0.54f);
        centerTitleRect.anchoredPosition = Vector2.zero;
        centerTitleRect.sizeDelta = new Vector2(1500f, 260f);

        // Soft warm glow behind title
        GameObject titleGlow = new GameObject("TitleBackGlow", typeof(RectTransform), typeof(Image));
        titleGlow.transform.SetParent(centerObj.transform, false);
        titleGlow.GetComponent<RectTransform>().sizeDelta = new Vector2(1200f, 260f);
        Image tgImg = titleGlow.GetComponent<Image>();
        tgImg.sprite = softCircleSprite;
        tgImg.color = new Color(0.02f, 0.01f, 0.006f, 0.72f);

        // Act Label with Flanking Wing Lines
        TMP_Text actTxt = CreateText(centerObj.transform, "ActLabel", actLabel, 22f, FontStyles.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(600f, 34f),
            TextAlignmentOptions.Center, emberOrange);
        actTxt.characterSpacing = 12f;

        leftWingLine = CreateLine(centerObj.transform, "LeftWing", new Vector2(-340f, -29f), new Vector2(240f, 2f), emberOrange);
        rightWingLine = CreateLine(centerObj.transform, "RightWing", new Vector2(340f, -29f), new Vector2(240f, 2f), emberOrange);

        // Main Chapter Title
        chapterTitleTmp = CreateText(centerObj.transform, "ChapterTitle", chapterTitle, 74f, FontStyles.Bold,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(1480f, 92f),
            TextAlignmentOptions.Center, Color.white);
        chapterTitleTmp.characterSpacing = 14f;

        // Bottom Ornate Rule + Diamond Crest
        CreateLine(centerObj.transform, "BottomRule", new Vector2(0f, -54f), new Vector2(780f, 2f), new Color(1f, 0.75f, 0.42f, 0.45f));

        GameObject diamond = new GameObject("CenterDiamond", typeof(RectTransform), typeof(Image));
        diamond.transform.SetParent(centerObj.transform, false);
        RectTransform dRect = diamond.GetComponent<RectTransform>();
        dRect.anchoredPosition = new Vector2(0f, -54f);
        dRect.sizeDelta = new Vector2(12f, 12f);
        dRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        diamond.GetComponent<Image>().color = sunGold;

        // Location / Day Subtitle
        TMP_Text locTxt = CreateText(centerObj.transform, "LocationTag", locationTag, 21f, FontStyles.Normal,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(1200f, 36f),
            TextAlignmentOptions.Center, sunGold);
        locTxt.characterSpacing = 7f;

        // 9. ANIMATED CHARACTER RUNNING ABOVE THE PROGRESS BAR
        bool hasRunner = animatedFeatureSprites != null && animatedFeatureSprites.Count > 0 && animatedFeatureSprites[0] != null;
        if (hasRunner)
        {
            GameObject shadObj = new GameObject("RunnerShadow", typeof(RectTransform), typeof(Image));
            shadObj.transform.SetParent(contentObj.transform, false);
            runnerShadowRect = shadObj.GetComponent<RectTransform>();
            runnerShadowRect.anchorMin = runnerShadowRect.anchorMax = new Vector2(0.5f, 0f);
            runnerShadowRect.anchoredPosition = new Vector2(0f, 146f);
            runnerShadowRect.sizeDelta = new Vector2(96f, 16f);
            Image shImg = shadObj.GetComponent<Image>();
            shImg.sprite = softCircleSprite;
            shImg.color = new Color(0f, 0f, 0f, 0.75f);

            GameObject runObj = new GameObject("HorizonRunner", typeof(RectTransform), typeof(Image));
            runObj.transform.SetParent(contentObj.transform, false);
            runnerRect = runObj.GetComponent<RectTransform>();
            runnerRect.anchorMin = runnerRect.anchorMax = new Vector2(0.5f, 0f);
            runnerRect.pivot = new Vector2(0.5f, 0f);
            runnerRect.anchoredPosition = new Vector2(0f, 148f);
            runnerRect.sizeDelta = new Vector2(96f * characterZoom, 96f * characterZoom);
            runnerImg = runObj.GetComponent<Image>();
            runnerImg.sprite = animatedFeatureSprites[0];
            runnerImg.preserveAspect = true;
            runnerImg.raycastTarget = false;
        }

        // 10. SEGMENTED MOLTEN PROGRESS GAUGE
        barBaseAnchoredPos = new Vector2(0f, 96f);
        GameObject barInner = CreateFramedPanel(contentObj.transform, "ProgressBarFrame",
            new Vector2(0.5f, 0f), barBaseAnchoredPos, new Vector2(BAR_WIDTH + 16f, 38f), out barOuterRect);

        GameObject barTrack = new GameObject("BarTrack", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        barTrack.transform.SetParent(barInner.transform, false);
        RectTransform trackRect = barTrack.GetComponent<RectTransform>();
        StretchFull(trackRect);
        trackRect.offsetMin = new Vector2(4f, 4f);
        trackRect.offsetMax = new Vector2(-4f, -4f);
        barTrack.GetComponent<Image>().color = new Color(0.05f, 0.025f, 0.015f, 1f);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(barTrack.transform, false);
        progressFillRect = fillObj.GetComponent<RectTransform>();
        progressFillRect.anchorMin = new Vector2(0f, 0f);
        progressFillRect.anchorMax = new Vector2(0f, 1f);
        progressFillRect.pivot = new Vector2(0f, 0.5f);
        progressFillRect.anchoredPosition = Vector2.zero;
        progressFillRect.sizeDelta = new Vector2(0f, 0f);
        fillObj.GetComponent<Image>().color = emberOrange;

        GameObject topHighlight = new GameObject("FillTopGold", typeof(RectTransform), typeof(Image));
        topHighlight.transform.SetParent(fillObj.transform, false);
        RectTransform thRect = topHighlight.GetComponent<RectTransform>();
        thRect.anchorMin = new Vector2(0f, 1f);
        thRect.anchorMax = new Vector2(1f, 1f);
        thRect.pivot = new Vector2(0.5f, 1f);
        thRect.anchoredPosition = Vector2.zero;
        thRect.sizeDelta = new Vector2(0f, 4f);
        topHighlight.GetComponent<Image>().color = sunGold;

        GameObject shimObj = new GameObject("BarShimmer", typeof(RectTransform), typeof(Image));
        shimObj.transform.SetParent(fillObj.transform, false);
        barShimmerRect = shimObj.GetComponent<RectTransform>();
        barShimmerRect.anchorMin = new Vector2(0f, 0f);
        barShimmerRect.anchorMax = new Vector2(0f, 1f);
        barShimmerRect.sizeDelta = new Vector2(220f, 0f);
        Image shimImg = shimObj.GetComponent<Image>();
        shimImg.sprite = streakSprite;
        shimImg.color = new Color(1f, 0.96f, 0.82f, 0.6f);

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
            notch.GetComponent<Image>().color = new Color(0.04f, 0.02f, 0.01f, 0.85f);
        }

        GameObject flashObj = new GameObject("BarMilestoneFlash", typeof(RectTransform), typeof(Image));
        flashObj.transform.SetParent(barTrack.transform, false);
        StretchFull(flashObj.GetComponent<RectTransform>());
        barFlashOverlay = flashObj.GetComponent<Image>();
        barFlashOverlay.color = new Color(1f, 0.92f, 0.65f, 0f);
        barFlashOverlay.raycastTarget = false;

        GameObject glowDot = new GameObject("LeadingSpark", typeof(RectTransform), typeof(Image));
        glowDot.transform.SetParent(barTrack.transform, false);
        progressGlowRect = glowDot.GetComponent<RectTransform>();
        progressGlowRect.anchorMin = progressGlowRect.anchorMax = new Vector2(0f, 0.5f);
        progressGlowRect.sizeDelta = new Vector2(58f, 58f);
        Image gdImg = glowDot.GetComponent<Image>();
        gdImg.sprite = softCircleSprite;
        gdImg.color = sunGold;

        statusText = CreateText(contentObj.transform, "StatusText", "PREPARING PROLOGUE...", 22f, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-BAR_WIDTH * 0.5f, 144f), new Vector2(900f, 36f),
            TextAlignmentOptions.BottomLeft, Color.white);
        statusText.characterSpacing = 6f;

        percentText = CreateText(contentObj.transform, "PercentText", "0%", 28f, FontStyles.Bold,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(BAR_WIDTH * 0.5f, 142f), new Vector2(300f, 40f),
            TextAlignmentOptions.BottomRight, sunGold);

        // 11. FOREGROUND WIND STREAKS & BOKEH EMBERS
        for (int i = 0; i < 16; i++)
        {
            GameObject dObj = new GameObject("ForegroundWind", typeof(RectTransform), typeof(Image));
            dObj.transform.SetParent(rootObj.transform, false);
            RectTransform windRect = dObj.GetComponent<RectTransform>();
            windRect.sizeDelta = new Vector2(Random.Range(280f, 600f), Random.Range(2f, 5f));
            windRect.localRotation = Quaternion.Euler(0f, 0f, 3.5f);
            windRect.anchoredPosition = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-480f, 480f));
            Image dImg = dObj.GetComponent<Image>();
            dImg.sprite = streakSprite;
            dImg.color = new Color(1f, 0.78f, 0.52f, Random.Range(0.06f, 0.15f));
            dImg.raycastTarget = false;
            dustStreaks.Add(windRect);
            dustSpeeds.Add(Random.Range(450f, 800f));
        }

        for (int i = 0; i < 22; i++)
            SpawnAmbientEmber(rootObj.transform, isForeground: true);

        // 12. CINEMA SHUTTER BARS
        topShutterRect = CreateShutterBar(rootObj.transform, true, 58f);
        bottomShutterRect = CreateShutterBar(rootObj.transform, false, 58f);
    }

    private void Update()
    {
        if (!isLoading) return;

        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;

        if (backdropArtRect != null)
        {
            float scale = 1.16f + Mathf.Sin(t * 0.22f) * 0.03f;
            backdropArtRect.localScale = new Vector3(scale, scale, 1f);

            Vector2 mouseShift = Vector2.zero;
            if (Screen.width > 0 && Screen.height > 0)
            {
                float nx = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
                float ny = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;
                mouseShift = new Vector2(-nx * 26f, -ny * 14f);
            }

            Vector2 drift = new Vector2(Mathf.Sin(t * 0.18f) * 28f, Mathf.Cos(t * 0.15f) * 14f);
            backdropArtRect.anchoredPosition = Vector2.Lerp(backdropArtRect.anchoredPosition, drift + mouseShift, dt * 3f);
        }

        for (int i = 0; i < heatMirageBands.Count; i++)
        {
            RectTransform mr = heatMirageBands[i];
            if (mr == null) continue;
            Vector2 p = mr.anchoredPosition;
            p.x = Mathf.Sin(t * 1.1f + i * 1.5f) * 90f;
            mr.anchoredPosition = p;
        }

        if (sunBeam1 != null)
        {
            Color c = sunBeam1.color;
            c.a = 0.09f + Mathf.Sin(t * 1.1f) * 0.04f;
            sunBeam1.color = c;
        }
        if (sunBeam2 != null)
        {
            Color c = sunBeam2.color;
            c.a = 0.07f + Mathf.Cos(t * 0.9f) * 0.03f;
            sunBeam2.color = c;
        }
        if (sunBeam3 != null)
        {
            Color c = sunBeam3.color;
            c.a = 0.06f + Mathf.Sin(t * 1.4f + 1.2f) * 0.03f;
            sunBeam3.color = c;
        }
        if (heatPulseOverlay != null)
        {
            Color hc = heatPulseOverlay.color;
            hc.a = 0.10f + Mathf.Sin(t * 1.8f) * 0.04f;
            heatPulseOverlay.color = hc;
        }

        if (runnerImg != null && animatedFeatureSprites.Count > 0)
        {
            if (animatedFeatureSprites.Count > 1)
            {
                spriteFrameTimer += dt;
                if (spriteFrameTimer >= 1f / Mathf.Max(1f, spriteAnimationFps))
                {
                    spriteFrameTimer = 0f;
                    spriteFrameIndex = (spriteFrameIndex + 1) % animatedFeatureSprites.Count;
                    if (animatedFeatureSprites[spriteFrameIndex] != null)
                        runnerImg.sprite = animatedFeatureSprites[spriteFrameIndex];
                }
            }
            else if (runnerRect != null)
            {
                float bob = Mathf.Abs(Mathf.Sin(t * 4f)) * 6f;
                runnerRect.anchoredPosition = new Vector2(runnerRect.anchoredPosition.x, 148f + bob);
            }
        }

        if (barShimmerRect != null)
        {
            float sweepX = Mathf.Repeat(t * 760f, BAR_WIDTH + 400f) - 200f;
            barShimmerRect.anchoredPosition = new Vector2(sweepX, 0f);
        }

        if (barOuterRect != null && barRecoilOffset > 0f)
        {
            barRecoilOffset = Mathf.MoveTowards(barRecoilOffset, 0f, dt * 32f);
            barOuterRect.anchoredPosition = barBaseAnchoredPos + new Vector2(0f, -barRecoilOffset);
        }

        if (barFlashOverlay != null && barFlashAlpha > 0f)
        {
            barFlashAlpha = Mathf.MoveTowards(barFlashAlpha, 0f, dt * 1.8f);
            Color fc = barFlashOverlay.color;
            fc.a = barFlashAlpha;
            barFlashOverlay.color = fc;
        }

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
            pos.x += Mathf.Sin(t * 2.4f + p.phase) * 26f * dt;
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
            sp.velocity.y -= 280f * dt;
            sp.rect.anchoredPosition += sp.velocity * dt;
            Color sc = sp.img.color;
            sc.a = 1f - (sp.life / sp.maxLife);
            sp.img.color = sc;
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

        lastMilestone = 0;
        barFlashAlpha = 0f;
        barRecoilOffset = 0f;
        SetProgress(0f);

        topShutterRect.sizeDelta = new Vector2(0f, 58f);
        bottomShutterRect.sizeDelta = new Vector2(0f, 58f);
        contentGroup.alpha = 0f;
        centerTitleRect.localScale = Vector3.one * 0.94f;
        chapterTitleTmp.characterSpacing = 32f;
        leftWingLine.localScale = new Vector3(0f, 1f, 1f);
        rightWingLine.localScale = new Vector3(0f, 1f, 1f);

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

        StartCoroutine(RevealTitleCardRoutine());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float loadTimer = 0f;
        float displayed = 0f;

        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            loadTimer += dt;

            float realProg = Mathf.Clamp01(op.progress / 0.9f);
            float timeProg = Mathf.Clamp01(loadTimer / minimumLoadTime);
            float target = Mathf.Min(realProg, timeProg);

            displayed = Mathf.MoveTowards(displayed, target, dt * 1.05f);
            SetProgress(displayed);

            if (displayed > 0.02f && displayed < 0.99f && Random.value < dt * 22f)
            {
                SpawnGaugeSparks(displayed, 1);
            }

            int milestone = Mathf.FloorToInt(displayed * 4f);
            if (milestone > lastMilestone && milestone <= 3)
            {
                lastMilestone = milestone;
                barFlashAlpha = 0.65f;
                barRecoilOffset = 5f;
                SpawnGaugeSparks(displayed, 12);
            }

            if (displayed > 0.6f)
                statusText.text = "LOADING ENVIRONMENT & CUTSCENE...";

            if (op.progress >= 0.9f && loadTimer >= minimumLoadTime && displayed >= 0.99f)
            {
                SetProgress(1f);
                barFlashAlpha = 0.9f;
                barRecoilOffset = 7f;
                SpawnGaugeSparks(1f, 20);
                statusText.text = "READY";
                break;
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.45f);

        float shutterCloseTime = 0.32f;
        elapsed = 0f;
        while (elapsed < shutterCloseTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / shutterCloseTime);
            float h = Mathf.Lerp(58f, 545f, k);
            topShutterRect.sizeDelta = new Vector2(0f, h);
            bottomShutterRect.sizeDelta = new Vector2(0f, h);
            contentGroup.alpha = 1f - k;
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
        yield return null;

        backdropArtImg.enabled = false;
        vignetteImg.enabled = false;
        heatPulseOverlay.enabled = false;
        sunBeam1.enabled = false;
        sunBeam2.enabled = false;
        sunBeam3.enabled = false;
        rootBgImg.enabled = false;

        float shutterOpenTime = 0.55f;
        elapsed = 0f;
        while (elapsed < shutterOpenTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, elapsed / shutterOpenTime);
            float h = Mathf.Lerp(545f, 0f, k);
            topShutterRect.sizeDelta = new Vector2(0f, h);
            bottomShutterRect.sizeDelta = new Vector2(0f, h);
            AudioListener.volume = Mathf.Lerp(0f, startVol, k);
            yield return null;
        }

        backdropArtImg.enabled = true;
        vignetteImg.enabled = true;
        heatPulseOverlay.enabled = true;
        sunBeam1.enabled = true;
        sunBeam2.enabled = true;
        sunBeam3.enabled = true;
        rootBgImg.enabled = true;

        AudioListener.volume = startVol;
        rootGroup.alpha = 0f;
        rootGroup.blocksRaycasts = false;
        rootGroup.gameObject.SetActive(false);
        isLoading = false;
    }

    private IEnumerator RevealTitleCardRoutine()
    {
        float dur = 0.75f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / dur), 3f);
            contentGroup.alpha = ease;
            centerTitleRect.localScale = Vector3.Lerp(Vector3.one * 0.94f, Vector3.one, ease);
            chapterTitleTmp.characterSpacing = Mathf.Lerp(32f, 14f, ease);
            leftWingLine.localScale = new Vector3(ease, 1f, 1f);
            rightWingLine.localScale = new Vector3(ease, 1f, 1f);
            yield return null;
        }
        contentGroup.alpha = 1f;
        centerTitleRect.localScale = Vector3.one;
        chapterTitleTmp.characterSpacing = 14f;
    }

    private void SetProgress(float val)
    {
        val = Mathf.Clamp01(val);
        float w = BAR_WIDTH * val;
        if (progressFillRect != null) progressFillRect.sizeDelta = new Vector2(w, 0f);
        if (progressGlowRect != null) progressGlowRect.anchoredPosition = new Vector2(w, 0f);
        if (percentText != null) percentText.text = $"{Mathf.RoundToInt(val * 100f)}%";

        // Character runs along the progress bar tip!
        if (runnerRect != null)
        {
            float rx = -BAR_WIDTH * 0.5f + w;
            runnerRect.anchoredPosition = new Vector2(rx, runnerRect.anchoredPosition.y);
            if (runnerShadowRect != null)
                runnerShadowRect.anchoredPosition = new Vector2(rx, 146f);
        }
    }

    private RectTransform CreateLine(Transform parent, string name, Vector2 pos, Vector2 size, Color col)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        obj.GetComponent<Image>().color = col;
        return r;
    }

    private void SpawnAmbientEmber(Transform parent, bool isForeground)
    {
        GameObject eObj = new GameObject(isForeground ? "FgEmber" : "BgEmber", typeof(RectTransform), typeof(Image));
        eObj.transform.SetParent(parent, false);
        RectTransform rect = eObj.GetComponent<RectTransform>();
        float sz = isForeground ? Random.Range(11f, 22f) : Random.Range(3f, 7f);
        rect.sizeDelta = new Vector2(sz, sz);
        rect.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));

        Image img = eObj.GetComponent<Image>();
        img.sprite = softCircleSprite;
        float alpha = isForeground ? Random.Range(0.22f, 0.45f) : Random.Range(0.45f, 0.85f);
        img.color = Random.value > 0.25f
            ? new Color(1f, Random.Range(0.48f, 0.78f), 0.14f, alpha)
            : new Color(1f, 0.94f, 0.68f, alpha);
        img.raycastTarget = false;

        float speedMult = isForeground ? 1.55f : 0.85f;
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
        if (barOuterRect == null) return;
        float spawnX = -BAR_WIDTH * 0.5f + (BAR_WIDTH * normalizedX);

        for (int i = 0; i < count; i++)
        {
            GameObject spObj = new GameObject("GaugeSpark", typeof(RectTransform), typeof(Image));
            spObj.transform.SetParent(barOuterRect.parent, false);
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
                velocity = new Vector2(Random.Range(-120f, 150f), Random.Range(100f, 240f)),
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
        bar.GetComponent<Image>().color = new Color(0.01f, 0.006f, 0.004f, 1f);
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