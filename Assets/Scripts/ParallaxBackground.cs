using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Target Camera")]
    public Transform cameraTransform;

    [Header("Parallax Depth")]
    [Range(0f, 1f)]
    public float parallaxEffectX = 0.5f;
    [Range(0f, 1f)]
    public float parallaxEffectY = 0.1f;

    [Header("Cave / Zone Cutoff")]
    public bool useMaxCameraX = true;
    public float maxCameraX = 32f;

    [Header("Infinite Horizontal Loop")]
    public bool infiniteHorizontal = false;

    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            lastCameraPosition = cameraTransform.position;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            textureUnitSizeX = (sr.sprite.rect.width / sr.sprite.pixelsPerUnit) * Mathf.Abs(transform.localScale.x);
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        float currentCamX = cameraTransform.position.x;
        float prevCamX = lastCameraPosition.x;

        if (useMaxCameraX)
        {
            currentCamX = Mathf.Min(currentCamX, maxCameraX);
            prevCamX = Mathf.Min(prevCamX, maxCameraX);
        }

        float deltaX = currentCamX - prevCamX;
        float deltaY = cameraTransform.position.y - lastCameraPosition.y;

        transform.position += new Vector3(
            deltaX * parallaxEffectX,
            deltaY * parallaxEffectY,
            0f
        );

        lastCameraPosition = cameraTransform.position;

        if (infiniteHorizontal && textureUnitSizeX > 0f)
        {
            float distanceFromCamera = currentCamX - transform.position.x;
            if (Mathf.Abs(distanceFromCamera) >= textureUnitSizeX)
            {
                float offsetPositionX = distanceFromCamera % textureUnitSizeX;
                transform.position = new Vector3(currentCamX + offsetPositionX, transform.position.y, transform.position.z);
            }
        }
    }
}