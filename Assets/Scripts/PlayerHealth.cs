using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    public bool isDead {get; private set;} = false;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    [Header("Checkpoint Settings")]
    public Vector3 currentCheckpoint;

    [Header("References")]
    public HeartUI heartUI;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        currentCheckpoint = transform.position; // Default checkpoint is start position
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }
    }
    public void TakeDamage(int damageAmount)
    {
        if (isInvincible ||isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }
    public void FallInHazard()
    {
        if (isDead) return;

        currentHealth -= 1;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Respawn at nearest zone checkpoint
            transform.position = currentCheckpoint;
            StartCoroutine(InvincibilityRoutine());
        }
    }
    public void InstantDeath()
    {
        currentHealth = 0;
        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }
        Die();
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        float flashInterval = 0.1f;

        // Visual blinking effect during invincibility
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Game Over!");
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        currentCheckpoint = newPosition;
    }
}