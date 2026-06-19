using UnityEngine;
using System.Collections.Generic;

public class PlayerWeaponManager : MonoBehaviour
{
    public static PlayerWeaponManager instance;

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
        if (instance == null)
        {
            instance = this;
        }
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
        GameObject spawnedWeapon = Instantiate(weaponPrefab, weaponHolder.transform);
        Vector3 weaponSpawnPosition = spawnedWeapon.GetComponent<Weapon>().weaponSpawnPosition;
        Quaternion weaponSpawnRotation = spawnedWeapon.GetComponent<Weapon>().weaponSpawnRotation;
        Vector3 weaponSpawnScale = spawnedWeapon.GetComponent<Weapon>().weaponSpawnScale;
        spawnedWeapon.transform.SetLocalPositionAndRotation(weaponSpawnPosition, weaponSpawnRotation);
        spawnedWeapon.transform.localScale = weaponSpawnScale;

        if (weaponsSpawned.Count == 0)
        {
            ChangeWeapon(spawnedWeapon);
        }
        else
        {
            spawnedWeapon.SetActive(false);
        }

        weaponsSpawned.Add(spawnedWeapon);
    }

    public void ChangeWeapon(GameObject weaponGameobject)
    {
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
}