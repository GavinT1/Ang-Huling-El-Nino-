using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class InspectPrompt : MonoBehaviour
{
    [Header("Hollow Knight Style Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float minAlpha = 0.65f;

    [Header("Shine Pulse Effect")]
    [SerializeField] private float shineSpeed = 4.5f;

    private CanvasGroup canvasGroup;
    private TMP_Text inspectText;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inspectText = GetComponent<TMP_Text>();
        canvasGroup.alpha = 0f;
    }

    public void ShowPrompt()
    {
        gameObject.SetActive(true);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(FadeAndPulseRoutine(true));
    }

    public void HidePrompt()
    {
        if (!gameObject.activeInHierarchy) return;
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(FadeAndPulseRoutine(false));
    }

    private IEnumerator FadeAndPulseRoutine(bool show)
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? minAlpha : 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;

        if (show)
        {
            pulseRoutine = StartCoroutine(ShinePulseEffect());
        }
        else
        {
            pulseRoutine = null;
        }
    }

    private IEnumerator ShinePulseEffect()
    {
        while (true)
        {
            float wave = Mathf.PingPong(Time.unscaledTime * shineSpeed, 1f);
            canvasGroup.alpha = Mathf.Lerp(minAlpha, 1.0f, wave);
            yield return null;
        }
    }
}