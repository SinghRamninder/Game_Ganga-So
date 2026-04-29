using UnityEngine;
using System.Collections;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

public class Weapon : MonoBehaviour
{
    [HideInInspector] public bool isMeleeWeapon = false;

    [HideInInspector] public FireMode fireMode;
    [HideInInspector] public float fireRate;
    [HideInInspector] public float range;
    [HideInInspector] public float cameraShakeIntensity;
    [HideInInspector] public float cameraShakeDuration;
    [HideInInspector] public float recoilForce = 2f;
    [HideInInspector] public float rumbleLeftFrequency;
    [HideInInspector] public float rumbleRightFrequency; 
    [HideInInspector] public float rumbleTime;
    [HideInInspector] public TrailRenderer bulletTrail;
    [HideInInspector] public Transform trailSpawnPos;

    [HideInInspector] public float healthModifier = 0f;
    [HideInInspector] public float speedModifier = 1f;

    [HideInInspector] public float damageToEnemy = 10f;
    [HideInInspector] public int raysPerShot = 1;
    [HideInInspector] public float aoeSpread = 0f;

    [HideInInspector] public int maxAmmo = 30;
    [HideInInspector] public float reloadTime = 1.5f;
    [HideInInspector] public TMPro.TextMeshProUGUI ammoText;

    private Transform firePoint;
    [HideInInspector] public ParticleSystem muzzleFlash;
    [HideInInspector] public Animator gunAnimator;

    [HideInInspector] public GameObject impactPrefab;
    [HideInInspector] public ParticleSystem impactEffectNonEnemy;
    [HideInInspector] public ParticleSystem impactEffectEnemy;
    [HideInInspector] public bool useSameImpactEffect = false;
    
    private float nextTimeToFire = 0f;
    private int currentAmmo;
    private bool isReloading = false;

    [HideInInspector] public Vector3 weaponSpawnPosition = Vector3.zero;
    [HideInInspector] public Quaternion weaponSpawnRotation = Quaternion.identity;
    [HideInInspector] public Vector3 weaponSpawnScale = Vector3.one;

    private void Awake()
    {
        if (ammoText == null)
        {
            GameObject obj = GameObject.Find("Bullet counter");
            if (obj != null)
            {
                ammoText = obj.GetComponent<TMPro.TextMeshProUGUI>();
            }
        }
    }

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
            if (InputManager.instance.controls.Weapon.Attack.WasPressedThisFrame())
            {
                TryFire();
            }
        }
        else if (fireMode == FireMode.FullAuto)
        {
            if (InputManager.instance.controls.Weapon.Attack.IsPressed())
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

                    StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, true));
                }

                if (enemy == null)
                {
                    StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, false));
                }

                
            }
            else
            {
                Vector3 maxRangePoint = rayOrigin + (fireDirection * range);
                StartCoroutine(SpawnTracer(trailSpawnPos.position, maxRangePoint, Vector3.zero, null));
            }
        }
    }

    private IEnumerator SpawnTracer(Vector3 spawnPos, Vector3 targetPos, Vector3 hitNormal, bool? isEnemyHit = null)
    {
        float time = 0;

        TrailRenderer bulletTrailSpawned = Instantiate(bulletTrail, spawnPos, Quaternion.identity);

        while (time < 1)
        {
            bulletTrailSpawned.transform.position = Vector3.Lerp(bulletTrailSpawned.transform.position, targetPos, time);
            time += Time.deltaTime / bulletTrailSpawned.time;
            yield return null;
        }

        bulletTrailSpawned.transform.position = targetPos;

        ParticleSystem effectToPlay = null;

        if (isEnemyHit == false)
        {
            if (impactPrefab != null)
            {
                Vector3 impactPos = targetPos + hitNormal * 0.01f;
                Quaternion impactRot = Quaternion.LookRotation(hitNormal);
                Instantiate(impactPrefab, impactPos, impactRot);
            }

            if (impactEffectNonEnemy != null) effectToPlay = impactEffectNonEnemy;
        }
        else if (isEnemyHit == true)
        {
            if (impactEffectEnemy != null) effectToPlay = impactEffectEnemy;
        }

        if (effectToPlay != null)
        {
            Vector3 impactPos = targetPos + hitNormal * 0.01f;
            Quaternion impactRot = Quaternion.LookRotation(hitNormal);
            ParticleSystem instance = Instantiate(effectToPlay, impactPos, impactRot);

            instance.Play();
            float totalDuration = instance.main.duration + instance.main.startLifetime.constantMax;
            Destroy(instance.gameObject, totalDuration);
        }

        Destroy(bulletTrailSpawned.gameObject, bulletTrailSpawned.time);
    }

    private void OnDrawGizmosSelected()
    {
        Camera cam = null;
        var playerController = GetComponentInParent<SimplePlayerController>();
        if (playerController != null)
        {
            cam = playerController.playerCamera;
        }

        Vector3 rayOrigin = transform.position;
        Vector3 baseDirection = -transform.forward;

        if (cam != null)
        {
            rayOrigin = cam.transform.position;
            baseDirection = -cam.transform.forward;
        }
        else if (firePoint != null)
        {
            rayOrigin = firePoint.position;
            baseDirection = -firePoint.forward;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayOrigin, baseDirection * range);

        if (aoeSpread > 0f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

            Vector3 upBound = (baseDirection + new Vector3(0, aoeSpread, 0)).normalized;
            Vector3 downBound = (baseDirection + new Vector3(0, -aoeSpread, 0)).normalized;
            Vector3 rightBound = (baseDirection + new Vector3(aoeSpread, 0, 0)).normalized;
            Vector3 leftBound = (baseDirection + new Vector3(-aoeSpread, 0, 0)).normalized;

            Gizmos.DrawRay(rayOrigin, upBound * range);
            Gizmos.DrawRay(rayOrigin, downBound * range);
            Gizmos.DrawRay(rayOrigin, rightBound * range);
            Gizmos.DrawRay(rayOrigin, leftBound * range);

            Vector3 endUp = rayOrigin + (upBound * range);
            Vector3 endDown = rayOrigin + (downBound * range);
            Vector3 endRight = rayOrigin + (rightBound * range);
            Vector3 endLeft = rayOrigin + (leftBound * range);

            Gizmos.DrawLine(endUp, endRight);
            Gizmos.DrawLine(endRight, endDown);
            Gizmos.DrawLine(endDown, endLeft);
            Gizmos.DrawLine(endLeft, endUp);
        }
    }
}
