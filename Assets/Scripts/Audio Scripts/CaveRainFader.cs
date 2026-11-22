using UnityEngine;

public class CaveRainFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource outdoorRainAudio;
    [SerializeField] private AudioSource indoorCaveRainAudio;
    [SerializeField] private AudioLowPassFilter lowPassFilter;

    [Header("1st-Floor Entrance Markers")]
    [SerializeField] private Transform caveEntrancePoint; // Place right at the 1st-floor doorway
    [SerializeField] private Transform deepCavePoint;     // Place further right inside the 1st-floor tunnel

    [Header("2nd-Floor Ceiling Cutoff")]
    [SerializeField] private Transform secondFloorCutoffPoint; // Place just above the 1st-floor ceiling

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float maxOutdoorVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float maxIndoorVolume = 0.35f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Muffle Settings")]
    [SerializeField] private float outdoorCutoff = 22000f;
    [SerializeField] private float caveCutoff = 900f;

    private void Update()
    {
        if (player == null || caveEntrancePoint == null || deepCavePoint == null) return;

        // 0 = outside or at the 1st-floor doorway, 1 = deep inside the 1st-floor cave
        float depthProgress = Mathf.InverseLerp(
            caveEntrancePoint.position.x,
            deepCavePoint.position.x,
            player.position.x
        );

        // If the player is inside the cave X-zone AND climbed above the 1st-floor ceiling, force silence (1.0)
        bool isInsideCaveX = player.position.x > caveEntrancePoint.position.x;
        bool isAboveFirstFloor = secondFloorCutoffPoint != null && player.position.y > secondFloorCutoffPoint.position.y;

        if (isInsideCaveX && isAboveFirstFloor)
        {
            depthProgress = 1f;
        }

        float targetOutdoor = Mathf.Lerp(maxOutdoorVolume, 0f, depthProgress);
        float targetIndoor = Mathf.Lerp(maxIndoorVolume, 0f, Mathf.InverseLerp(0f, 0.6f, depthProgress));
        float targetCutoff = Mathf.Lerp(outdoorCutoff, caveCutoff, depthProgress);

        if (outdoorRainAudio != null)
            outdoorRainAudio.volume = Mathf.MoveTowards(outdoorRainAudio.volume, targetOutdoor, fadeSpeed * Time.deltaTime);

        if (indoorCaveRainAudio != null)
            indoorCaveRainAudio.volume = Mathf.MoveTowards(indoorCaveRainAudio.volume, targetIndoor, fadeSpeed * Time.deltaTime);

        if (lowPassFilter != null)
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, targetCutoff, 15000f * Time.deltaTime);
    }
}