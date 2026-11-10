using UnityEngine;

public class ButtonInteractable : MonoBehaviour
{
    [Header("components")]
    public SpriteRenderer spriteRenderer;

    [Header("sprites")]
    public Sprite buttonUnpressedSprite;
    public Sprite buttonPressedSprite;

    [Header("settings")]
    public bool isActivated = false;
    public KeyCode interactKey = KeyCode.E;
    public float pressedYOffset = 0.05f;

    private bool isPlayerInRange = false;
    private Vector3 unpressedLocalPos;

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        unpressedLocalPos = transform.localScale;
        UpdateSprite();
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            PressButton();
        }
    }


    public void PressButton()
    {
        isActivated = !isActivated;
        UpdateSprite();

        if (isActivated)
        {
            Debug.Log("Button pressed!");
        }
        else
        {
            Debug.Log("Button released!");
        }
    }

    private void UpdateSprite()
    {
        if(spriteRenderer == null) return;

        if(buttonPressedSprite != null || buttonUnpressedSprite != null)
        {
            spriteRenderer.sprite = isActivated ? buttonPressedSprite : buttonUnpressedSprite;
        }
        else
        {
            if(isActivated)
            {
                transform.localScale = unpressedLocalPos + new Vector3(0, pressedYOffset, 0);
            }
            else
            {
                transform.localScale = unpressedLocalPos;
            }
    }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) isPlayerInRange = false;
    }

}
