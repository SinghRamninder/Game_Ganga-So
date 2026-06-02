using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealthManager : MonoBehaviour
{
    public static PlayerHealthManager instance;

    [Header("Health Settings")]
    public float baseMaxHealth = 100f;
    private float currentMaxHealth;
    private float currentHealth;
    
    [Header("UI")]
    public TextMeshProUGUI healthText;
    public List<GameObject> healthBars = new List<GameObject>();
    public Image bloodImage;

    private Coroutine playerHitEffect;

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

        if (playerHitEffect != null)
        {
            StopCoroutine(playerHitEffect);
        }

        playerHitEffect = StartCoroutine(DisplayHitEffect());

        UpdateUI();

        if (currentHealth <= 0)
        {
            if (bloodImage != null)
            {
                Color color = bloodImage.color;
                color.a = 0f;
                bloodImage.color = color;
            }

            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.Ceil(currentHealth)} / {Mathf.Ceil(currentMaxHealth)}";
        }

        int activeBars = Mathf.CeilToInt(currentHealth / 10f);
        for (int i = 0; i < healthBars.Count; i++)
        {
            if (healthBars[i] != null)
            {
                healthBars[i].SetActive(i < activeBars);
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Heal(float amount)
{
    currentHealth += amount;
    if (currentHealth > currentMaxHealth) currentHealth = currentMaxHealth;
    UpdateUI();
}

    private IEnumerator DisplayHitEffect()
    {
        if (bloodImage != null)
        {
            Color color = bloodImage.color;

            float startAlpha = 0.6f;
            color.a = startAlpha;
            bloodImage.color = color;

            float time = 0f;
            float fadeDuration = 5f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                color.a = Mathf.Lerp(color.a, 0f, time / fadeDuration);
                bloodImage.color = color;

                yield return null;
            }

            color.a = 0f;
            bloodImage.color = color;
        }
    }
}
