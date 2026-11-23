using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FloodAlertUI : MonoBehaviour
{
    [Header("Alert Timing")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Pulse Animation")]
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float minAlphaDuringPulse = 0.45f;
    [SerializeField] private float scalePunchAmount = 0.05f;

    [Header("Optional Alert Sound")]
    [SerializeField] private AudioSource alertAudioSource;
    [SerializeField] private AudioClip alertSfx;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Coroutine alertRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;

        // Start completely hidden
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void TriggerAlert()
    {
        if (alertRoutine != null)
        {
            StopCoroutine(alertRoutine);
        }
        alertRoutine = StartCoroutine(ShowAlertRoutine());
    }

    public void HideAlertImmediately()
    {
        if (alertRoutine != null)
        {
            StopCoroutine(alertRoutine);
            alertRoutine = null;
        }
        canvasGroup.alpha = 0f;
        transform.localScale = originalScale;
    }

    private IEnumerator ShowAlertRoutine()
    {
        if (alertAudioSource != null && alertSfx != null)
        {
            alertAudioSource.PlayOneShot(alertSfx);
        }

        // 1. Quick fade-in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        // 2. Pulse & subtle scale punch for the main alert window
        float activeTime = Mathf.Max(0f, displayDuration - (fadeDuration * 2f));
        float elapsed = 0f;
        while (elapsed < activeTime)
        {
            elapsed += Time.deltaTime;
            float wave = (Mathf.Sin(elapsed * pulseSpeed) + 1f) * 0.5f; // 0 to 1
            canvasGroup.alpha = Mathf.Lerp(minAlphaDuringPulse, 1f, wave);
            transform.localScale = originalScale * (1f + wave * scalePunchAmount);
            yield return null;
        }

        // 3. Quick fade-out
        t = 0f;
        float startAlpha = canvasGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = originalScale;
        alertRoutine = null;
    }
}