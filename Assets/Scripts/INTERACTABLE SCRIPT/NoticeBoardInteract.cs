using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;

public class NoticeBoardInteract : MonoBehaviour
{
    [Header("Interaction Prompt & Note UI")]
    public GameObject interactPrompt;
    public InspectPrompt inspectPromptAnim;
    public GameObject backgroundPanel;   // Parent "Panel" (dark backdrop)
    public GameObject notePanel;         // Child "NOTE PANEL"
    public KeyCode interactKey = KeyCode.E;

    [Header("Detection Settings")]
    public float interactRadius = 2.5f;
    public Transform playerTransform;    // Drag your root PLAYER object here!

    [Header("Popup Animation Settings")]
    public float openDuration = 0.25f;
    public float closeDuration = 0.18f;
    public float overshootMultiplier = 1.06f;
    public bool unrollFromTop = false;

    [Header("Cinemachine Zoom Settings")]
    public CinemachineCamera targetVcam;
    public float nearBoardZoom = -0.8f;
    public float readingZoomOut = 1.8f;
    public float zoomSpeed = 5f;

    private RectTransform noteRect;
    private CanvasGroup noteCanvasGroup;
    private CanvasGroup bgCanvasGroup;
    private Vector3 originalScale = Vector3.one;
    private Coroutine animCoroutine;

    private float normalOrthoSize;
    private float targetOrthoSize;
    private float lastToggleTime = -10f;

    private bool playerInRange = false;
    private bool isReading = false;
    private bool hasReadOnce = false;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject namedPlayer = GameObject.Find("PLAYER");
            if (namedPlayer != null)
            {
                playerTransform = namedPlayer.transform;
            }
            else
            {
                GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (taggedPlayer != null) playerTransform = taggedPlayer.transform.root;
            }
        }

        if (targetVcam == null)
        {
            targetVcam = FindAnyObjectByType<CinemachineCamera>();
        }

        if (targetVcam != null)
        {
            normalOrthoSize = targetVcam.Lens.OrthographicSize;
            targetOrthoSize = normalOrthoSize;
        }

        if (interactPrompt != null && inspectPromptAnim == null)
        {
            inspectPromptAnim = interactPrompt.GetComponent<InspectPrompt>();
        }

        if (inspectPromptAnim != null)
        {
            interactPrompt.SetActive(true);
            inspectPromptAnim.HidePrompt();
        }
        else if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        if (backgroundPanel != null)
        {
            bgCanvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
            if (bgCanvasGroup == null) bgCanvasGroup = backgroundPanel.AddComponent<CanvasGroup>();
            backgroundPanel.SetActive(false);
        }

        if (notePanel != null)
        {
            noteRect = notePanel.GetComponent<RectTransform>();
            if (noteRect != null)
            {
                originalScale = noteRect.localScale;
                if (unrollFromTop) noteRect.pivot = new Vector2(0.5f, 1f);
            }

            noteCanvasGroup = notePanel.GetComponent<CanvasGroup>();
            if (noteCanvasGroup == null) noteCanvasGroup = notePanel.AddComponent<CanvasGroup>();
            notePanel.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Distance check
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool nowInRange = dist <= interactRadius;

            if (nowInRange != playerInRange)
            {
                playerInRange = nowInRange;
                UpdateTargetZoom();

                if (playerInRange)
                {
                    if (!isReading)
                    {
                        if (inspectPromptAnim != null) inspectPromptAnim.ShowPrompt();
                        else if (interactPrompt != null) interactPrompt.SetActive(true);
                    }
                }
                else
                {
                    if (inspectPromptAnim != null) inspectPromptAnim.HidePrompt();
                    else if (interactPrompt != null) interactPrompt.SetActive(false);
                    if (isReading) CloseNote();
                }
            }
        }

        // 2. Smooth Cinemachine zoom transition
        if (targetVcam != null)
        {
            var lens = targetVcam.Lens;
            if (Mathf.Abs(lens.OrthographicSize - targetOrthoSize) > 0.001f)
            {
                lens.OrthographicSize = Mathf.Lerp(
                    lens.OrthographicSize,
                    targetOrthoSize,
                    Time.unscaledDeltaTime * zoomSpeed
                );
                targetVcam.Lens = lens;
            }
        }

        // 3. Toggle note cleanly (with anti-double-press cooldown & Esc support)
        if (Time.unscaledTime - lastToggleTime > 0.15f)
        {
            if (isReading && (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape)))
            {
                lastToggleTime = Time.unscaledTime;
                CloseNote();
            }
            else if (!isReading && playerInRange && Input.GetKeyDown(interactKey))
            {
                lastToggleTime = Time.unscaledTime;
                OpenNote();
            }
        }
    }

    private void UpdateTargetZoom()
    {
        if (isReading)
            targetOrthoSize = normalOrthoSize + nearBoardZoom + readingZoomOut;
        else if (playerInRange)
            targetOrthoSize = normalOrthoSize + nearBoardZoom;
        else
            targetOrthoSize = normalOrthoSize;
    }

    public void OpenNote()
    {
        isReading = true;
        UpdateTargetZoom();

        if (inspectPromptAnim != null) inspectPromptAnim.HidePrompt();
        else if (interactPrompt != null) interactPrompt.SetActive(false);

        Time.timeScale = 0f;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateOpen());
    }

    public void CloseNote()
    {
        isReading = false;
        UpdateTargetZoom();
        Time.timeScale = 1f;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateClose());

        if (!hasReadOnce)
        {
            hasReadOnce = true;
            if (QuestObjectiveTracker.Instance != null)
            {
                QuestObjectiveTracker.Instance.SetStage(QuestObjectiveTracker.QuestStage.CollectJars);
            }
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(true);
            bgCanvasGroup.alpha = 0f;
        }

        if (notePanel != null)
        {
            notePanel.SetActive(true);
            noteCanvasGroup.alpha = 0f;
        }

        Vector3 startScale = new Vector3(originalScale.x * 0.85f, originalScale.y * 0.05f, originalScale.z);
        Vector3 bounceScale = new Vector3(originalScale.x * overshootMultiplier, originalScale.y * overshootMultiplier, originalScale.z);
        if (noteRect != null) noteRect.localScale = startScale;

        float bouncePhase = openDuration * 0.7f;
        float settlePhase = openDuration * 0.3f;
        float elapsed = 0f;

        while (elapsed < bouncePhase)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / bouncePhase);

            if (bgCanvasGroup != null) bgCanvasGroup.alpha = t;
            if (noteRect != null) noteRect.localScale = Vector3.Lerp(startScale, bounceScale, t);
            if (noteCanvasGroup != null) noteCanvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
            yield return null;
        }

        if (bgCanvasGroup != null) bgCanvasGroup.alpha = 1f;

        elapsed = 0f;
        while (elapsed < settlePhase)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / settlePhase);

            if (noteRect != null) noteRect.localScale = Vector3.Lerp(bounceScale, originalScale, t);
            yield return null;
        }

        if (noteRect != null) noteRect.localScale = originalScale;
        if (noteCanvasGroup != null) noteCanvasGroup.alpha = 1f;
        animCoroutine = null;
    }

    private IEnumerator AnimateClose()
    {
        float elapsed = 0f;
        Vector3 currentScale = noteRect != null ? noteRect.localScale : originalScale;
        Vector3 endScale = new Vector3(originalScale.x * 0.85f, originalScale.y * 0.05f, originalScale.z);

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);

            if (bgCanvasGroup != null) bgCanvasGroup.alpha = 1f - t;
            if (noteRect != null) noteRect.localScale = Vector3.Lerp(currentScale, endScale, t);
            if (noteCanvasGroup != null) noteCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        if (noteRect != null) noteRect.localScale = originalScale;
        if (noteCanvasGroup != null) noteCanvasGroup.alpha = 0f;
        if (notePanel != null) notePanel.SetActive(false);

        if (bgCanvasGroup != null) bgCanvasGroup.alpha = 0f;
        if (backgroundPanel != null) backgroundPanel.SetActive(false);

        if (playerInRange)
        {
            if (inspectPromptAnim != null) inspectPromptAnim.ShowPrompt();
            else if (interactPrompt != null) interactPrompt.SetActive(true);
        }
        animCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}