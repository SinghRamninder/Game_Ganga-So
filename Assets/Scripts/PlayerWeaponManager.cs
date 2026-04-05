using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponHolder;

    private GameObject currentGun;
    private Gun currentGunScript;

    private void Update()
    {
        if (currentGunScript != null)
            currentGunScript.HandleFireInput();
    }

    public void EquipGun(GameObject gunPrefab)
    {
        if (currentGun == gunPrefab) return;

        if (currentGun != null)
        {
            Destroy(currentGun);
        }

        currentGun = Instantiate(gunPrefab, weaponHolder.transform);
        currentGun.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        currentGunScript = currentGun.GetComponent<Gun>();
    }
}
