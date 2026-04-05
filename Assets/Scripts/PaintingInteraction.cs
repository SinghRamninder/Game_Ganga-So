using UnityEngine;

public class PaintingInteraction : MonoBehaviour
{
    [SerializeField] private GameObject gun;

    private PlayerWeaponManager playerWeaponManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && playerWeaponManager != null)
        {
            playerWeaponManager.EquipGun(gun);
            Debug.Log("Gun Equiped");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerWeaponManager = other.GetComponent<PlayerWeaponManager>();
            Debug.Log("Left control to equip the gun");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerWeaponManager = null;
            Debug.Log("Outside collider");
        }
    }
}
