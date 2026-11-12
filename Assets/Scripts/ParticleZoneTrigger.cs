using UnityEngine;

public class ParticleZoneTrigger : MonoBehaviour
{
    public ParticleSystem zoneParticles;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoneParticles !=null)
        {
            zoneParticles.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoneParticles !=null)
        {
            zoneParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
