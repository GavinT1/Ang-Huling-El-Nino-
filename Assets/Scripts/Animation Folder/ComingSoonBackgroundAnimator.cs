using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComingSoonBackgroundAnimator : MonoBehaviour
{
    [Header("Template Text (Drag your Text (TMP) here)")]
    public TMP_Text templateText;

    [Header("Grid Layout")]
    public string repeatedWord = "COMING SOON";
    public int rows = 6;
    public int columns = 5;
    public float horizontalSpacing = 460f;
    public float verticalSpacing = 175f;

    [Header("Motion (Runs while game is paused)")]
    public float scrollSpeed = 65f;
    public bool alternateRowDirection = true;

    [Header("Solid vs Outline Wave Effect")]
    public Color solidColor = Color.white;
    public Color outlineTint = new Color(1f, 1f, 1f, 0.28f);
    public float pulseSpeed = 2.2f;

    private class RowData
    {
        public RectTransform rowRect;
        public float direction;
        public float totalWidth;
        public List<TMP_Text> items = new List<TMP_Text>();
    }

    private List<RowData> spawnedRows = new List<RowData>();
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            BuildGrid();
        }
    }

    private void BuildGrid()
    {
        if (templateText == null)
        {
            templateText = GetComponentInChildren<TMP_Text>(true);
        }
        if (templateText == null) return;

        initialized = true;

        // Add RectMask2D so scrolling text stays neatly clipped inside the SETTINGS panel
        if (GetComponent<RectMask2D>() == null)
        {
            gameObject.AddComponent<RectMask2D>();
        }

        float totalHeight = (rows - 1) * verticalSpacing;
        float startY = totalHeight * 0.5f;

        for (int r = 0; r < rows; r++)
        {
            GameObject rowObj = new GameObject($"Row_{r}", typeof(RectTransform));
            rowObj.transform.SetParent(transform, false);
            // Keep background grid behind any foreground "Back" button
            rowObj.transform.SetSiblingIndex(0);

            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, startY - r * verticalSpacing);

            RowData data = new RowData();
            data.rowRect = rowRect;
            data.direction = (alternateRowDirection && r % 2 == 1) ? -1f : 1f;
            data.totalWidth = columns * horizontalSpacing;

            float startX = -data.totalWidth * 0.5f;

            for (int c = 0; c < columns; c++)
            {
                TMP_Text item = Instantiate(templateText, rowRect);
                item.gameObject.SetActive(true);
                item.text = repeatedWord;
                item.raycastTarget = false;

                RectTransform itemRect = item.GetComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = new Vector2(startX + c * horizontalSpacing, 0f);

                data.items.Add(item);
            }

            spawnedRows.Add(data);
        }

        // Hide the original single template text so only the animated grid shows
        templateText.gameObject.SetActive(false);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        for (int r = 0; r < spawnedRows.Count; r++)
        {
            RowData row = spawnedRows[r];
            float halfWidth = row.totalWidth * 0.5f;

            for (int c = 0; c < row.items.Count; c++)
            {
                TMP_Text txt = row.items[c];
                RectTransform rect = txt.rectTransform;

                // Continuous horizontal marquee scroll
                Vector2 pos = rect.anchoredPosition;
                pos.x += row.direction * scrollSpeed * dt;

                if (pos.x > halfWidth) pos.x -= row.totalWidth;
                else if (pos.x < -halfWidth) pos.x += row.totalWidth;

                rect.anchoredPosition = pos;

                // Diagonal wave alternating between bright solid text and dim/outline text
                float wave = Mathf.Sin((Time.unscaledTime * pulseSpeed) + (r * 0.85f) + (c * 0.65f));
                txt.color = wave > 0.15f ? solidColor : outlineTint;
            }
        }
    }
}