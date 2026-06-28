using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    public static PlayerWeaponManager instance;

    [HideInInspector] public List<GameObject> ownedPrefabs = new();
    [HideInInspector] public int currentIndex = 0;

    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject crosshair;

    public GameObject headCrosshair;
    public PlayerPortraitUI portraitUI;

    [HideInInspector] public List<GameObject> weaponsSpawned;

    private GameObject currentWeapon;
    [HideInInspector] public Weapon currentWeaponScript;
    private GameObject equipedWeapon;

    [HideInInspector] public int weaponLayerNumber = 0;

    // NEW: FinalShot will set this to true.
    // This prevents normal HandleFireInput() from also firing during the final shot.
    [HideInInspector] public bool blockNormalWeaponInput = false;

    private void Awake()
    {
        instance = this;

        if (weaponHolder == null)
        {
            WeaponsInventory inventory = GetComponentInChildren<WeaponsInventory>(true);
            if (inventory != null)
            {
                weaponHolder = inventory.gameObject;
            }
        }

        weaponsSpawned = new List<GameObject>();
        ownedPrefabs = new List<GameObject>();
    }

    private void Update()
    {
        if (blockNormalWeaponInput)
            return;

        if (currentWeaponScript != null)
        {
            if (crosshair != null)
            {
                crosshair.SetActive(true);
            }

            currentWeaponScript.HandleFireInput();
        }
    }

    public void EquipGun(GameObject weaponPrefab)
    {
        if (currentWeapon == weaponPrefab) return;

        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        currentWeapon = Instantiate(weaponPrefab, weaponHolder.transform);

        Weapon weapon = currentWeapon.GetComponent<Weapon>();

        Vector3 weaponSpawnPosition = weapon.weaponSpawnPosition;
        Quaternion weaponSpawnRotation = weapon.weaponSpawnRotation;
        Vector3 weaponSpawnScale = weapon.weaponSpawnScale;

        currentWeapon.transform.SetLocalPositionAndRotation(weaponSpawnPosition, weaponSpawnRotation);
        currentWeapon.transform.localScale = weaponSpawnScale;

        currentWeaponScript = weapon;

        Debug.Log($"[WeaponManager] EquipGun executed. Weapon script found: {currentWeaponScript != null} | Portrait UI connected: {portraitUI != null}");

        if (currentWeaponScript != null)
        {
            Debug.Log($"[WeaponManager] Weapon portrait data exists: {currentWeaponScript.weaponPortraitData != null}");
        }

        if (currentWeaponScript != null && portraitUI != null)
        {
            portraitUI.ChangeWeaponPortrait(currentWeaponScript.weaponPortraitData);
        }

        if (currentWeaponScript != null && HealthBarUI.instance != null)
        {
            HealthBarUI.instance.ChangeHealthBarVisuals(currentWeaponScript.weaponPortraitData);
        }
    }

    public void SpawnWeapon(GameObject weaponPrefab)
    {
        if (ownedPrefabs.Contains(weaponPrefab)) return;

        GameObject spawnedWeapon = Instantiate(weaponPrefab, weaponHolder.transform);

        Weapon w = spawnedWeapon.GetComponent<Weapon>();

        spawnedWeapon.transform.SetLocalPositionAndRotation(w.weaponSpawnPosition, w.weaponSpawnRotation);
        spawnedWeapon.transform.localScale = w.weaponSpawnScale;

        weaponsSpawned.Add(spawnedWeapon);
        ownedPrefabs.Add(weaponPrefab);

        ChangeWeapon(spawnedWeapon);
    }

    public void ChangeWeapon(GameObject weaponGameobject)
    {
        if (weaponGameobject == null) return;
        if (equipedWeapon == weaponGameobject) return;

        if (equipedWeapon != null)
            equipedWeapon.SetActive(false);

        weaponGameobject.SetActive(true);

        currentWeaponScript = weaponGameobject.GetComponent<Weapon>();
        equipedWeapon = weaponGameobject;

        Debug.Log($"[WeaponManager] ChangeWeapon executed for {weaponGameobject.name}. Weapon script: {currentWeaponScript != null} | Portrait UI connected: {portraitUI != null}");

        if (currentWeaponScript != null)
        {
            Debug.Log($"[WeaponManager] Weapon portrait data exists: {currentWeaponScript.weaponPortraitData != null}");
        }

        if (currentWeaponScript != null && portraitUI != null)
        {
            portraitUI.ChangeWeaponPortrait(currentWeaponScript.weaponPortraitData);
        }

        if (currentWeaponScript != null && HealthBarUI.instance != null)
        {
            HealthBarUI.instance.ChangeHealthBarVisuals(currentWeaponScript.weaponPortraitData);
        }

        currentIndex = weaponsSpawned.IndexOf(weaponGameobject);
    }

    public PlayerPortraitData GetCurrentWeaponPortraitData()
    {
        if (currentWeaponScript != null)
        {
            return currentWeaponScript.weaponPortraitData;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            if (WeaponLoadoutStore.Instance != null)
                WeaponLoadoutStore.Instance.Save(ownedPrefabs, currentIndex);

            instance = null;
        }
    }
}