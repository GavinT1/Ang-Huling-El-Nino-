using UnityEngine;

public class PromptBob : MonoBehaviour
{
    public float floatDistance = 6f;
    public float speed = 4f;
    private RectTransform rect;
    private Vector2 startPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.unscaledTime * speed) * floatDistance;
        rect.anchoredPosition = startPos + new Vector2(0f, offsetY);
    }
}