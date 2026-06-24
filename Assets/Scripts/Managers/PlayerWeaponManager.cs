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
    private Weapon currentWeaponScript;
    private GameObject equipedWeapon;

    [HideInInspector] public int weaponLayerNumber = 0;

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
        Vector3 weaponSpawnPosition = currentWeapon.GetComponent<Weapon>().weaponSpawnPosition;
        Quaternion weaponSpawnRotation = currentWeapon.GetComponent<Weapon>().weaponSpawnRotation;
        Vector3 weaponSpawnScale = currentWeapon.GetComponent<Weapon>().weaponSpawnScale;
        currentWeapon.transform.SetLocalPositionAndRotation(weaponSpawnPosition, weaponSpawnRotation);
        currentWeapon.transform.localScale = weaponSpawnScale;

        currentWeaponScript = currentWeapon.GetComponent<Weapon>();

        // 🔎 ردیاب متد EquipGun
        Debug.Log($"[WeaponManager] متد EquipGun اجرا شد. اسکریپت اسلحه پیدا شد: {currentWeaponScript != null} | پرتره UI وصل است: {portraitUI != null}");
        if (currentWeaponScript != null)
        {
            Debug.Log($"[WeaponManager] دیتای عکس روی این اسلحه وجود دارد: {currentWeaponScript.weaponPortraitData != null}");
        }

        // تغییر پرتره
        if (currentWeaponScript != null && portraitUI != null)
        {
            portraitUI.ChangeWeaponPortrait(currentWeaponScript.weaponPortraitData);
        }

        // 🟢 خط جدید: تغییر خودکار اچ‌پ‌بار مچ شده با تفنگ
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
        ownedPrefabs.Add(weaponPrefab);          // keep this list parallel to weaponsSpawned

        // if (weaponsSpawned.Count == 1)
        //     ChangeWeapon(spawnedWeapon);         // first weapon becomes active
        // else
        //     spawnedWeapon.SetActive(false);

        ChangeWeapon(spawnedWeapon);
    }

    public void ChangeWeapon(GameObject weaponGameobject)
    {
        if (weaponGameobject == null) return;
        if (equipedWeapon == weaponGameobject) return;
        
        if (equipedWeapon != null) equipedWeapon.SetActive(false);

        weaponGameobject.SetActive(true);
        currentWeaponScript = weaponGameobject.GetComponent<Weapon>();
        equipedWeapon = weaponGameobject;

        // 🔎 ردیاب متد ChangeWeapon
        Debug.Log($"[WeaponManager] متد ChangeWeapon برای اسلحه {weaponGameobject.name} اجرا شد. اسکریپت اسلحه: {currentWeaponScript != null} | پرتره UI وصل است: {portraitUI != null}");
        if (currentWeaponScript != null)
        {
            Debug.Log($"[WeaponManager] دیتای عکس روی این اسلحه وجود دارد: {currentWeaponScript.weaponPortraitData != null}");
        }

        // تغییر پرتره
        if (currentWeaponScript != null && portraitUI != null)
        {
            portraitUI.ChangeWeaponPortrait(currentWeaponScript.weaponPortraitData);
        }

        // 🟢 خط جدید: تغییر خودکار اچ‌پ‌بار مچ شده با تفنگ
        if (currentWeaponScript != null && HealthBarUI.instance != null)
        {
            HealthBarUI.instance.ChangeHealthBarVisuals(currentWeaponScript.weaponPortraitData);
        }

        currentIndex = weaponsSpawned.IndexOf(weaponGameobject);
    }

    // 🔵 خطوط جدید: تابع رادار برای دادن دیتای تفنگ فعلی به اسکریپت مرگ
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