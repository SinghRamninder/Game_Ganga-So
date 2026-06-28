using UnityEngine;
using System.Collections;
using FMODUnity;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

public struct BarrelTraceResult
{
    public bool hitBarrel;
    public Vector3 hitPosition;
    public Vector3 trailSpawnPosition;
    public GameObject hitTrailObject;
    public ExplosiveBarrels barrel;
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

    [HideInInspector] public float damageToEnemyHead = 10f;
    [HideInInspector] public float damageToEnemyBody = 10f;
    [HideInInspector] public int raysPerShot = 1;
    [HideInInspector] public float aoeSpread = 0f;

    [HideInInspector] public int maxAmmo = 30;
    [HideInInspector] public float reloadTime = 1.5f;
    [HideInInspector] public TMPro.TextMeshProUGUI ammoText;

    private Transform firePoint;
    [HideInInspector] public ParticleSystem muzzleFlash;
    [HideInInspector] public Animator gunAnimator;
    [HideInInspector] public Animator gunAnimator2;

    [HideInInspector] public GameObject impactPrefab;
    [HideInInspector] public ParticleSystem impactEffectNonEnemy;
    [HideInInspector] public ParticleSystem impactEffectEnemy;
    [HideInInspector] public bool useSameImpactEffect = false;
    
    private float nextTimeToFire = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private bool firedDuringAttackPress = false;
    private bool firedDuringLeftPress = false;
    private bool firedDuringRightPress = false;

    [HideInInspector] public Vector3 weaponSpawnPosition = Vector3.zero;
    [HideInInspector] public Quaternion weaponSpawnRotation = Quaternion.identity;
    [HideInInspector] public Vector3 weaponSpawnScale = Vector3.one;

    [HideInInspector] public bool isDualPistol = false;
    [HideInInspector] public bool isRailGun = false;
    [HideInInspector] public bool differentShootButtons = true;
    [HideInInspector] public Transform shootPoint1;
    private bool isSP1 = true;
    private bool isMP1 = true;
    [HideInInspector] public Transform shootPoint2;

    [Header("Animations")]
    public string attackAnimString = "Attack";
    public string reloadAnimString = "Reload";
    [HideInInspector] public bool useReloadAnimLength = false;
[Header("UI Portrait Settings")]
    public PlayerPortraitData weaponPortraitData;
    private Sprite gunFirstSprite;
    private Sprite gunSecondSprite;

    [HideInInspector] public bool useOldMethod = false;

    [Header("Splatter settings")]
    public int splatterNumber = 0;
    public BloodImpactProfile particlesProfile;

    [Header("LayerMask Settings")]
    public LayerMask layerMask;

    [Header("Audio Settings")] [Header("FMOD")]
    public EventReference fireEventFmod;
    public EventReference tailEventFmod;
    public EventReference reloadEventFmod;
    public AudioClip reloadSound;
    [Range(0f, 1f)] public float reloadVolume;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume;
    public GameObject audioSource;
    [HideInInspector] public GameObject audioSource2;

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
        if (gunFirstSprite == null)
        {
            SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
            if (childSprites.Length > 0) gunFirstSprite = childSprites[0].sprite;
            if (childSprites.Length > 1 && isDualPistol) gunSecondSprite = childSprites[1].sprite;
        }

        if (gunAnimator == null)
        {
            Animator[] animators = GetComponentsInChildren<Animator>();
            if (animators.Length > 0) gunAnimator = animators[0];
            if (animators.Length > 1 && isDualPistol) gunAnimator2 = animators[1];
        }

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
        if (gunAnimator != null)
        {
            PlayAnimation("Default", gunAnimator);
        }
        if (gunAnimator2 != null && isDualPistol)
        {
            PlayAnimation("Default", gunAnimator2);
        }
        if (gunFirstSprite != null)
        {
            SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
            if (childSprites.Length > 0) childSprites[0].sprite = gunFirstSprite;
            if (childSprites.Length > 1 && isDualPistol && gunSecondSprite != null) childSprites[1].sprite = gunSecondSprite;
        }

        if (audioSource != null)
        {
            AudioSource[] audioSources = audioSource.GetComponents<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null)
                    Destroy(audioSource);
            }
        }
        
        if (audioSource2 != null)
        {
            AudioSource[] audioSources = audioSource2.GetComponents<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null)
                    Destroy(audioSource);
            }
        }

        if (PlayerHealthManager.instance != null && !isMeleeWeapon)
        {
            PlayerHealthManager.instance.RemoveHealthBonus(healthModifier);
        }
        var pc = GetComponentInParent<SimplePlayerController>();
        if (pc != null) pc.SetSpeedModifier(1f);
    }

    public void CheckpointSpawn()
    {
        currentAmmo = maxAmmo;

        if (gunAnimator != null)
        {
            PlayAnimation("Default", gunAnimator);
        }

        if (gunAnimator2 != null && isDualPistol)
        {
            PlayAnimation("Default", gunAnimator2);
        }

        if (gunFirstSprite != null)
        {
            SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
            if (childSprites.Length > 0) childSprites[0].sprite = gunFirstSprite;
            if (childSprites.Length > 1 && isDualPistol && gunSecondSprite != null) childSprites[1].sprite = gunSecondSprite;
        }

        if (audioSource != null)
        {
            AudioSource[] audioSources = audioSource.GetComponents<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null)
                    Destroy(audioSource);
            }
        }

        if (audioSource2 != null)
        {
            AudioSource[] audioSources = audioSource2.GetComponents<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null)
                    Destroy(audioSource);
            }
        }
    }

    private void Update()
    {
        if (!isMeleeWeapon && !isReloading && currentAmmo == 0)
        {
            StartCoroutine(Reload());
        }

        if (!isMeleeWeapon && !isReloading && currentAmmo < maxAmmo)
        {
            if (InputManager.instance.controls.Weapon.Reload.WasPressedThisFrame())
            {
                StartCoroutine(Reload());
            }
        }
    }

    public void HandleFireInput()
    {
        if (PauseMenuController.Instance?.IsGamePaused == true)
            return;

        if (CutsceneManager.Instance?.IsCutscenePlaying == true)
            return;

        if (GameOverManager.instance?.IsGameOver == true)
            return;

        return; // This is a temporary measure to disable firing during the final shot. Remove this line when implementing the final shot logic.

        if (fireMode == FireMode.SemiAuto)
        {
            if (isDualPistol && differentShootButtons)
            {
                if (InputManager.instance.controls.Weapon.DualPistolLeft.WasPressedThisFrame())
                {
                    isSP1 = true;
                    isMP1 = true;
                    TryFire();
                }

                if (InputManager.instance.controls.Weapon.DualPistolRight.WasPressedThisFrame())
                {
                    isSP1 = false;
                    isMP1 = false;
                    TryFire();
                }
            }
            else if (InputManager.instance.controls.Weapon.Attack.WasPressedThisFrame())
            {
                TryFire();
            }

        }
        else if (fireMode == FireMode.FullAuto)
        {
            if (isDualPistol && differentShootButtons)
            {
                if (InputManager.instance.controls.Weapon.DualPistolLeft.IsPressed())
                {
                    isSP1 = true;
                    isMP1 = true;
                    if (TryFire()) firedDuringLeftPress = true;
                }
                else if (InputManager.instance.controls.Weapon.DualPistolLeft.WasReleasedThisFrame())
                {
                    PlayTailEvent(ref firedDuringLeftPress);
                }

                if (InputManager.instance.controls.Weapon.DualPistolRight.IsPressed())
                {
                    isSP1 = false;
                    isMP1 = false;
                    if (TryFire()) firedDuringRightPress = true;
                }
                else if (InputManager.instance.controls.Weapon.DualPistolRight.WasReleasedThisFrame())
                {
                    PlayTailEvent(ref firedDuringRightPress);
                }
            }
            else if (InputManager.instance.controls.Weapon.Attack.IsPressed())
            {
                if (TryFire()) firedDuringAttackPress = true;
            }
            else if (InputManager.instance.controls.Weapon.Attack.WasReleasedThisFrame())
            {
                PlayTailEvent(ref firedDuringAttackPress);
            }
        }
    }

    private bool TryFire()
    {
        if (isReloading) return false;
        if (!isMeleeWeapon && currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return false;
        }

        if (Time.time < nextTimeToFire) return false;

        nextTimeToFire = Time.time + (1f / fireRate);

        if (isMeleeWeapon)
        {
            StartCoroutine(MeleeAttack());
            return false;
        }
        else
        {
            Fire();
            return true;
        }
    }

    private void PlayTailEvent(ref bool firedDuringPress)
    {
        if (firedDuringPress && !tailEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(tailEventFmod, gameObject);
        }

        firedDuringPress = false;
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        if (!reloadEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(reloadEventFmod, gameObject);
        }

        if (audioSource != null & reloadSound != null)
        {
            if (isDualPistol)
            {
                StartCoroutine(AudioManager.Instance.PlaySound(audioSource, reloadSound, reloadVolume, true));
            }
            else
            {
                StartCoroutine(AudioManager.Instance.PlaySound(audioSource, reloadSound, reloadVolume, false));
            }
        }

        if (isDualPistol && audioSource2 != null & reloadSound != null)
        {
            StartCoroutine(AudioManager.Instance.PlaySound(audioSource2, reloadSound, reloadVolume, true));
        }

        float currentReloadTime = reloadTime;

        if (gunAnimator != null) 
        {
            PlayAnimation(reloadAnimString, gunAnimator);
            if (useReloadAnimLength)
            {
                AnimatorClipInfo[] clipInfo = gunAnimator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    currentReloadTime = clipInfo[0].clip.length;
                }
            }
        }

        if (isDualPistol && gunAnimator2 != null)
        {
            PlayAnimation(reloadAnimString, gunAnimator2);
        }

        if (ammoText != null) ammoText.text = "Reloading...";
        
        yield return new WaitForSeconds(currentReloadTime);

        //if (gunAnimator != null) gunAnimator.SetBool(reloadAnimString, false);
        //if (isDualPistol && gunAnimator2 != null) gunAnimator2.SetBool(reloadAnimString, false);

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

    private void PlayAnimation(string animName, Animator currentAnimator)
    {
        if (currentAnimator != null && currentAnimator.isActiveAndEnabled && !string.IsNullOrEmpty(animName))
        {
            if (fireMode == FireMode.SemiAuto)
            {
                currentAnimator.Play(animName, 0, 0f);
            }
            else
            {
                currentAnimator.Play(animName);
            }
            
            currentAnimator.Update(0f);
        }
    }

    private IEnumerator MeleeAttack()
    {
        Animator currentAnimator = gunAnimator;
        if (isDualPistol && !isSP1 && gunAnimator2 != null)
        {
            currentAnimator = gunAnimator2;
        }

        if (currentAnimator != null && !string.IsNullOrEmpty(attackAnimString))
        {
            PlayAnimation(attackAnimString, currentAnimator);
        }

        GameObject currentAudio = audioSource;
        if (isDualPistol && !isSP1 && audioSource2 != null)
        {
            currentAudio = audioSource2;
        }

        if (currentAudio != null & shootSound != null)
        {
            if (isDualPistol)
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, true));
            }
            else
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, false));
            }
        }

        if (!fireEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(fireEventFmod, gameObject);
        }

        if (isDualPistol)
        {
            isSP1 = !isSP1;
        }

        float animLength = 1f;
        if (currentAnimator != null)
        {
            AnimatorClipInfo[] clipInfo = currentAnimator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                animLength = clipInfo[0].clip.length;
            }
        }

        yield return new WaitForSeconds(animLength * 0.5f);

        Camera cam = null;
        var playerController = GetComponentInParent<SimplePlayerController>();
        if (playerController != null)
        {
            cam = playerController.playerCamera;
        }

        Vector3 rayOrigin = (cam != null) ? cam.transform.position : transform.position;
        Vector3 baseDirection = (cam != null) ? cam.transform.forward : transform.forward;

        if (Physics.Raycast(rayOrigin, baseDirection, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            EnemyHitBox enemyHitBox = hit.collider.GetComponent<EnemyHitBox>();
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null && enemyHitBox != null)
            {
                if (enemyHitBox.bodyPart == BodyPart.Head)
                {
                    enemy.TakeDamage(damageToEnemyHead, BodyPart.Head);
                }
                else if (enemyHitBox.bodyPart == BodyPart.Body)
                {
                    enemy.TakeDamage(damageToEnemyBody, BodyPart.Body);
                }
                
                if (impactEffectEnemy != null)
                {
                    Vector3 impactPos = hit.point + hit.normal * 0.05f;
                    Quaternion impactRot = Quaternion.LookRotation(hit.normal);
                    ParticleSystem instance = Instantiate(impactEffectEnemy, impactPos, impactRot);

                    ParticleCollisionSpawner instanceScript = instance.GetComponent<ParticleCollisionSpawner>();
                    if (impactPrefab != null && instanceScript != null)
                        instanceScript.prefabToSpawn = impactPrefab;

                    if (instanceScript != null)
                    {
                        instanceScript.SpawnBlood(-baseDirection, particlesProfile);
                    }
                }
            }
            else
            {
                if (impactEffectNonEnemy != null)
                {
                    Instantiate(impactEffectNonEnemy, hit.point + hit.normal * 0.05f, Quaternion.LookRotation(hit.normal));
                }
            }
        }

        yield return new WaitForSeconds(animLength * 0.5f);
    }

    public BarrelTraceResult FireAndReturnBarrelHit(bool leftClick, bool rightClick)
    {
        var result = new BarrelTraceResult();

        bool useLeftSide = true;
        if (isDualPistol && differentShootButtons)
        {
            if (leftClick)
            {
                useLeftSide = true;
                isSP1 = true;
                isMP1 = true;
            }
            else if (rightClick)
            {
                useLeftSide = false;
                isSP1 = false;
                isMP1 = false;
            }
            else
            {
                useLeftSide = isSP1;
            }
        }

        if (!isMeleeWeapon)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }

        Animator currentAnimator = gunAnimator;
        if (isDualPistol && !useLeftSide && gunAnimator2 != null)
        {
            currentAnimator = gunAnimator2;
        }

        if (currentAnimator != null)
        {
            PlayAnimation(attackAnimString, currentAnimator);
        }

        GameObject currentAudio = audioSource;
        if (isDualPistol && !useLeftSide && audioSource2 != null)
        {
            currentAudio = audioSource2;
        }

        if (currentAudio != null && shootSound != null)
        {
            if (isDualPistol)
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, true));
            }
            else
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, false));
            }
        }

        if (!fireEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(fireEventFmod, gameObject);
        }

        if (muzzleFlash != null && !isDualPistol)
        {
            muzzleFlash.Play();
        }
        else if (isDualPistol && muzzleFlash != null)
        {
            if (isMP1 && shootPoint1 != null)
            {
                muzzleFlash.transform.position = shootPoint1.position;
                muzzleFlash.Play();
                if (!differentShootButtons)
                    isMP1 = false;
            }
            else if (shootPoint2 != null)
            {
                muzzleFlash.transform.position = shootPoint2.position;
                muzzleFlash.Play();
                if (!differentShootButtons)
                    isMP1 = true;
            }
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

        Transform spawnTransform = trailSpawnPos;
        if (spawnTransform == null)
        {
            if (isDualPistol)
            {
                spawnTransform = useLeftSide ? shootPoint1 : shootPoint2;
            }

            if (spawnTransform == null)
            {
                spawnTransform = firePoint;
            }

            if (spawnTransform == null)
            {
                spawnTransform = transform;
            }
        }

        Vector3 rayOrigin = (cam != null) ? cam.transform.position : (firePoint != null ? firePoint.position : transform.position);
        Vector3 baseDirection = (cam != null) ? cam.transform.forward : ((firePoint != null) ? -firePoint.forward : transform.forward);

        bool freezeTrail = false;
        GameObject trailObjectToReturn = null;
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

            Vector3 hitNormal = Vector3.zero;
            Vector3 targetPoint = rayOrigin + (fireDirection * 100000f);
            if (Physics.Raycast(rayOrigin, fireDirection, out RaycastHit hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
                hitNormal = hit.normal;

                ExplosiveBarrels barrel = hit.collider.GetComponent<ExplosiveBarrels>();
                if (barrel == null)
                {
                    barrel = hit.collider.GetComponentInParent<ExplosiveBarrels>();
                }

                if (barrel != null && trailObjectToReturn == null)
{
    result.hitBarrel = true;
    result.barrel = barrel;
    result.hitPosition = hit.point;
    result.trailSpawnPosition = spawnTransform.position;
    result.hitTrailObject = null;

    freezeTrail = true;
}
            }

            GameObject spawnedTrail = SpawnTracerTrail(spawnTransform.position, targetPoint, hitNormal, fireDirection, false, null, freezeTrail);
            if (freezeTrail && trailObjectToReturn == null)
            {
                trailObjectToReturn = spawnedTrail;
                result.hitTrailObject = spawnedTrail;
            }
            freezeTrail = false;
        }

        return result;
    }

    public void Fire()
    {
        if (!isMeleeWeapon)
        {
            currentAmmo--;
            UpdateAmmoUI();
        }

        Animator currentAnimator = gunAnimator;
        if (isDualPistol && !isSP1 && gunAnimator2 != null)
        {
            currentAnimator = gunAnimator2;
        }

        if (currentAnimator != null)
        {
            PlayAnimation(attackAnimString, currentAnimator);
        }

        GameObject currentAudio = audioSource;
        if (isDualPistol && !isSP1 && audioSource2 != null)
        {
            currentAudio = audioSource2;
        }

        if (currentAudio != null & shootSound != null)
        {
            if (isDualPistol)
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, true));
            }
            else
            {
                StartCoroutine(AudioManager.Instance.PlaySound(currentAudio, shootSound, shootVolume, false));
            }
        }

        if (!fireEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(fireEventFmod, gameObject);
        }
        if (muzzleFlash != null && !isDualPistol)
        {
            muzzleFlash.Play();
        }
        else if (isDualPistol)
        {
            if (isMP1)
            {
                muzzleFlash.transform.position = shootPoint1.position;
                muzzleFlash.Play();
                if (!differentShootButtons)
                    isMP1 = false;
            }
            else
            {
                muzzleFlash.transform.position = shootPoint2.position;
                muzzleFlash.Play();
                if (!differentShootButtons)
                    isMP1 = true;
            }
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

            if (isDualPistol)
            {
                if (isSP1)
                {
                    trailSpawnPos = shootPoint1;
                    if (!differentShootButtons)
                        isSP1 = false;
                }
                else
                {
                    trailSpawnPos = shootPoint2;
                    if (!differentShootButtons)
                        isSP1 = true;
                }
            }

            if (isRailGun)
            {
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, fireDirection, range, layerMask, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                if (hits.Length == 0)
                {
                    Vector3 maxRangePoint = rayOrigin + (fireDirection * range);
                    StartCoroutine(SpawnTracer(trailSpawnPos.position, maxRangePoint, Vector3.zero, Vector3.zero, false, null));
                }
                else
                {
                    bool hitWall = false;
                    foreach (RaycastHit hit in hits)
                    {
                        EnemyHitBox enemyHitBox = hit.collider.GetComponent<EnemyHitBox>();
                        Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                        ExplosiveBarrels barrel = hit.collider.GetComponent<ExplosiveBarrels>();
                        ExplosiveServers server = hit.collider.GetComponent<ExplosiveServers>();
                        NoSplatterSurface noSplatterSurface = hit.collider.GetComponent<NoSplatterSurface>();

                        if (enemy != null && enemyHitBox != null)
                        {
                            if (enemyHitBox.bodyPart == BodyPart.Head)
                            {
                                enemy.TakeDamage(damageToEnemyHead, BodyPart.Head);
                                StartCoroutine(headShotEffect());
                            }
                            else if (enemyHitBox.bodyPart == BodyPart.Body)
                            {
                                enemy.TakeDamage(damageToEnemyBody, BodyPart.Body);
                            }

                            StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, fireDirection, false, true));
                        }
                        else
                        {
                            bool isSplatterSurface = true;
                            if (noSplatterSurface != null)
                            {
                                isSplatterSurface = false;
                            }

                            if (barrel != null)
                            {
                                barrel.Explode();
                            }
                            if (server != null)
                            {
                                server.TakeDamage(damageToEnemyBody, hit.point, hit.normal);
                            }
                            StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, fireDirection, isSplatterSurface, false));
                            hitWall = true;
                            break;
                        }
                    }

                    if (!hitWall)
                    {
                        Vector3 maxRangePoint = rayOrigin + (fireDirection * range);
                        StartCoroutine(SpawnTracer(trailSpawnPos.position, maxRangePoint, Vector3.zero, Vector3.zero, false, null));
                    }
                }
            }
            else
            {
                if (Physics.Raycast(rayOrigin, fireDirection, out RaycastHit hit, range, layerMask, QueryTriggerInteraction.Ignore))
                {
                    EnemyHitBox enemyHitBox = hit.collider.GetComponent<EnemyHitBox>();
                    Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                    ExplosiveBarrels barrel = hit.collider.GetComponent<ExplosiveBarrels>();
                    ExplosiveServers server = hit.collider.GetComponent<ExplosiveServers>();
                    NoSplatterSurface noSplatterSurface = hit.collider.GetComponent<NoSplatterSurface>();

                    if (enemy != null && enemyHitBox != null)
                    {
                        if (enemyHitBox.bodyPart == BodyPart.Head)
                        {
                            Debug.Log("Head Shot");
                            enemy.TakeDamage(damageToEnemyHead, BodyPart.Head);
                            StartCoroutine(headShotEffect());
                        }
                        else if (enemyHitBox.bodyPart == BodyPart.Body)
                        {
                            Debug.Log("Body Shot");
                            enemy.TakeDamage(damageToEnemyBody, BodyPart.Body);
                        }

                        StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, fireDirection, false, true));
                    }

                    if (enemy == null)
                    {
                        bool isSplatterSurface = true;
                        if (noSplatterSurface != null)
                        {
                            isSplatterSurface = false;
                        }

                        if (barrel != null)
                        {
                            barrel.Explode();
                        }
                        if (server != null)
                        {
                            server.TakeDamage(damageToEnemyBody, hit.point, hit.normal);
                        }
                        StartCoroutine(SpawnTracer(trailSpawnPos.position, hit.point, hit.normal, fireDirection, isSplatterSurface, false));
                    }

                }
                else
                {
                    Vector3 maxRangePoint = rayOrigin + (fireDirection * range);
                    StartCoroutine(SpawnTracer(trailSpawnPos.position, maxRangePoint, Vector3.zero, Vector3.zero, false, null));
                }
            }
        }
    }

    private IEnumerator SpawnTracer(Vector3 spawnPos, Vector3 targetPos, Vector3 hitNormal, Vector3 bulletDirection, bool spawnSplatter, bool? isEnemyHit = null, bool freezeTrailPosition = false)
    {
        TrailRenderer bulletTrailSpawned = Instantiate(bulletTrail, spawnPos, Quaternion.identity);
        if (bulletTrailSpawned != null)
        {
            yield return StartCoroutine(AnimateTracer(bulletTrailSpawned, targetPos, hitNormal, bulletDirection, spawnSplatter, isEnemyHit, freezeTrailPosition));
        }
    }

    private GameObject SpawnTracerTrail(Vector3 spawnPos, Vector3 targetPos, Vector3 hitNormal, Vector3 bulletDirection, bool spawnSplatter, bool? isEnemyHit = null, bool freezeTrailPosition = false)
    {
        TrailRenderer bulletTrailSpawned = Instantiate(bulletTrail, spawnPos, Quaternion.identity);
        if (bulletTrailSpawned != null)
        {
            StartCoroutine(AnimateTracer(bulletTrailSpawned, targetPos, hitNormal, bulletDirection, spawnSplatter, isEnemyHit, freezeTrailPosition));
            return bulletTrailSpawned.gameObject;
        }

        return null;
    }

    private IEnumerator AnimateTracer(TrailRenderer bulletTrailSpawned, Vector3 targetPos, Vector3 hitNormal, Vector3 bulletDirection, bool spawnSplatter, bool? isEnemyHit = null, bool freezeTrailPosition = false)
    {
        float time = 0;

        if (!freezeTrailPosition)
        {
            while (time < 1)
            {
                bulletTrailSpawned.transform.position = Vector3.Lerp(bulletTrailSpawned.transform.position, targetPos, time);
                time += Time.deltaTime / bulletTrailSpawned.time;
                yield return null;
            }

            bulletTrailSpawned.transform.position = targetPos;
        }
        else
        {
            bulletTrailSpawned.transform.position = targetPos;
        }

        ParticleSystem effectToPlay = null;

        if (isEnemyHit == false)
        {
            if (impactPrefab != null && spawnSplatter)
            {
                float targetSize = SplatterMeshManager.Instance != null ? SplatterMeshManager.Instance.GetMaxSplatterSize(splatterNumber) : 1.5f;
                if (IsFullyOnWall(targetPos, hitNormal, targetSize))
                {
                    if (useOldMethod) //Testing purposes
                    {
                        Vector3 impactPos = targetPos + hitNormal * 0.01f;
                        Quaternion impactRot = Quaternion.LookRotation(-hitNormal);
                        GameObject spawnedSplat = Instantiate(impactPrefab, impactPos, impactRot);

                        MeshRenderer splatMesh = spawnedSplat.GetComponent<MeshRenderer>();
                        if (splatMesh != null)
                        {
                            splatMesh.sortingLayerName = "Splatter";
                            if (PlayerWeaponManager.instance != null)
                                splatMesh.sortingOrder = PlayerWeaponManager.instance.weaponLayerNumber;

                            Debug.Log(splatMesh.sortingLayerName);
                            Debug.Log(splatMesh.sortingOrder);
                        }

                        if (PlayerWeaponManager.instance != null)
                            PlayerWeaponManager.instance.weaponLayerNumber++;
                    }
                    else
                    {
                        SplatterMeshManager.Instance.AddSplatter(splatterNumber, targetPos, hitNormal);
                    }
                }
                
            }

            if (impactEffectNonEnemy != null) effectToPlay = impactEffectNonEnemy;
        }
        else if (isEnemyHit == true)
        {
            if (impactEffectEnemy != null) effectToPlay = impactEffectEnemy;
        }

        if (effectToPlay != null)
        {
            Vector3 impactPos = targetPos + hitNormal * -0.05f;
            Quaternion impactRot = Quaternion.LookRotation(hitNormal);
            ParticleSystem instance = Instantiate(effectToPlay, impactPos, impactRot);

            ParticleCollisionSpawner instanceScript = instance.GetComponent<ParticleCollisionSpawner>();
            if (impactPrefab != null && instanceScript != null)
                instanceScript.prefabToSpawn = impactPrefab;

            if (instanceScript != null)
            {
                instanceScript.SpawnBlood(bulletDirection, particlesProfile);
            }

            //instance.Play();
            //float totalDuration = instance.main.duration + instance.main.startLifetime.constantMax;
            //Destroy(instance.gameObject, totalDuration);
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
        Vector3 baseDirection = transform.forward;

        if (cam != null)
        {
            rayOrigin = cam.transform.position;
            baseDirection = cam.transform.forward;
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

    bool IsFullyOnWall(Vector3 center, Vector3 normal, float size)
    {
        Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;

        if (right == Vector3.zero)
            right = Vector3.Cross(Vector3.forward, normal).normalized;

        Vector3 up = Vector3.Cross(normal, right);

        float half = size * 0.5f;

        Vector3[] corners =
        {
        center + right * half + up * half,
        center - right * half + up * half,
        center + right * half - up * half,
        center - right * half - up * half
    };

        foreach (Vector3 corner in corners)
        {
            Vector3 start = corner + normal * 0.05f;

            if (!Physics.Raycast(start, -normal, 0.1f))
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator headShotEffect()
    {
        if (PlayerWeaponManager.instance.headCrosshair == null)
            yield break;

        PlayerWeaponManager.instance.headCrosshair.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        PlayerWeaponManager.instance.headCrosshair.SetActive(false);
    }

    //private IEnumerator soundEffect(GameObject targetAudioSourceObj, AudioClip clip, float volume)
    //{
    //    AudioSource source = targetAudioSourceObj.AddComponent<AudioSource>();
    //    source.clip = clip;
    //    source.volume = volume;
    //    if (isDualPistol)
    //    {
    //        source.spatialBlend = 1f;
    //        source.minDistance = 5f;
    //        source.maxDistance = 50f;
    //    }
    //    source.Play();

    //    yield return new WaitForSeconds(source.clip.length);
        
    //    Destroy(source, 2f);
    //}
}
