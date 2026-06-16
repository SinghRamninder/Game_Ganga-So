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
        if (Input.GetKeyDown(damageTestKey))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(healTestKey))
        {
            Heal(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateUI();

        // ⭐ فعال کردن افکت گلیچِ ضربه خوردن چهره به مدت ۰.۳ ثانیه
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null && currentHealth > 0)
        {
            PlayerWeaponManager.instance.portraitUI.TriggerHitEffect(0.3f);
        }

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealthDisplay(currentHealth);
        }

        // ⭐ آپدیت کردن وضعیت پورتریت پایه بر اساس میزان خون فعلی پلیر
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died! Trigger Game Over screen here.");
        
        // ⭐ فعال کردن پورتریت مرگ (صفحه بلواسکرین)
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth(0, maxHealth);
        }
    }
}