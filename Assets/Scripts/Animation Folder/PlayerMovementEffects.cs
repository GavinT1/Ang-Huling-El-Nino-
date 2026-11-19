using System.Collections;
using UnityEngine;

public class PlayerMovementEffects : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;

    [Header("1. Walk & Run Dust Kick")]
    public Color dustColor = new Color(0.85f, 0.80f, 0.72f, 0.75f);
    public Vector2 dustSpawnOffset = new Vector2(-0.28f, 0.14f);
    public float walkStepInterval = 0.34f;
    public float runStepInterval = 0.20f;
    public float minMoveSpeed = 0.5f;
    public float sprintSpeedThreshold = 6.5f;

    [Header("2. Sprint Dash Streaks & Trail")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public Color dashStreakColor = new Color(0.95f, 0.95f, 1f, 0.55f);
    public float streakSpawnInterval = 0.06f;
    public bool enableSprintAfterimage = true;
    public float afterimageInterval = 0.08f;

    private Rigidbody2D rb;
    private SpriteRenderer playerSR;
    private Transform groundCheck;
    private Sprite pixelSquareSprite;

    private float stepTimer;
    private float streakTimer;
    private float afterimageTimer;
    private bool wasGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSR = GetComponent<SpriteRenderer>();
        groundCheck = transform.Find("GroundCheck");
        pixelSquareSprite = CreatePixelSquareSprite();
    }

    void Update()
    {
        if (rb == null || playerSR == null) return;

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        bool isMoving = horizontalSpeed > minMoveSpeed;
        bool isGrounded = CheckGrounded();

        bool isSprinting = isMoving && (Input.GetKey(sprintKey) || horizontalSpeed >= sprintSpeedThreshold);

        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.1f)
        {
            SpawnFootstepDust(isSprinting);
            stepTimer = 0f;
        }
        wasGrounded = isGrounded;

        if (isGrounded && isMoving)
        {
            stepTimer += Time.deltaTime;
            float currentInterval = isSprinting ? runStepInterval : walkStepInterval;

            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f;
                SpawnFootstepDust(isSprinting);
            }
        }
        else
        {
            stepTimer = walkStepInterval * 0.8f;
        }

        if (isSprinting)
        {
            streakTimer += Time.deltaTime;
            if (streakTimer >= streakSpawnInterval)
            {
                streakTimer = 0f;
                SpawnDashStreak();
            }

            if (enableSprintAfterimage)
            {
                afterimageTimer += Time.deltaTime;
                if (afterimageTimer >= afterimageInterval)
                {
                    afterimageTimer = 0f;
                    SpawnAfterimageGhost();
                }
            }
        }
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        if (groundLayer.value == 0)
            return Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void SpawnFootstepDust(bool isSprinting)
    {
        Vector3 basePos = groundCheck != null ? groundCheck.position : transform.position;
        float facingDir = playerSR.flipX ? -1f : 1f;
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            facingDir = Mathf.Sign(rb.linearVelocity.x);

        Vector3 heelAnchor = basePos + new Vector3(
            dustSpawnOffset.x * facingDir,
            dustSpawnOffset.y,
            0f
        );

        int puffCount = 3;
        for (int i = 0; i < puffCount; i++)
        {
            Vector3 randomJitter = new Vector3(
                Random.Range(-0.14f, 0.08f) * facingDir,
                Random.Range(0f, 0.10f),
                0f
            );

            float size = Random.Range(0.08f, 0.16f) * (isSprinting ? 1.2f : 1f);
            Vector2 driftVelocity = new Vector2(
                -facingDir * Random.Range(0.6f, 1.4f),
                Random.Range(0.45f, 0.85f)
            );

            StartCoroutine(AnimatePixelParticle(
                heelAnchor + randomJitter,
                new Vector3(size, size, 1f),
                driftVelocity,
                dustColor,
                0.45f,
                shrinkOverTime: true
            ));
        }
    }

    private void SpawnDashStreak()
    {
        float facingDir = Mathf.Abs(rb.linearVelocity.x) > 0.1f
            ? Mathf.Sign(rb.linearVelocity.x)
            : (playerSR.flipX ? -1f : 1f);

        Vector3 spawnPos = transform.position + new Vector3(
            -facingDir * Random.Range(0.1f, 0.5f),
            Random.Range(-0.45f, 0.45f),
            0f
        );

        float streakLength = Random.Range(0.45f, 0.95f);
        float streakThickness = 0.045f;
        Vector2 streakVel = new Vector2(-facingDir * Random.Range(2.2f, 3.8f), 0f);

        StartCoroutine(AnimatePixelParticle(
            spawnPos,
            new Vector3(streakLength, streakThickness, 1f),
            streakVel,
            dashStreakColor,
            0.22f,
            shrinkOverTime: false
        ));
    }

    private void SpawnAfterimageGhost()
    {
        GameObject ghost = new GameObject("SprintGhost");
        ghost.transform.position = transform.position;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = playerSR.sprite;
        sr.flipX = playerSR.flipX;
        sr.sharedMaterial = playerSR.sharedMaterial;
        sr.sortingLayerID = playerSR.sortingLayerID;
        sr.sortingOrder = playerSR.sortingOrder - 1;

        Color startColor = new Color(0.75f, 0.9f, 1f, 0.35f);
        sr.color = startColor;

        StartCoroutine(FadeAndDestroy(ghost, sr, startColor, 0.18f));
    }

    private IEnumerator AnimatePixelParticle(
        Vector3 worldPos,
        Vector3 startScale,
        Vector2 velocity,
        Color startColor,
        float duration,
        bool shrinkOverTime)
    {
        GameObject obj = new GameObject("DustPuff");
        obj.transform.position = worldPos;
        obj.transform.localScale = startScale;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = pixelSquareSprite;
        sr.sharedMaterial = playerSR.sharedMaterial;
        sr.sortingLayerID = playerSR.sortingLayerID;
        sr.sortingOrder = playerSR.sortingOrder + 1;
        sr.color = startColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            obj.transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity *= Mathf.Pow(0.25f, Time.deltaTime);

            if (shrinkOverTime)
            {
                obj.transform.localScale = Vector3.Lerp(startScale, startScale * 0.4f, t);
            }

            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = c;

            yield return null;
        }

        Destroy(obj);
    }

    private IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr, Color startColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = c;

            yield return null;
        }
        Destroy(obj);
    }

    private Sprite CreatePixelSquareSprite()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}