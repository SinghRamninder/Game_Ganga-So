using UnityEngine;
using System.Collections;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

public class Gun : MonoBehaviour
{
    [SerializeField] private FireMode fireMode;
    [SerializeField] private float fireRate;
    [SerializeField] private float range;
    [SerializeField] private float cameraShakeIntensity;
    [SerializeField] private float cameraShakeDuration;
    [SerializeField] private float recoilForce = 2f;
    [SerializeField, Range(0f, 1f)] private float rumbleLeftFrequency; 
    [SerializeField, Range(0f, 1f)] private float rumbleRightFrequency; 
    [SerializeField] private float rumbleTime; 

    [Header("Gun Passives")]
    public float healthModifier = 0f;
    public float speedModifier = 1f;

    [Header("Damage & AoE")]
    public float damageToEnemy = 10f;
    public int raysPerShot = 1;
    public float aoeSpread = 0f;

    [Header("Ammo & Reload")]
    public bool isMeleeWeapon = false;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;
    public TMPro.TextMeshProUGUI ammoText;

    [Header("Effects")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    public Animator gunAnimator;

    [Header("Impact Effects")]
    public GameObject impactPrefab;
    public ParticleSystem impactEffectNonEnemy;
    public ParticleSystem impactEffectEnemy;
    public bool useSameImpactEffect = false;
    
    private float nextTimeToFire = 0f;
    private int currentAmmo;
    private bool isReloading = false;

    private void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        isReloading = false;
        if (PlayerHealthManager.instance != null && !isMeleeWeapon)
        {
            PlayerHealthManager.instance.AddHealthBonus(healthModifier);
        }
        var pc = GetComponentInParent<SimplePlayerController>();
        if (pc != null) pc.SetSpeedModifier(speedModifier);

        UpdateAmmoUI();
    }

    private void OnDisable()
    {
        if (PlayerHealthManager.instance != null && !isMeleeWeapon)
        {
            PlayerHealthManager.instance.RemoveHealthBonus(healthModifier);
        }
        var pc = GetComponentInParent<SimplePlayerController>();
        if (pc != null) pc.SetSpeedModifier(1f);
    }

    private void Update()
    {
        if (!isMeleeWeapon && !isReloading && currentAmmo < maxAmmo)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                StartCoroutine(Reload());
            }
        }
    }

    public void HandleFireInput()
    {
        if (fireMode == FireMode.SemiAuto)
        {
            if (InputManager.instance.controls.Gun.Fire.WasPressedThisFrame())
            {
                TryFire();
            }
        }
        else if (fireMode == FireMode.FullAuto)
        {
            if (InputManager.instance.controls.Gun.Fire.IsPressed())
            {
                TryFire();
            }
        }
    }

    private void TryFire()
    {
        if (isReloading) return;
        if (!isMeleeWeapon && currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Time.time < nextTimeToFire) return;

        nextTimeToFire = Time.time + (1f / fireRate);

        Fire();
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (gunAnimator != null) gunAnimator.SetTrigger("Reload");
        if (ammoText != null) ammoText.text = "Reloading...";
        
        yield return new WaitForSeconds(reloadTime);
        
        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            if (isMeleeWeapon) ammoText.text = "";
            else ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
    }

    public void Fire()
    {
        if (!isMeleeWeapon)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Shoot");
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (RumbleManager.instance != null)
            RumbleManager.instance.RumblePulse(rumbleLeftFrequency, rumbleRightFrequency, rumbleTime);

        Camera cam = null;
        var playerController = GetComponentInParent<SimplePlayerController>();
        if (playerController != null)
        {
            cam = playerController.playerCamera;
            playerController.TriggerCameraShake(cameraShakeIntensity, cameraShakeDuration);
            playerController.TriggerWeaponRecoil(recoilForce);
        }

        // Use Camera for accurate aiming instead of firePoint if possible
        Vector3 rayOrigin = (cam != null) ? cam.transform.position : firePoint.position;
        Vector3 baseDirection = (cam != null) ? cam.transform.forward : -firePoint.forward;

        for (int i = 0; i < raysPerShot; i++)
        {
            Vector3 fireDirection = baseDirection;
            if (aoeSpread > 0f && raysPerShot > 1)
            {
                fireDirection += new Vector3(
                    Random.Range(-aoeSpread, aoeSpread),
                    Random.Range(-aoeSpread, aoeSpread),
                    Random.Range(-aoeSpread, aoeSpread));
                fireDirection.Normalize();
            }

            if (Physics.Raycast(rayOrigin, fireDirection, out RaycastHit hit, range))
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageToEnemy);
                }

                if (impactPrefab != null)
                {
                    Vector3 impactPos = hit.point + hit.normal * 0.01f;
                    Quaternion impactRot = Quaternion.LookRotation(hit.normal);
                    Instantiate(impactPrefab, impactPos, impactRot);
                }

                ParticleSystem effectToPlay = useSameImpactEffect ? impactEffectNonEnemy : ((enemy != null) ? impactEffectEnemy : impactEffectNonEnemy);

                if (effectToPlay != null)
                {
                    Vector3 impactPos = hit.point + hit.normal * 0.01f;
                    Quaternion impactRot = Quaternion.LookRotation(hit.normal);
                    ParticleSystem instance = Instantiate(effectToPlay, impactPos, impactRot);
                    instance.Play();
                    float totalDuration = instance.main.duration + instance.main.startLifetime.constantMax;
                    Destroy(instance.gameObject, totalDuration);
                }
            }
        }
    }
}
