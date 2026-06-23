using UnityEngine;
using FMODUnity;

public enum PickableType
{
    Weapon,
    KeyCard,
    HealthPack
}

public class Pickables : MonoBehaviour
{
    [HideInInspector] public PickableType Type = PickableType.Weapon;
    [HideInInspector] public GameObject weaponToEquip;
    [HideInInspector] public int healAmount = 25;

    [Header("Animation settings")]
    [SerializeField] private float maxUpAnimPos = 0.5f;
    [SerializeField] private float animationSpeed = 0.5f;

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
            switch (Type)
            {
                case PickableType.Weapon:
                    if (PlayerWeaponManager.instance != null && weaponToEquip != null)
                    {
                        PlayerWeaponManager.instance.SpawnWeapon(weaponToEquip);
                    }
                    break;

                case PickableType.KeyCard:
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.keyCards.Add(gameObject.name);
                        Debug.Log("Key added into inventory: " +  gameObject.name);
                    }

                    if (!pickupEventFmod.IsNull)
                    {
                        RuntimeManager.PlayOneShotAttached(pickupEventFmod, gameObject);
                    }
                    break;

                case PickableType.HealthPack:
                    if (PlayerHealthManager.instance != null)
                    {
                        PlayerHealthManager.instance.Heal(healAmount);
                    }

                    if (!pickupEventFmod.IsNull)
                    {
                        RuntimeManager.PlayOneShotAttached(pickupEventFmod, gameObject);
                    }
                    break;

                default:
                    break;
            }

            Destroy(gameObject);
        }
    }
}
