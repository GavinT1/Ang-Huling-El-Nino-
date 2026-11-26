using UnityEngine;

public class HorizontalCameraFollow : MonoBehaviour
{
    public Transform target;       // Drag PLAYER here
    public float smoothSpeed = 8f; // Higher = snappier follow

    [Header("Optional House Left/Right Limits")]
    public bool clampX = false;
    public float minX = -5f;
    public float maxX = 5f;

    private float lockedY;
    private float lockedZ;

    void OnEnable()
    {
        // Lock the exact Y height and Z depth the camera has when item-hunt starts
        lockedY = transform.position.y;
        lockedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x;
        if (clampX)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }

        float newX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, lockedY, lockedZ);
    }
}