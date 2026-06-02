using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 8;
    private int currentHealth;

    [Header("UI Reference")]
    [Tooltip("Drag your HealthHUD parent object (the one with the HealthHUDController script) here")]
    [SerializeField] private HealthHUDController healthUI;

    [Header("Editor Test Keys")]
    [Tooltip("Press this key while playing in the Unity Editor to simulate taking damage")]
    [SerializeField] private KeyCode damageTestKey = KeyCode.T;

    [Tooltip("Press this key while playing in the Unity Editor to simulate healing")]
    [SerializeField] private KeyCode healTestKey = KeyCode.Y;

    private void Start()
    {
        // Initialize health at full value when the game starts
        currentHealth = maxHealth;
        
        // Push the initial full health status straight to your UI
        UpdateUI();
    }

    private void Update()
    {
        // --- EDITOR TESTING CONTROLS ---
        // Pressing 'T' will deal 1 damage
        if (Input.GetKeyDown(damageTestKey))
        {
            TakeDamage(1);
        }

        // Pressing 'Y' will restore 1 damage
        if (Input.GetKeyDown(healTestKey))
        {
            Heal(1);
        }
    }

    /// <summary>
    /// Call this from enemy scripts, hazards, or projectiles to hurt the player.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        
        // Clamp health so it never goes below zero
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateUI();

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Call this when picking up a health pack or painting item.
    /// </summary>
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        
        // Clamp health so it never exceeds your max capacity of 8 bars
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealthDisplay(currentHealth);
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died! Trigger Game Over screen here.");
        // Add your player death logic here (e.g., open a death panel, pause game, etc.)
    }
}