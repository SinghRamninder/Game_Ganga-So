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

        // فعال کردن افکت گلیچِ ضربه خوردن چهره به مدت ۰.۳ ثانیه
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null && currentHealth > 0)
        {
            PlayerWeaponManager.instance.portraitUI.TriggerHitEffect(0.3f);
        }

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

        // آپدیت کردن وضعیت پورتریت پایه بر اساس میزان خون فعلی
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth((int)currentHealth, (int)currentMaxHealth);
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        
        // فعال کردن پورتریت مرگ (صفحه بلواسکرین)
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth(0, (int)currentMaxHealth);
        }

        // ایجاد تاخیر کوتاه برای نمایش صفحه مرگ و سپس ریست لول
        StartCoroutine(ResetLevelAfterDelay(0.6f));
    }

    private IEnumerator ResetLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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