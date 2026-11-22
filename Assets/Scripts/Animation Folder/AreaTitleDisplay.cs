using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AreaTitleDisplay : MonoBehaviour
{
    [Header("Area Text (Customize Per Scene)")]
    [SerializeField] private string areaTitle = "WEEPING OUTSKIRTS";
    [SerializeField] private string areaSubtitle = "THE RAIN-SOAKED BORDER";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [Tooltip("Optional: Drag a divider line or flourish RectTransform here later")]
    [SerializeField] private RectTransform dividerTransform;
    [Tooltip("Drag all your HUD elements (Bar, Hearts, QuestTrackerHUD, etc.) here")]
    [SerializeField] private CanvasGroup[] gameplayHudGroups;

    [Header("Trigger Settings")]
    [SerializeField] private bool showOnSceneStart = true;
    [SerializeField] private float startDelay = 1.0f;

    [Header("Cinematic Timing")]
    [SerializeField] private float hudFadeOutDuration = 0.4f;
    [SerializeField] private float titleSweepDuration = 1.1f;
    [SerializeField] private float subtitleDelay = 0.35f;
    [SerializeField] private float holdDuration = 3.2f;
    [SerializeField] private float fadeOutDuration = 1.8f;
    [SerializeField] private float hudFadeInDuration = 0.7f;

    [Header("Typography & Motion Polish")]
    [SerializeField] private float startTracking = 2f;
    [SerializeField] private float endTracking = 12f;
    [SerializeField] private float exitTracking = 18f;
    [SerializeField] private float verticalDrift = 10f;

    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;
    private bool hasShownThisScene;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        if (titleText != null) titleText.text = areaTitle;
        if (subtitleText != null) subtitleText.text = areaSubtitle;
    }

    private void Start()
    {
        if (showOnSceneStart)
        {
            SetAllHudAlpha(0f);
            ShowAreaTitle();
        }
    }

    public void ShowAreaTitle(bool onlyOnce = true)
    {
        if (onlyOnce && hasShownThisScene) return;
        hasShownThisScene = true;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(CinematicRevealRoutine());
    }

    private IEnumerator CinematicRevealRoutine()
    {
        RectTransform titleRect = titleText != null ? titleText.rectTransform : null;
        RectTransform subRect = subtitleText != null ? subtitleText.rectTransform : null;

        Vector2 titleRestPos = titleRect != null ? titleRect.anchoredPosition : Vector2.zero;
        Vector2 subRestPos = subRect != null ? subRect.anchoredPosition : Vector2.zero;

        // Initial hidden state for banner
        canvasGroup.alpha = 1f;
        SetElementAlpha(titleText, 0f);
        SetElementAlpha(subtitleText, 0f);

        if (titleText != null) titleText.characterSpacing = startTracking;
        if (titleRect != null) titleRect.anchoredPosition = titleRestPos + new Vector2(0f, verticalDrift);
        if (subRect != null) subRect.anchoredPosition = subRestPos + new Vector2(0f, -verticalDrift * 0.7f);
        if (dividerTransform != null) dividerTransform.localScale = new Vector3(0f, 1f, 1f);

        // 0. Fade out Gameplay HUD smoothly (if triggered mid-gameplay)
        float currentHudAlpha = GetMaxHudAlpha();
        if (currentHudAlpha > 0f)
        {
            float hudElapsed = 0f;
            while (hudElapsed < hudFadeOutDuration)
            {
                hudElapsed += Time.deltaTime;
                SetAllHudAlpha(Mathf.Lerp(currentHudAlpha, 0f, hudElapsed / hudFadeOutDuration));
                yield return null;
            }
            SetAllHudAlpha(0f);
        }

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // 1. ENTRANCE: Left-to-right character wave + tracking expansion + counter-drift
        float totalEnter = titleSweepDuration + subtitleDelay;
        float elapsed = 0f;

        while (elapsed < totalEnter)
        {
            elapsed += Time.deltaTime;

            float titleProgress = Mathf.Clamp01(elapsed / titleSweepDuration);
            float titleEase = 1f - Mathf.Pow(1f - titleProgress, 3f);

            if (titleText != null)
            {
                titleText.characterSpacing = Mathf.Lerp(startTracking, endTracking, titleEase);
                ApplyCharacterWaveFade(titleText, titleProgress, 0.35f, verticalDrift * (1f - titleEase));
            }

            if (dividerTransform != null)
            {
                float lineT = Mathf.Clamp01((elapsed - 0.15f) / (titleSweepDuration * 0.8f));
                float lineEase = 1f - Mathf.Pow(1f - lineT, 3f);
                dividerTransform.localScale = new Vector3(lineEase, 1f, 1f);
            }

            float subProgress = Mathf.Clamp01((elapsed - subtitleDelay) / (titleSweepDuration * 0.85f));
            float subEase = 1f - Mathf.Pow(1f - subProgress, 3f);

            if (subtitleText != null)
            {
                SetElementAlpha(subtitleText, subEase);
                subRect.anchoredPosition = Vector2.Lerp(
                    subRestPos + new Vector2(0f, -verticalDrift * 0.7f),
                    subRestPos,
                    subEase
                );
            }

            yield return null;
        }

        // Lock clean resting state
        if (titleText != null)
        {
            titleText.characterSpacing = endTracking;
            ResetMeshState(titleText, 1f);
            titleRect.anchoredPosition = titleRestPos;
        }
        if (subtitleText != null)
        {
            SetElementAlpha(subtitleText, 1f);
            subRect.anchoredPosition = subRestPos;
        }
        if (dividerTransform != null) dividerTransform.localScale = Vector3.one;

        // 2. HOLD: Lingers cleanly while player explores
        yield return new WaitForSeconds(holdDuration);

        // 3. EXIT: Slow atmospheric mist dissolve + HUD gently fades back in near the end
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeOutDuration);

            canvasGroup.alpha = 1f - t;

            if (titleText != null)
            {
                titleText.characterSpacing = Mathf.Lerp(endTracking, exitTracking, t);
            }
            if (dividerTransform != null)
            {
                dividerTransform.localScale = new Vector3(1f - (t * 0.3f), 1f, 1f);
            }

            float hudStartPoint = fadeOutDuration - hudFadeInDuration;
            if (elapsed >= hudStartPoint)
            {
                float hudT = Mathf.SmoothStep(0f, 1f, (elapsed - hudStartPoint) / hudFadeInDuration);
                SetAllHudAlpha(hudT);
            }

            yield return null;
        }

        // Guarantee full restoration
        canvasGroup.alpha = 0f;
        SetAllHudAlpha(1f);
        if (titleRect != null) titleRect.anchoredPosition = titleRestPos;
        if (subRect != null) subRect.anchoredPosition = subRestPos;
        if (titleText != null) titleText.characterSpacing = endTracking;
        activeRoutine = null;
    }

    private float GetMaxHudAlpha()
    {
        if (gameplayHudGroups == null) return 0f;
        float max = 0f;
        foreach (var group in gameplayHudGroups)
        {
            if (group != null && group.alpha > max) max = group.alpha;
        }
        return max;
    }

    private void SetAllHudAlpha(float alpha)
    {
        if (gameplayHudGroups == null) return;
        bool visible = alpha > 0.9f;

        foreach (var group in gameplayHudGroups)
        {
            if (group == null) continue;
            group.alpha = alpha;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }

    private void ApplyCharacterWaveFade(TextMeshProUGUI tmp, float progress, float spread, float yOffset)
    {
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;
        int charCount = textInfo.characterCount;
        if (charCount == 0) return;

        Color32 baseColor = tmp.color;

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            float charNorm = charCount > 1 ? (float)i / (charCount - 1) : 0f;
            float windowStart = charNorm * (1f - spread);
            float charT = Mathf.Clamp01((progress - windowStart) / spread);
            float smoothT = Mathf.SmoothStep(0f, 1f, charT);

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;
            byte alpha = (byte)(smoothT * 255f);

            for (int v = 0; v < 4; v++)
            {
                vertexColors[vertIndex + v] = new Color32(baseColor.r, baseColor.g, baseColor.b, alpha);
            }

            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;
            float charYDrift = yOffset * (1f - smoothT);
            for (int v = 0; v < 4; v++)
            {
                vertices[vertIndex + v].y += charYDrift;
            }
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
    }

    private void ResetMeshState(TextMeshProUGUI tmp, float alpha)
    {
        SetElementAlpha(tmp, alpha);
        tmp.ForceMeshUpdate();
    }

    private void SetElementAlpha(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }
}