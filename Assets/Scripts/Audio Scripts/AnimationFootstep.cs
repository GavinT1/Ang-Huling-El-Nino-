using UnityEngine;

public class AnimationFootstep : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] stepClips;

    // Called automatically by the Walk animation clip
    public void PlayFootstep()
    {
        if (audioSource == null || stepClips == null || stepClips.Length == 0) return;
        AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
        audioSource.PlayOneShot(clip);
    }
}