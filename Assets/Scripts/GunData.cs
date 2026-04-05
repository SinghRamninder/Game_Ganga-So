using UnityEngine;

public enum FireModes
{
    SemiAuto,
    FullAuto
}

[CreateAssetMenu(menuName = "Weapons/Gun Data")]
public class GunData : ScriptableObject
{
    public FireModes fireMode;
    public float fireRate;
    public float range;
    public float cameraShakeIntensity;
    public float cameraShakeDuration;
    public float recoilForce = 2f;
    public GameObject impactPrefab;
    public GameObject muzzleFlash;
    public ParticleSystem impactEffect;
}
