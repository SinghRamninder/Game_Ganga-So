using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPortraitUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    
    private PlayerPortraitData currentPortraitData; 
    private int currentHealth = 100;
    private int maxHealth = 100;
    
    private bool isDead = false;
    private bool isHitActive = false;
    private bool isBlinking = false;

    private void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    // این تابع توسط PlayerWeaponManager زمان سوئیچ اسلحه صدا زده می‌شود
    public void ChangeWeaponPortrait(PlayerPortraitData newWeaponData)
    {
        if (newWeaponData == null) return;
        currentPortraitData = newWeaponData;
        UpdateBasePortrait(); 
    }

    // این تابع توسط سیستم سلامتی بازی صدا زده می‌شود
    public void UpdateHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;

        if (currentHealth <= 0)
        {
            isDead = true;
            if (currentPortraitData != null) portraitImage.sprite = currentPortraitData.dead;
            StopAllCoroutines();
            return;
        }

        if (!isHitActive && !isBlinking) UpdateBasePortrait();
    }

    // این تابع توسط سیستم سلامتی زمان آسیب دیدن صدا زده می‌شود
    public void TriggerHitEffect(float hitDuration = 0.25f)
    {
        if (isDead || currentPortraitData == null) return;
        StopCoroutine(nameof(HitPortraitRoutine));
        StartCoroutine(HitPortraitRoutine(hitDuration));
    }

    private IEnumerator HitPortraitRoutine(float duration)
    {
        isHitActive = true;
        float healthPercentage = (float)currentHealth / maxHealth;

        if (healthPercentage <= 0.3f) portraitImage.sprite = currentPortraitData.lowHit;
        else if (healthPercentage <= 0.7f) portraitImage.sprite = currentPortraitData.midHit;
        else portraitImage.sprite = currentPortraitData.fullHit;

        yield return new WaitForSeconds(duration);

        isHitActive = false;
        UpdateBasePortrait();
    }

    // ⭐ این تابع توسط اسکریپت Weapon.cs در زمان ریلود گان صدا زده می‌شود
    public void TriggerReloadPortrait(float duration)
    {
        if (isDead || currentPortraitData == null) return;
        
        // متوقف کردن کوروتین‌های دیگر برای جلوگیری از پرش تصویر
        StopCoroutine(nameof(ReloadPortraitRoutine));
        StartCoroutine(ReloadPortraitRoutine(duration));
    }

    private IEnumerator ReloadPortraitRoutine(float duration)
    {
        isHitActive = true; // فعال کردن فلگ برای اینکه پلک‌زدن موقتاً متوقف شود
        float healthPercentage = (float)currentHealth / maxHealth;

        // چون تصویر مستقلی برای ریلود در آرت‌ها نیست، از حالت چشم‌بسته/گلیچ بر اساس میزان خون استفاده می‌کنیم
        if (healthPercentage <= 0.3f) portraitImage.sprite = currentPortraitData.lowBlink;
        else if (healthPercentage <= 0.7f) portraitImage.sprite = currentPortraitData.midBlink;
        else portraitImage.sprite = currentPortraitData.fullBlink;

        yield return new WaitForSeconds(duration);

        isHitActive = false;
        UpdateBasePortrait();
    }

    private IEnumerator BlinkRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(3f, 7f));

            if (!isHitActive && !isDead && currentPortraitData != null)
            {
                isBlinking = true;
                float healthPercentage = (float)currentHealth / maxHealth;

                if (healthPercentage <= 0.3f) portraitImage.sprite = currentPortraitData.lowBlink;
                else if (healthPercentage <= 0.7f) portraitImage.sprite = currentPortraitData.midBlink;
                else portraitImage.sprite = currentPortraitData.fullBlink;

                yield return new WaitForSeconds(0.15f);

                isBlinking = false;
                if (!isHitActive) UpdateBasePortrait();
            }
        }
    }

    private void UpdateBasePortrait()
    {
        if (isDead || isHitActive || currentPortraitData == null) return;

        float healthPercentage = (float)currentHealth / maxHealth;

        if (healthPercentage <= 0.3f) portraitImage.sprite = currentPortraitData.lowHealth;
        else if (healthPercentage <= 0.7f) portraitImage.sprite = currentPortraitData.midHealth;
        else portraitImage.sprite = currentPortraitData.fullHealth;
    }
}