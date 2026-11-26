using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Custom Spawn Offset")]
    public Transform respawnPoint;

    [Header("Floating Prompt (Same as Notice Board)")]
    public GameObject interactPrompt;
    public InspectPrompt inspectPromptAnim;
    public bool hidePromptAfterFirstVisit = false;

    [Header("Soft Blink / Pulse on Checkpoint Item")]
    public SpriteRenderer itemSprite;
    public bool pulseWhenPlayerNear = true;
    public bool keepPulsingAfterActivated = true;
    public float pulseSpeed = 1.6f;          // Lower = slower, gentler breathing
    [Range(0.2f, 1f)]
    public float minAlpha = 0.55f;           // Keeps sprite visible so it's never flashy
    public Color activeTint = Color.white;   // Optional subtle highlight tint

    [Header("Background Light 2D Glow")]
    public Light2D checkpointLight;
    public float minLightIntensity = 0.4f;
    public float maxLightIntensity = 1.2f;

    private Animator anim;
    private Color originalColor = Color.white;
    private bool isActivated = false;
    private bool playerInRange = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }

        if (itemSprite == null)
        {
            itemSprite = GetComponent<SpriteRenderer>();
        }
        if (itemSprite != null)
        {
            originalColor = itemSprite.color;
        }

        if (checkpointLight == null)
        {
            checkpointLight = GetComponentInChildren<Light2D>();
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
    }

    private void Update()
    {
        bool shouldPulse = (pulseWhenPlayerNear && playerInRange) || (keepPulsingAfterActivated && isActivated);

        if (shouldPulse)
        {
            // Smooth sine wave between 0 and 1 (no harsh on/off strobe)
            float wave = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f;

            if (itemSprite != null)
            {
                float alpha = Mathf.Lerp(minAlpha, 1f, wave);
                Color targetColor = isActivated ? activeTint : originalColor;
                targetColor.a = alpha;
                itemSprite.color = targetColor;
            }

            if (checkpointLight != null)
            {
                checkpointLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, wave);
            }
        }
        else
        {
            if (itemSprite != null)
            {
                itemSprite.color = isActivated ? activeTint : originalColor;
            }

            if (checkpointLight != null)
            {
                checkpointLight.intensity = minLightIntensity;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("FlagTrigger" + other.name);

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player tag detected! Updating checkpoint");
            if (!isActivated)
            {
                isActivated = true;
                if (anim != null)
                {
                    anim.SetTrigger("Activate");
                }
            }

            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetActiveCheckpoint(respawnPoint.position);
            }
            else
            {
                Debug.LogError("CheckpointManager Instance is MISSING in the scene!");
            }

            if (!hidePromptAfterFirstVisit || !isActivated)
            {
                if (inspectPromptAnim != null) inspectPromptAnim.ShowPrompt();
                else if (interactPrompt != null) interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (inspectPromptAnim != null) inspectPromptAnim.HidePrompt();
            else if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}