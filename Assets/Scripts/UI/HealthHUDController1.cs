using UnityEngine;
using UnityEngine.UI;

public class HealthHUDController : MonoBehaviour
{
    [Header("Health Sprite Assets")]
    [Tooltip("Drop your custom full red 'HPbar' PNG here")]
    [SerializeField] private Sprite fullBarSprite;

    [Tooltip("Drop your custom outline 'HPbar-empty' PNG here")]
    [SerializeField] private Sprite emptyBarSprite;

    [Header("UI Component Array")]
    [Tooltip("Drag all 8 of your HPBar Image objects here in order (1 to 8)")]
    [SerializeField] private Image[] hpBars;

    /// <summary>
    /// Updates the health UI by swapping full sprites to empty sprites based on current health.
    /// </summary>
    /// <param name="currentHealth">The player's current health value (expected 0 to 8)</param>
    public void UpdateHealthDisplay(int currentHealth)
    {
        // Safety check to ensure the array has been assigned in the Inspector
        if (hpBars == null || hpBars.Length == 0)
        {
            Debug.LogWarning("HP Bars array is empty on HealthHUDController! Please assign your UI images.", this);
            return;
        }

        // Loop through every single health bar slot on your UI
        for (int i = 0; i < hpBars.Length; i++)
        {
            // If the slot index is less than current health, it's an active health point
            if (i < currentHealth)
            {
                hpBars[i].sprite = fullBarSprite;
            }
            // Otherwise, the health point is lost. Swap it to your custom empty PNG outline!
            else
            {
                hpBars[i].sprite = emptyBarSprite;
            }
        }
    }
}