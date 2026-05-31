using UnityEngine;
using UnityEngine.UI; // Required for using the UI Image component

public class WeaponPortraitController : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponPortraitMapping
    {
        [Tooltip("The exact name of the gun prefab/GameObject (e.g., Dual Pistol).")]
        public string weaponName;

        [Tooltip("Drag the artwork/portrait Sprite asset from your Project folder here.")]
        public Sprite weaponPortraitSprite;
    }

    [Header("UI Reference")]
    [Tooltip("Drag the single portrait UI Image object from your Canvas Hierarchy here.")]
    [SerializeField] private Image targetUIImage;

    [Header("Default Settings")]
    [Tooltip("The portrait to display if no weapon is equipped (optional).")]
    [SerializeField] private Sprite defaultPortrait;

    [Header("Weapon Portrait Mappings")]
    [SerializeField] private WeaponPortraitMapping[] weaponPortraits;

    private Transform playerWeaponContainer;

    private void Update()
    {
        // Dynamically find and assign the container where PlayerWeaponManager stores guns
        if (playerWeaponContainer == null && PlayerWeaponManager.instance != null)
        {
            playerWeaponContainer = PlayerWeaponManager.instance.transform;
        }

        // If the container is found, scan it to see which weapon is currently active
        if (playerWeaponContainer != null)
        {
            UpdatePortrait();
        }
    }

    private void UpdatePortrait()
    {
        if (targetUIImage == null) return;

        string activeWeaponName = "";

        // Scan through the children of the player's weapon holder slot
        foreach (Transform child in playerWeaponContainer)
        {
            if (child.gameObject.activeSelf)
            {
                activeWeaponName = child.gameObject.name;
                break; // Stop looking once we find the active one
            }
        }

        // Search our list to see if we have a sprite mapped to this gun's name
        foreach (var mapping in weaponPortraits)
        {
            if (!string.IsNullOrEmpty(activeWeaponName) && activeWeaponName.Contains(mapping.weaponName))
            {
                if (mapping.weaponPortraitSprite != null)
                {
                    targetUIImage.sprite = mapping.weaponPortraitSprite;
                    return; // Portrait updated, exit function
                }
            }
        }

        // If no active weapon was found or it didn't match anything, fall back to default portrait
        targetUIImage.sprite = defaultPortrait;
    }
}