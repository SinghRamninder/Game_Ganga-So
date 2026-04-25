using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject crosshair;

    private GameObject currentGun;
    private Weapon currentGunScript;

    private void Update()
    {
        if (currentGunScript != null)
        {
            //Temporary
            if (crosshair != null)
            {
                crosshair.SetActive(true);
            }
            //Temporary

            currentGunScript.HandleFireInput();
        }
    }

    public void EquipGun(GameObject weaponPrefab)
    {
        if (currentGun == weaponPrefab) return;

        if (currentGun != null)
        {
            Destroy(currentGun);
        }

        currentGun = Instantiate(weaponPrefab, weaponHolder.transform);
        Vector3 weaponSpawnPosition = currentGun.GetComponent<Weapon>().weaponSpawnPosition;
        Quaternion weaponSpawnRotation = currentGun.GetComponent<Weapon>().weaponSpawnRotation;
        Vector3 weaponSpawnScale = currentGun.GetComponent<Weapon>().weaponSpawnScale;
        currentGun.transform.SetLocalPositionAndRotation(weaponSpawnPosition, weaponSpawnRotation);
        currentGun.transform.localScale = weaponSpawnScale;

        currentGunScript = currentGun.GetComponent<Weapon>();
    }
}
