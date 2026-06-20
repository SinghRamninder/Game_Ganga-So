using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WeaponOverviewManager : MonoBehaviour
{
    public static WeaponOverviewManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private RectTransform cardRectTransform; // 🆕 رفرنس کادر کارت برای تغییر سایز کدی
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private Button collectedButton;

    [Header("FX Settings")]
    [SerializeField] private GameObject vfxEffectObject; // گیم‌اوبجکت افکت پارتیکل شما
    [SerializeField] private float fxPopSpeed = 8f; // سرعت باز شدن کارت

    [Header("Display Settings")]
    [SerializeField] private float buttonDelayDuration = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Coroutine UIAnimCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (overviewPanel != null) overviewPanel.SetActive(false);
        if (vfxEffectObject != null) vfxEffectObject.SetActive(false);
    }

    public bool ShowWeaponOverview(PlayerPortraitData data)
    {
        if (data == null || overviewPanel == null) return false;

        if (UIAnimCoroutine != null) StopCoroutine(UIAnimCoroutine);

        // تزریق اطلاعات متنی و تصویری فیگما
        if (weaponNameText != null) weaponNameText.text = data.weaponName;
        if (weaponDescriptionText != null) weaponDescriptionText.text = data.weaponDescription;
        if (cardBackgroundImage != null && data.weaponOverviewCardBG != null)
        {
            cardBackgroundImage.sprite = data.weaponOverviewCardBG;
        }

        // فعال‌سازی اوبجکت‌ها
        overviewPanel.SetActive(true);
        if (vfxEffectObject != null) vfxEffectObject.SetActive(true);
        if (collectedButton != null) collectedButton.gameObject.SetActive(false);

        // 🛑 متوقف کردن زمان بازی و آزاد کردن ماوس
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 🔊 پخش صدای ورود
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // ⚡ شروع افکت کدیِ ورود و باز شدن دکمه
        UIAnimCoroutine = StartCoroutine(AnimateOpenAndShowButton());
        return true;
    }

    // 🎬 افکت پاپ‌آپ کاملاً کدی بدون نیاز به انیماتور
    private IEnumerator AnimateOpenAndShowButton()
    {
        if (cardRectTransform != null)
        {
            // ابتدا سایز کارت را صفر می‌کنیم
            cardRectTransform.localScale = Vector3.zero;

            // کارت را به آرامی بزرگ می‌کنیم (با استفاده از unscaledDeltaTime چون زمان متوقف است)
            while (cardRectTransform.localScale.x < 0.99f)
            {
                cardRectTransform.localScale = Vector3.Lerp(cardRectTransform.localScale, Vector3.one, Time.unscaledDeltaTime * fxPopSpeed);
                yield return null;
            }
            cardRectTransform.localScale = Vector3.one;
        }

        // حالا منتظر می‌مانیم تا دکمه ظاهر شود
        yield return new WaitForSecondsRealtime(buttonDelayDuration);
        
        if (collectedButton != null)
        {
            collectedButton.gameObject.SetActive(true);
        }
    }

    public void CloseWeaponOverview()
    {
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // غیرفعال‌سازی افکت‌ها و برگشت به بازی
        if (vfxEffectObject != null) vfxEffectObject.SetActive(false);
        if (overviewPanel != null) overviewPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}