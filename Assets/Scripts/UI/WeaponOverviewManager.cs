using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WeaponOverviewManager : MonoBehaviour
{
    public static WeaponOverviewManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;

    [Header("Display Settings")]
    [Tooltip("How long the overview card stays on the screen before fading out.")]
    [SerializeField] private float displayDuration = 3.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("The sound clip played when the card appears on screen.")]
    [SerializeField] private AudioClip openSound;
    
    [Tooltip("The sound clip played right before the card disappears.")]
    [SerializeField] private AudioClip closeSound;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Automatically catch the AudioSource component if it's on the same GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (overviewPanel != null) overviewPanel.SetActive(false);
    }

    /// <summary>
    /// Call this method to dynamically load, display, and play audio for a weapon's overview.
    /// </summary>
    public bool ShowWeaponOverview(PlayerPortraitData data)
    {
        if (data == null || overviewPanel == null) return false;

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        if (weaponNameText != null) weaponNameText.text = data.weaponName;
        if (weaponDescriptionText != null) weaponDescriptionText.text = data.weaponDescription;
        
        if (cardBackgroundImage != null && data.weaponOverviewCardBG != null)
        {
            cardBackgroundImage.sprite = data.weaponOverviewCardBG;
        }

        // 🔊 Play the enter sound effect
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        overviewPanel.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
        return true;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        // 🔊 Play the exit sound effect right before turning off the panel
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        if (overviewPanel != null) overviewPanel.SetActive(false);
    }
}