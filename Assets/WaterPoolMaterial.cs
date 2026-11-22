using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
public class WaterPoolMaterial : MonoBehaviour
{
    [Header("Pool Dimensions")]
    [SerializeField] private float width = 8f;
    [SerializeField] private float height = 2.5f;
    [SerializeField] private int columnsPerUnit = 6;

    [Header("Rendering Order")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 5;

    [Header("Spring Wave Physics")]
    [SerializeField] private float springStiffness = 0.025f;
    [SerializeField] private float damping = 0.035f;
    [SerializeField] private float waveSpread = 0.04f;

    [Header("Idle Surface Ripples")]
    [SerializeField] private float idleWaveAmplitude = 0.04f;
    [SerializeField] private float idleWaveFrequency = 1.8f;
    [SerializeField] private float idleWaveSpeed = 2.2f;

    [Header("Player Splash Interaction")]
    [SerializeField] private float entrySplashMultiplier = 0.06f;
    [SerializeField] private float swimRippleMultiplier = 0.015f;
    [SerializeField] private float maxSplashVelocity = 0.45f;

    private Mesh mesh;
    private Vector3[] vertices;
    private float[] baseHeights;
    private float[] currentHeights;
    private float[] velocities;
    private int nodeCount;
    private BoxCollider2D triggerCollider;

    private void Awake()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        SetupCollider();
        BuildWaterMesh();
    }

    private void SetupCollider()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(width, height);
        triggerCollider.offset = Vector2.zero;
    }

    private void BuildWaterMesh()
    {
        mesh = new Mesh();
        mesh.name = "InteractiveWaterMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        nodeCount = Mathf.Max(2, Mathf.RoundToInt(width * columnsPerUnit) + 1);
        vertices = new Vector3[nodeCount * 2];
        Vector2[] uvs = new Vector2[nodeCount * 2];
        int[] triangles = new int[(nodeCount - 1) * 6];

        baseHeights = new float[nodeCount];
        currentHeights = new float[nodeCount];
        velocities = new float[nodeCount];

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        for (int i = 0; i < nodeCount; i++)
        {
            float t = (float)i / (nodeCount - 1);
            float x = Mathf.Lerp(-halfW, halfW, t);

            baseHeights[i] = halfH;
            currentHeights[i] = halfH;

            // Top vertex (surface)
            vertices[i * 2] = new Vector3(x, halfH, 0f);
            uvs[i * 2] = new Vector2(t, 1f);

            // Bottom vertex (pool floor)
            vertices[i * 2 + 1] = new Vector3(x, -halfH, 0f);
            uvs[i * 2 + 1] = new Vector2(t, 0f);

            if (i < nodeCount - 1)
            {
                int tri = i * 6;
                int v = i * 2;

                triangles[tri] = v;
                triangles[tri + 1] = v + 2;
                triangles[tri + 2] = v + 1;

                triangles[tri + 3] = v + 1;
                triangles[tri + 4] = v + 2;
                triangles[tri + 5] = v + 3;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void FixedUpdate()
    {
        // 1. Update each surface spring node
        for (int i = 0; i < nodeCount; i++)
        {
            float displacement = currentHeights[i] - baseHeights[i];
            float acceleration = -springStiffness * displacement - damping * velocities[i];
            velocities[i] += acceleration;
            currentHeights[i] += velocities[i];
        }

        // 2. Propagate waves to neighboring nodes
        float[] leftDeltas = new float[nodeCount];
        float[] rightDeltas = new float[nodeCount];

        for (int pass = 0; pass < 4; pass++)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (i > 0)
                {
                    leftDeltas[i] = waveSpread * (currentHeights[i] - currentHeights[i - 1]);
                    velocities[i - 1] += leftDeltas[i];
                }
                if (i < nodeCount - 1)
                {
                    rightDeltas[i] = waveSpread * (currentHeights[i] - currentHeights[i + 1]);
                    velocities[i + 1] += rightDeltas[i];
                }
            }

            for (int i = 0; i < nodeCount; i++)
            {
                if (i > 0) currentHeights[i - 1] += leftDeltas[i];
                if (i < nodeCount - 1) currentHeights[i + 1] += rightDeltas[i];
            }
        }

        // 3. Apply spring wave + subtle idle shimmer to mesh top vertices
        for (int i = 0; i < nodeCount; i++)
        {
            float x = vertices[i * 2].x;
            float idleOffset = Mathf.Sin((x * idleWaveFrequency) + (Time.time * idleWaveSpeed)) * idleWaveAmplitude;
            vertices[i * 2].y = currentHeights[i] + idleOffset;
        }

        mesh.vertices = vertices;
    }

    public void Splash(float worldX, float splashVelocity)
    {
        float localX = transform.InverseTransformPoint(new Vector3(worldX, 0f, 0f)).x;
        float normalizedX = Mathf.InverseLerp(-width * 0.5f, width * 0.5f, localX);
        int centerIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (nodeCount - 1)), 0, nodeCount - 1);

        float clampedVel = Mathf.Clamp(splashVelocity, -maxSplashVelocity, maxSplashVelocity);

        for (int offset = -2; offset <= 2; offset++)
        {
            int idx = centerIndex + offset;
            if (idx >= 0 && idx < nodeCount)
            {
                float falloff = 1f - (Mathf.Abs(offset) / 3f);
                velocities[idx] += clampedVel * falloff;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            Splash(other.transform.position.x, rb.linearVelocity.y * entrySplashMultiplier);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.5f)
        {
            float surfaceWorldY = transform.position.y + (height * 0.5f);
            if (Mathf.Abs(other.bounds.min.y - surfaceWorldY) < 0.6f || other.bounds.max.y >= surfaceWorldY)
            {
                Splash(other.transform.position.x, -Mathf.Abs(rb.linearVelocity.x) * swimRippleMultiplier * Time.fixedDeltaTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.1f));
    }
}