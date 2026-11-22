using UnityEngine;
using UnityEngine.Rendering.Universal;

public class JarIdleAnimation : MonoBehaviour
{
    [Header("Floating Bob Settings")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 2.5f;

    [Header("Subtle Tilt")]
    [SerializeField] private bool enableSubtleTilt = true;
    [SerializeField] private float tiltAngle = 4f;

    [Header("Scale Pulse (Breathing)")]
    [SerializeField] private bool enableScalePulse = true;
    [SerializeField] private float pulseAmount = 0.04f;
    [SerializeField] private float pulseSpeed = 3f;

    [Header("2D Light Shimmer (Optional)")]
    [SerializeField] private Light2D jarLight;
    [SerializeField] private float minLightIntensity = 0.65f;
    [SerializeField] private float maxLightIntensity = 1.15f;
    [SerializeField] private float shimmerSpeed = 3.0f;

    [Header("Timing")]
    [SerializeField] private bool randomizeStartOffset = true;

    private Vector3 startLocalPos;
    private Vector3 startLocalScale;
    private Quaternion startLocalRot;
    private float phaseOffset;

    private void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalScale = transform.localScale;
        startLocalRot = transform.localRotation;

        if (jarLight == null)
        {
            jarLight = GetComponentInChildren<Light2D>();
        }

        phaseOffset = randomizeStartOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        float timeWithOffset = Time.time + phaseOffset;

        // 1. Vertical floating bob
        Vector3 newPos = startLocalPos;
        newPos.y += Mathf.Sin(timeWithOffset * bobSpeed) * bobHeight;
        transform.localPosition = newPos;

        // 2. Subtle tilt
        if (enableSubtleTilt)
        {
            float angle = Mathf.Cos(timeWithOffset * bobSpeed * 0.8f) * tiltAngle;
            transform.localRotation = startLocalRot * Quaternion.Euler(0f, 0f, angle);
        }

        // 3. Subtle scale pulse
        if (enableScalePulse)
        {
            float scaleFactor = 1f + (Mathf.Sin(timeWithOffset * pulseSpeed) * pulseAmount);
            transform.localScale = startLocalScale * scaleFactor;
        }

        // 4. 2D Light shimmer
        if (jarLight != null)
        {
            float shimmerWave = (Mathf.Sin(timeWithOffset * shimmerSpeed) + 1f) * 0.5f;
            jarLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, shimmerWave);
        }
    }
}