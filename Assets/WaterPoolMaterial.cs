using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
public class WaterPoolMaterial : MonoBehaviour
{
    [Header("Pool Dimensions")]
    [SerializeField] private float width = 8f;
    [SerializeField] private float height = 2.5f;
    [Tooltip("Higher values (10-14) make raindrop dimples pinpoint and realistic")]
    [SerializeField] private int columnsPerUnit = 12;

    [Header("Rendering Order")]
    [SerializeField] private string sortingLayerName = "Player";
    [SerializeField] private int sortingOrder = 2;

    [Header("Spring Wave Physics")]
    [SerializeField] private float springStiffness = 0.02f;
    [SerializeField] private float damping = 0.07f;
    [SerializeField] private float waveSpread = 0.03f;

    [Header("Idle Surface Ripples")]
    [SerializeField] private float idleWaveAmplitude = 0.02f;
    [SerializeField] private float idleWaveFrequency = 2.2f;
    [SerializeField] private float idleWaveSpeed = 2.0f;

    [Header("Player Splash Interaction")]
    [SerializeField] private float entrySplashMultiplier = 0.012f;
    [SerializeField] private float swimRippleMultiplier = 0.01f;
    [SerializeField] private float maxSplashVelocity = 0.1f;
    [SerializeField] private int splashRadius = 6;
    [SerializeField] private GameObject splashEffectPrefab;
    [SerializeField] private float minEntrySpeedForParticles = 2.0f;

    [Header("Realistic Rain / Droplet Dimples")]
    [Tooltip("Keep very low (0.0003 - 0.0008) for realistic rain pitter-patter")]
    [SerializeField] private float dropletSplashMultiplier = 0.00045f;
    [SerializeField] private float maxDropletImpulse = 0.012f;
    [Tooltip("Optional: Destroy raindrops/droplets as soon as they hit the water surface")]
    [SerializeField] private bool destroyDropletsOnImpact = false;

    private Mesh mesh;
    private Vector3[] vertices;
    private float[] baseHeights;
    private float[] currentHeights;
    private float[] velocities;
    private int nodeCount;
    private BoxCollider2D triggerCollider;
    private float bottomWorldY;

    private void Awake()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        bottomWorldY = transform.position.y - (height * 0.5f);

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

    public void SetWaterHeight(float newHeight)
    {
        newHeight = Mathf.Max(0.05f, newHeight);
        float deltaH = newHeight - height;
        height = newHeight;

        Vector3 pos = transform.position;
        pos.y = bottomWorldY + (height * 0.5f);
        transform.position = pos;

        float halfH = height * 0.5f;
        for (int i = 0; i < nodeCount; i++)
        {
            baseHeights[i] = halfH;
            currentHeights[i] += deltaH * 0.5f;
            vertices[i * 2 + 1].y = -halfH;
        }

        if (triggerCollider != null)
        {
            triggerCollider.size = new Vector2(width, height);
        }
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

            vertices[i * 2] = new Vector3(x, halfH, 0f);
            uvs[i * 2] = new Vector2(t, 1f);

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
        for (int i = 0; i < nodeCount; i++)
        {
            float displacement = currentHeights[i] - baseHeights[i];
            float acceleration = -springStiffness * displacement - damping * velocities[i];
            velocities[i] += acceleration;
            currentHeights[i] += velocities[i];
        }

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

        for (int offset = -splashRadius; offset <= splashRadius; offset++)
        {
            int idx = centerIndex + offset;
            if (idx >= 0 && idx < nodeCount)
            {
                float t = (float)Mathf.Abs(offset) / (splashRadius + 1);
                float falloff = Mathf.Cos(t * Mathf.PI * 0.5f);
                velocities[idx] += clampedVel * falloff;
            }
        }
    }

    private void SplashSingleNode(float worldX, float splashVelocity)
    {
        float localX = transform.InverseTransformPoint(new Vector3(worldX, 0f, 0f)).x;
        float normalizedX = Mathf.InverseLerp(-width * 0.5f, width * 0.5f, localX);
        int idx = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (nodeCount - 1)), 0, nodeCount - 1);

        velocities[idx] += Mathf.Clamp(splashVelocity, -maxDropletImpulse, maxDropletImpulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        if (other.CompareTag("Player"))
        {
            Splash(other.transform.position.x, rb.linearVelocity.y * entrySplashMultiplier);

            if (Mathf.Abs(rb.linearVelocity.y) >= minEntrySpeedForParticles)
            {
                SpawnSplashParticles(other.transform.position.x);
            }
        }
        else
        {
            SplashSingleNode(other.transform.position.x, rb.linearVelocity.y * dropletSplashMultiplier);

            if (destroyDropletsOnImpact)
            {
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

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

    private void SpawnSplashParticles(float worldX)
    {
        if (splashEffectPrefab == null) return;

        float surfaceWorldY = transform.position.y + (height * 0.5f);
        Vector3 spawnPos = new Vector3(worldX, surfaceWorldY, 0f);
        Instantiate(splashEffectPrefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.1f));
    }
}