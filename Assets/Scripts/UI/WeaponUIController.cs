using UnityEngine;

public class WeaponUIController : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponIconMapping
    {
        [Tooltip("The exact name of the gun prefab/GameObject (e.g., Pistol).")]
        public string weaponName;

        [Tooltip("Drag the UI Image GameObject from your Canvas Hierarchy here.")]
        public GameObject iconUIObject;
    }

    [Header("Weapon Icon Settings")]
    [SerializeField] private WeaponIconMapping[] weaponIcons;

    private Transform playerWeaponContainer;

    private void Start()
    {
        // Hide all weapon icons dynamically when the game starts
        DeactivateAllIcons();
    }

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
            CheckActiveWeapon();
        }
    }

    private void CheckActiveWeapon()
    {
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

        // Loop through your inspector list mapping to turn the right UI element on or off
        foreach (var mapping in weaponIcons)
        {
            if (mapping.iconUIObject != null)
            {
                // Checks if the active weapon name matches your mapping (ignores "(Clone)" text)
                bool isThisWeaponActive = !string.IsNullOrEmpty(activeWeaponName) && activeWeaponName.Contains(mapping.weaponName);
                
                // Toggle the UI object's visibility
                mapping.iconUIObject.SetActive(isThisWeaponActive);
            }
        }
    }

    private void DeactivateAllIcons()
    {
        foreach (var mapping in weaponIcons)
        {
            if (mapping.iconUIObject != null)
            {
                mapping.iconUIObject.SetActive(false);
            }
        }
    }
}