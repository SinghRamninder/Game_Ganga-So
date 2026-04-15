using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealthManager : MonoBehaviour
{
    public static PlayerHealthManager instance;

    [Header("Health Settings")]
    public float baseMaxHealth = 100f;
    private float currentMaxHealth;
    private float currentHealth;
    
    [Header("UI")]
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentMaxHealth = baseMaxHealth;
        currentHealth = currentMaxHealth;
        UpdateUI();
    }

    public void AddHealthBonus(float amount)
    {
        currentMaxHealth += amount;
        currentHealth += amount;
        if (currentHealth > currentMaxHealth) currentHealth = currentMaxHealth;
        UpdateUI();
    }

    public void RemoveHealthBonus(float amount)
    {
        currentMaxHealth -= amount;
        currentHealth -= amount;
        if (currentHealth > currentMaxHealth) currentHealth = currentMaxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.Ceil(currentHealth)} / {Mathf.Ceil(currentMaxHealth)}";
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
