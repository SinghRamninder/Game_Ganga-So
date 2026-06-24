using System.Collections;
using UnityEngine;
using FMODUnity;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponToEquip;
    [SerializeField] private float maxUpAnimPos = 2f;
    [SerializeField] private float animationSpeed = 1f;

    [Header("FMOD")]
    public EventReference pickupEventFmod;

    private Camera mainCamera;
    private float startYPos;

    private void Start()
    {
        mainCamera = Camera.main;
        startYPos = transform.position.y;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time * animationSpeed, 1f);
        float upPos = Mathf.Lerp(startYPos, startYPos + maxUpAnimPos, t);

        transform.position = new Vector3(transform.position.x, upPos, transform.position.z);

        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (PlayerWeaponManager.instance != null)
            {
            if (!pickupEventFmod.IsNull)
                {
                    RuntimeManager.PlayOneShotAttached(pickupEventFmod, gameObject);
                }
                Weapon weaponScript = weaponToEquip.GetComponent<Weapon>();
                if (weaponScript != null && weaponScript.weaponPortraitData != null)
                {
                    if (WeaponOverviewManager.instance != null)
                    {
                        WeaponOverviewManager.instance.ShowWeaponOverview(weaponScript.weaponPortraitData);
                    }
                }

                PlayerWeaponManager.instance.SpawnWeapon(weaponToEquip);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("Player Weapon Manager instance does not exists.");
            }
        }
    }
}