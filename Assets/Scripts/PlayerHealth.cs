using System.Collections;
using System.Numerics;
using NUnit.Framework;      
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


    [Header("References")]
    public HeartUI heartUI;
    public GameObject deathChoicePanel;
    public RisingWater risingWater;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerHydration hydration;


    void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        hydration = GetComponent<PlayerHydration>();

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }

        if (deathChoicePanel !=null)
        {
            deathChoicePanel.SetActive(false);
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
            DieByEnvironment();
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
            DieByEnvironment();
        }
        else
        {
            // Respawn at nearest zone checkpoint
            RespawnAtCheckpoint();
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
        DieByEnvironment();
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

    public void DieByEnvironment()
    {
        if (isDead) return;
        Die();

        if(CheckpointManager.Instance !=null && CheckpointManager.Instance.HasReachedCheckpoint())
        {
            if(deathChoicePanel !=null)
            {
                deathChoicePanel.SetActive(true);
            }
        }
        else
        {
            RestartFromBeginning();
        }
    }

    public void OnClick_RespawnAtCheckpoint()
    {
        if(deathChoicePanel !=null)
        {
            deathChoicePanel.SetActive(false);
        }
        FullResetAndSpawn();
    }

    public void OnClick_RestartFromBeginning()
    {
        if(deathChoicePanel !=null)
        {
            deathChoicePanel.SetActive(false);
        }
        RestartFromBeginning();
    }

    public void RestartFromBeginning()
    {
        isDead = false;
        currentHealth = maxHealth;

        if(hydration !=null)
        {
            hydration.ResetHydration();
        }
        
        if(heartUI !=null)
        {
            heartUI.UpdateHearts(currentHealth);
        }

        if(CheckpointManager.Instance !=null)
        {
            transform.position = CheckpointManager.Instance.GetDefaultSpawnPosition();
        }

        if (rb !=null)
        {
            rb.linearVelocity = UnityEngine.Vector2.zero;
        }

        if (risingWater !=null)
        {
            risingWater.ResetWater();
        }
    }

    public void RespawnAtCheckpoint()
    {
        if(CheckpointManager.Instance !=null)
        {
            transform.position = CheckpointManager.Instance.GetCheckPointPosition();
        }

        if (rb !=null)
        {
            rb.linearVelocity = UnityEngine.Vector2.zero;
        }
    }

    public void FullResetAndSpawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        if(hydration !=null)
        {
            hydration.ResetHydration();
        }
        
        if (risingWater !=null)
        {
            risingWater.ResetWater();
        }

        if (heartUI !=null)
        {
            heartUI.UpdateHearts(currentHealth);
        }
        RespawnAtCheckpoint();
    }
}