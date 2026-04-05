using UnityEngine;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

[CreateAssetMenu(menuName = "Weapons/Gun Data")]
public class GunData : ScriptableObject
{
    public FireMode fireMode;
    public float fireRate;
    public float range;
    public GameObject impactPrefab;
    public GameObject muzzleFlash;
    public ParticleSystem impactEffect;
}
