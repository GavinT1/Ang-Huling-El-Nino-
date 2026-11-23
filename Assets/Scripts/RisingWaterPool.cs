using UnityEngine;

[RequireComponent(typeof(WaterPoolMaterial))]
public class RisingWaterPool : MonoBehaviour
{
    [Header("Fill Levels (Height in World Units)")]
    [SerializeField] private float startHeight = 0.2f;
    [SerializeField] private float maxHeight = 6.0f;

    [Header("Fill Speed")]
    [SerializeField] private float fillSpeed = 0.4f;
    [SerializeField] private bool startFillingOnAwake = true;
    [SerializeField] private bool loopDrainAndFill = false;
    [SerializeField] private float pauseAtTopOrBottom = 1.5f;

    private WaterPoolMaterial waterPool;
    private float currentHeight;
    private bool isFilling;
    private bool goingUp = true;
    private float pauseTimer;

    private void Awake()
    {
        waterPool = GetComponent<WaterPoolMaterial>();
        currentHeight = startHeight;
        isFilling = startFillingOnAwake;
    }

    private void Start()
    {
        waterPool.SetWaterHeight(currentHeight);
    }

    private void FixedUpdate()
    {
        if (!isFilling) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        if (goingUp)
        {
            currentHeight += fillSpeed * Time.fixedDeltaTime;
            if (currentHeight >= maxHeight)
            {
                currentHeight = maxHeight;
                if (loopDrainAndFill)
                {
                    goingUp = false;
                    pauseTimer = pauseAtTopOrBottom;
                }
                else
                {
                    isFilling = false;
                }
            }
        }
        else
        {
            currentHeight -= fillSpeed * Time.fixedDeltaTime;
            if (currentHeight <= startHeight)
            {
                currentHeight = startHeight;
                goingUp = true;
                pauseTimer = pauseAtTopOrBottom;
            }
        }

        waterPool.SetWaterHeight(currentHeight);
    }

    public void StartFilling() => isFilling = true;
    public void StopFilling() => isFilling = false;

    public void ResetWater()
    {
        currentHeight = startHeight;
        goingUp = true;
        waterPool.SetWaterHeight(currentHeight);
    }
}