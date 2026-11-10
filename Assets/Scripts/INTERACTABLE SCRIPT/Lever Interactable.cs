using System.Collections;
using UnityEngine;

public class LeverInteractable : MonoBehaviour
{
    [Header("components")]
    public SpriteRenderer spriteRenderer;

    [Header("sprites")]
    public Sprite leverOnSprite;
    public Sprite leverOffSprite;

    [Header("settings")]
    public bool isActivated = false;
    public KeyCode ineractKey = KeyCode.E;

    private bool isPlayerInRange = false;
    private Vector3 originalScale;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        originalScale = transform.localScale;
        UpdateSprite();
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(ineractKey))
        {
            ToggleLever();
        }
    }

    public void ToggleLever()
    {
        isActivated = !isActivated;
        UpdateSprite();

        StopAllCoroutines();
        StartCoroutine(PunchScaleEffect());

        if (isActivated)
        {
            Debug.Log("Lever activated!");
        }
        else
        {
            Debug.Log("Lever deactivated!");
        }
    }

    private void UpdateSprite()
    {
        
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = isActivated;
    }

    private IEnumerator PunchScaleEffect()
    {
        Vector3 squishedScale = new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z);
        transform.localScale = squishedScale;
        float elapsedTime = 0f;
        float duration = 0.1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squishedScale, originalScale, elapsedTime / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player in range of lever.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left range of lever.");
        }
    }
}