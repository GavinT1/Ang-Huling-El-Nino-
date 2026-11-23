using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerFootstepAudio : MonoBehaviour
{
    public enum SurfaceType
    {
        Grass,
        CaveStone,
        Mud,
        MetalOrWood
    }

    [System.Serializable]
    public class SurfaceFootstepSet
    {
        public string surfaceName = "Grass";
        public SurfaceType surfaceType = SurfaceType.Grass;

        [Header("Slow / Dehydrated Walk Clips")]
        public AudioClip[] slowSteps;

        [Header("Normal Walk Clips")]
        public AudioClip[] genericSteps;

        [Header("Sprint Clips (Alternating Left / Right)")]
        public AudioClip[] fastLeftFoot;
        public AudioClip[] fastRightFoot;

        [Header("Downhill Sprint Boost Clips (Alternating Left / Right)")]
        public AudioClip[] fasterLeftFoot;
        public AudioClip[] fasterRightFoot;

        [Header("Optional Jump / Land Clips")]
        public AudioClip[] jumpClips;
        public AudioClip[] landClips;
    }

    [Header("Surface Audio Sets (Area 1 to Area 3)")]
    [SerializeField] private SurfaceType currentSurface = SurfaceType.Grass;
    [SerializeField] private SurfaceFootstepSet[] surfaceSets;

    [Header("Ground & Speed Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Step Timing (Seconds Between Steps)")]
    [SerializeField] private float slowStepInterval = 0.52f;
    [SerializeField] private float walkStepInterval = 0.38f;
    [SerializeField] private float fastStepInterval = 0.25f;
    [SerializeField] private float fasterStepInterval = 0.19f;

    [Header("Speed Thresholds")]
    [Tooltip("Below this speed uses Slow_Step (e.g., when dehydrated)")]
    [SerializeField] private float slowSpeedThreshold = 4.2f;
    [Tooltip("Above this speed uses Fast Left/Right")]
    [SerializeField] private float fastSpeedThreshold = 6.5f;
    [Tooltip("Above this speed uses Faster Left/Right (e.g., downhill sprint)")]
    [SerializeField] private float fasterSpeedThreshold = 9.5f;

    [Header("Volume & Pitch Variation")]
    [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float landVolume = 0.85f;
    [SerializeField] private float minPitch = 0.94f;
    [SerializeField] private float maxPitch = 1.06f;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private PlayerHealth healthSystem;
    private PlayerHydration hydrationSystem;
    private PlayerMovement playerMovement;

    private float stepTimer;
    private bool useLeftFoot = true;
    private bool wasGrounded;
    private int lastClipIndex = -1;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<PlayerHealth>();
        hydrationSystem = GetComponent<PlayerHydration>();
        playerMovement = GetComponent<PlayerMovement>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Pure 2D audio for platformer clarity
    }

    private void Update()
    {
        if (healthSystem != null && healthSystem.isDead) return;

        bool isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
        }

        // Play landing sound when touching down from a jump/fall
        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.1f)
        {
            PlayLandingSound();
            stepTimer = walkStepInterval * 0.6f; // Brief pause before next walk step
        }
        wasGrounded = isGrounded;

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        bool isMovingHorizontally = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && horizontalSpeed > 0.2f;

        if (!isGrounded || !isMovingHorizontally)
        {
            stepTimer = 0.05f; // Ready to trigger immediately when starting to walk
            return;
        }

        // Choose step interval based on your PlayerMovement speeds (dehydrated walk, normal walk, sprint, downhill sprint)
        float currentInterval = walkStepInterval;
        if (horizontalSpeed >= fasterSpeedThreshold)
            currentInterval = fasterStepInterval;
        else if (horizontalSpeed >= fastSpeedThreshold)
            currentInterval = fastStepInterval;
        else if (horizontalSpeed < slowSpeedThreshold)
            currentInterval = slowStepInterval;

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep(horizontalSpeed);
            stepTimer = currentInterval;
        }
    }

    private void PlayFootstep(float horizontalSpeed)
    {
        SurfaceFootstepSet set = GetCurrentSurfaceSet();
        if (set == null) return;

        AudioClip[] chosenPool = null;

        if (horizontalSpeed >= fasterSpeedThreshold)
        {
            chosenPool = useLeftFoot ? set.fasterLeftFoot : set.fasterRightFoot;
            if (chosenPool == null || chosenPool.Length == 0)
                chosenPool = useLeftFoot ? set.fastLeftFoot : set.fastRightFoot;
        }
        else if (horizontalSpeed >= fastSpeedThreshold)
        {
            chosenPool = useLeftFoot ? set.fastLeftFoot : set.fastRightFoot;
        }
        else if (horizontalSpeed < slowSpeedThreshold)
        {
            chosenPool = set.slowSteps;
        }

        // Fallback to Generic_Step if a tier is empty or at normal walk speed
        if (chosenPool == null || chosenPool.Length == 0)
        {
            chosenPool = set.genericSteps;
        }

        AudioClip clip = PickRandomClip(chosenPool);
        if (clip != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip, footstepVolume);
            useLeftFoot = !useLeftFoot; // Alternate Left -> Right -> Left
        }
    }

    private void PlayLandingSound()
    {
        SurfaceFootstepSet set = GetCurrentSurfaceSet();
        if (set == null) return;

        AudioClip[] pool = (set.landClips != null && set.landClips.Length > 0) ? set.landClips : set.genericSteps;
        AudioClip clip = PickRandomClip(pool);
        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.0f);
            audioSource.PlayOneShot(clip, landVolume);
        }
    }

    private SurfaceFootstepSet GetCurrentSurfaceSet()
    {
        if (surfaceSets == null || surfaceSets.Length == 0) return null;

        for (int i = 0; i < surfaceSets.Length; i++)
        {
            if (surfaceSets[i].surfaceType == currentSurface)
                return surfaceSets[i];
        }
        return surfaceSets[0];
    }

    private AudioClip PickRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        int idx = Random.Range(0, clips.Length);
        if (idx == lastClipIndex)
        {
            idx = (idx + 1) % clips.Length;
        }
        lastClipIndex = idx;
        return clips[idx];
    }

    public void SetSurface(SurfaceType newSurface)
    {
        currentSurface = newSurface;
    }
}