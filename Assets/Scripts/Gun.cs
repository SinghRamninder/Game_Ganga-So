using UnityEngine;

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
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem impactEffectPrefab;

    private float nextTimeToFire = 0f;

    public void HandleFireInput()
    {
        if (fireMode == FireMode.SemiAuto)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryFire();
            }
        }
        else if (fireMode == FireMode.FullAuto)
        {
            if (Input.GetMouseButton(0))
            {
                TryFire();
            }
        }
    }

    private void TryFire()
    {
        if (Time.time < nextTimeToFire) return;

        nextTimeToFire = Time.time + (1f / fireRate);

        Fire();
    }

    public void Fire()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        var playerController = GetComponentInParent<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.TriggerCameraShake(cameraShakeIntensity, cameraShakeDuration);
            playerController.TriggerWeaponRecoil(recoilForce);
        }

        if (Physics.Raycast(firePoint.position, -firePoint.forward, out RaycastHit hit, range))
        {
            if (impactPrefab != null)
            {
                Vector3 impactPosition = hit.point + hit.normal * 0.01f;
                Quaternion impactRotation = Quaternion.LookRotation(hit.normal);

                Instantiate(impactPrefab, impactPosition, impactRotation);

                if (impactEffectPrefab != null)
                {
                    ParticleSystem impactEffect = Instantiate(impactEffectPrefab, impactPosition, impactRotation);
                    impactEffect.Play();

                    float totalDuration = impactEffect.main.duration + impactEffect.main.startLifetime.constantMax;
                    Destroy(impactEffect.gameObject, totalDuration);
                }
            }
        }
    }
}
