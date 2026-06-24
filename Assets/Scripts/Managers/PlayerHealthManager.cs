using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class PlayerHealthManager : MonoBehaviour
{
    public static PlayerHealthManager instance;

    [Header("Health Settings")]
    public float baseMaxHealth = 100f;
    private float currentMaxHealth;
    [HideInInspector] public float currentHealth;
    
    [Header("UI")]
    public TextMeshProUGUI healthText;
    public List<GameObject> healthBars = new List<GameObject>();
    public Image bloodImage;

    [Header("FMOD")]
    public EventReference hitEventFmod;
    public EventReference deathEventFmod;

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
        if (!hitEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(hitEventFmod, gameObject);
        }

        currentHealth -= damage;

        if (playerHitEffect != null)
        {
            StopCoroutine(playerHitEffect);
        }

        playerHitEffect = StartCoroutine(DisplayHitEffect());

        UpdateUI();

        // Trigger face hit glitch effect for 0.3 seconds
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

        // Update base portrait state based on current health
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth((int)currentHealth, (int)currentMaxHealth);
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");

        if (!deathEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(deathEventFmod, gameObject);
        }

        // Activate death portrait (blue screen effect)
        if (PlayerWeaponManager.instance != null && PlayerWeaponManager.instance.portraitUI != null)
        {
            PlayerWeaponManager.instance.portraitUI.UpdateHealth(0, (int)currentMaxHealth);
        }

        // Open game over screen and pass current weapon data
        if (GameOverManager.instance != null && PlayerWeaponManager.instance != null)
        {
            PlayerPortraitData currentWeaponData = PlayerWeaponManager.instance.GetCurrentWeaponPortraitData();
            GameOverManager.instance.ShowGameOverScreen(currentWeaponData);
        }

        // ⚠️ Important note for your checkpoint system:
        // This line instantly teleports the player to the checkpoint upon death.
        // If you want the game to pause, show the game over screen, and let the player click Continue,
        // it's better to keep this line commented out for now so the menu works properly:
        /*
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SpawnAtCheckpoint();
        }
        */
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