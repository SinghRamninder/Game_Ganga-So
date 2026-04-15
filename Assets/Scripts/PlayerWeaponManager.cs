using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject crosshair;

    private GameObject currentGun;
    private Gun currentGunScript;

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

    public void EquipGun(GameObject gunPrefab)
    {
        if (currentGun == gunPrefab) return;

        if (currentGun != null)
        {
            Destroy(currentGun);
        }

        currentGun = Instantiate(gunPrefab, weaponHolder.transform);
        Vector3 gunSpawnPosition = currentGun.GetComponent<Gun>().spawnOffset;
        currentGun.transform.SetLocalPositionAndRotation(gunSpawnPosition, Quaternion.identity);

        currentGunScript = currentGun.GetComponent<Gun>();
    }
}
