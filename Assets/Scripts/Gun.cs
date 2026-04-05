using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;

    private float nextTimeToFire = 0f;

    public void HandleFireInput()
    {
        if (gunData.fireMode == FireMode.SemiAuto)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryFire();
            }
        }
        else if (gunData.fireMode == FireMode.FullAuto)
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

        nextTimeToFire = Time.time + (1f / gunData.fireRate);

        Fire();
    }

    public void Fire()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (Physics.Raycast(firePoint.position, -firePoint.forward, out RaycastHit hit, gunData.range))
        {
            if (gunData.impactPrefab != null)
            {
                Vector3 impactPosition = hit.point + hit.normal * 0.01f;
                Quaternion impactRotation = Quaternion.LookRotation(hit.normal);

                Instantiate(gunData.impactPrefab, impactPosition, impactRotation);

                if (gunData.impactEffect != null)
                {
                    ParticleSystem impactEffect = Instantiate(gunData.impactEffect, impactPosition, impactRotation);
                    impactEffect.Play();

                    float totalDuration = impactEffect.main.duration + impactEffect.main.startLifetime.constantMax;
                    Destroy(impactEffect.gameObject, totalDuration);
                }
            }
        }
    }
}
