using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [Header("Heart Displays")]
    public Image[] hearts; 
    public Sprite fullHeart;
    public Sprite emptyHeart; 

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
                hearts[i].enabled = true; 
            }
            else
            {
                if (emptyHeart != null)
                {
                    hearts[i].sprite = emptyHeart; 
                }
                else
                {
                    hearts[i].enabled = false; 
                }
            }
        }
    }
}