using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class WeaponOverviewManager : MonoBehaviour
{
    public static WeaponOverviewManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private RectTransform cardRectTransform;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;

    [Header("FX Settings")]
    [SerializeField] private GameObject vfxEffectObject;
    [SerializeField] private float fxPopSpeed = 8f;

    [Header("Display Settings")]
    [SerializeField] private float clickDelayDuration = 1.0f; // Safety delay before double-click is allowed

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool canClose = false;
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

        if (weaponNameText != null) weaponNameText.text = data.weaponName;
        if (weaponDescriptionText != null) weaponDescriptionText.text = data.weaponDescription;
        if (cardBackgroundImage != null && data.weaponOverviewCardBG != null)
        {
            cardBackgroundImage.sprite = data.weaponOverviewCardBG;
        }

        overviewPanel.SetActive(true);
        if (vfxEffectObject != null) vfxEffectObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        canClose = false;
        UIAnimCoroutine = StartCoroutine(AnimateOpenAndEnableClick());
        return true;
    }

    private IEnumerator AnimateOpenAndEnableClick()
    {
        if (cardRectTransform != null)
        {
            cardRectTransform.localScale = Vector3.zero;
            while (cardRectTransform.localScale.x < 0.99f)
            {
                cardRectTransform.localScale = Vector3.Lerp(cardRectTransform.localScale, Vector3.one, Time.unscaledDeltaTime * fxPopSpeed);
                yield return null;
            }
            cardRectTransform.localScale = Vector3.one;
        }

        yield return new WaitForSecondsRealtime(clickDelayDuration);
        canClose = true;
    }

    // Called automatically via Event Trigger on the CardBackground
    public void OnCardClick(BaseEventData baseEventData)
    {
        if (!canClose) return;

        PointerEventData pointerEventData = baseEventData as PointerEventData;
        if (pointerEventData != null && pointerEventData.clickCount == 2)
        {
            CloseWeaponOverview();
        }
    }

    public void CloseWeaponOverview()
    {
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        if (vfxEffectObject != null) vfxEffectObject.SetActive(false);
        if (overviewPanel != null) overviewPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}