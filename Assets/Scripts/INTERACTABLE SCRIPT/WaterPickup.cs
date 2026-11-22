using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class WaterPickup : MonoBehaviour
{

    public enum PickupType
    {
        Small,
        Medium,
        Large
    }
    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Small;//20%

    [Header("Audio / fx ")]
    public AudioClip pickupSound;
    private static readonly List<WaterPickup> allJars = new List<WaterPickup>();

    private SpriteRenderer spriteRenderer;
    private Collider2D jarCollider;
    private bool isCollected;

    private ParticleSystem jarParticles;
    private Light2D jarLight;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        jarCollider = GetComponent<Collider2D>();
        jarParticles = GetComponentInChildren<ParticleSystem>();
        jarLight = GetComponentInChildren<Light2D>();
    }

    private void OnEnable()
    {
        if (!allJars.Contains(this))
            allJars.Add(this);
    }

    private void OnDisable()
    {
        allJars.Remove(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;   
        if (collision.CompareTag("Player"))
        {
            PlayerHydration hydration = collision.GetComponent<PlayerHydration>();
            
            if (hydration != null)
            {
                isCollected = true;
                float restorePercent = GetRestorePercentage();
                hydration.RefillHydration(restorePercent);
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                if(spriteRenderer !=null) spriteRenderer.enabled = false;
                if(jarCollider !=null) jarCollider.enabled = false;
                if (jarParticles != null) jarParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (jarLight != null) jarLight.enabled = false;
            }
        }
    }

    public void RespawnJar()
    {
        isCollected = false;
        if (spriteRenderer !=null) spriteRenderer.enabled = true;
        if(jarCollider !=null) jarCollider.enabled = true;
        if (jarParticles != null) jarParticles.Play();
        if (jarLight != null) jarLight.enabled = true;
    }

    public static void RespawnAllJars()
    {
        for (int i = 0; i <allJars.Count; i++)
        {
            if(allJars[i] !=null)
            {
                allJars[i].RespawnJar();
            }
        }
    }

    private float GetRestorePercentage()
    {
        switch (pickupType)
        {
            case PickupType.Large:
            return 100f;

            case PickupType.Medium:
            return 50f;
            
            case PickupType.Small:
            default:
            return 20f;

        }
    }
}