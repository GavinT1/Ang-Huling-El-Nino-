using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0f, 1f)] public float parallaxFactorX = 0.3f; // Horizontal parallax depth
    [Range(0f, 1f)] public float parallaxFactorY = 0.9f; // Vertical parallax depth

    private Vector3 startPos;
    private Vector3 startCameraPos;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            startPos = transform.position;
            startCameraPos = cameraTransform.position;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Calculate total distance camera has moved from start point
        Vector3 distance = cameraTransform.position - startCameraPos;

        // Directly anchor position to prevent Y-axis drift
        transform.position = new Vector3(
            startPos.x + (distance.x * parallaxFactorX),
            startPos.y + (distance.y * parallaxFactorY),
            startPos.z
        );
    }
}