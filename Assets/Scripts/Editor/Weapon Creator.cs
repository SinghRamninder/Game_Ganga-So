using System.IO;
using UnityEditor;
using UnityEngine;

public class WeaponCreator : EditorWindow
{
    private enum WeaponType
    {
        Gun,
        Melee
    }

    private string weaponName = "Weapon Name";
    private Sprite weaponSprite;

    private FireMode fireMode;
    private float fireRate;
    private float fireRange;
    private float damage;
    private int bulletsPerShot;
    private float areaOfEffect;
    private int maxAmmo;
    private float reloadTime;
    private float recoilForce;

    private float playerHealthModifier;
    private float playerSpeedModifier = 1f;

    private TrailRenderer bulletTrail;
    private Transform trailSpawnPos;

    private ParticleSystem muzzleFlash;
    private GameObject impactPrefab;
    private ParticleSystem impactEffectNonEnemy;
    private ParticleSystem impactEffectEnemy;

    private int selectedTemplateIndex = 0;
    private string[] availableWeaponNames;
    private GameObject[] availableWeaponPrefabs;

    private float copiedCameraShakeIntensity;
    private float copiedCameraShakeDuration;
    private float copiedRumbleLeftFrequency;
    private float copiedRumbleRightFrequency;
    private float copiedRumbleTime;
    private bool copiedUseSameImpactEffect;
    private Vector3 copiedWeaponSpawnPosition = Vector3.zero;
    private Quaternion copiedWeaponSpawnRotation = Quaternion.identity;
    private Vector3 copiedWeaponSpawnScale = Vector3.one;

    private WeaponType weaponType = WeaponType.Gun;

    [MenuItem("Tools/ Weapon Creator")]
    public static void OpenWindow()
    {
        GetWindow<WeaponCreator>("Weapon Creator");
    }

    private void OnEnable()
    {
        LoadAvailableWeapons();
    }

    private void LoadAvailableWeapons()
    {
        string folderPath = "Assets/Prefabs/Weapons";
        if (!Directory.Exists(folderPath))
        {
            availableWeaponNames = new string[] { "None" };
            availableWeaponPrefabs = new GameObject[] { null };
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
        availableWeaponPrefabs = new GameObject[guids.Length + 1];
        availableWeaponNames = new string[guids.Length + 1];

        availableWeaponNames[0] = "None";
        availableWeaponPrefabs[0] = null;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            availableWeaponPrefabs[i + 1] = prefab;
            if (prefab != null) availableWeaponNames[i + 1] = prefab.name;
        }
    }

    private void CopyValuesFromWeapon(GameObject prefab)
    {
        if (prefab == null) return;
        Weapon w = prefab.GetComponent<Weapon>();
        if (w == null) return;

        weaponType = w.isMeleeWeapon ? WeaponType.Melee : WeaponType.Gun;
        fireMode = w.fireMode;
        fireRate = w.fireRate;
        fireRange = w.range;
        damage = w.damageToEnemy;
        bulletsPerShot = w.raysPerShot;
        areaOfEffect = w.aoeSpread;
        maxAmmo = w.maxAmmo;
        reloadTime = w.reloadTime;
        recoilForce = w.recoilForce;

        playerHealthModifier = w.healthModifier;
        playerSpeedModifier = w.speedModifier;

        bulletTrail = w.bulletTrail;
        muzzleFlash = w.muzzleFlash;
        impactPrefab = w.impactPrefab;
        impactEffectNonEnemy = w.impactEffectNonEnemy;
        impactEffectEnemy = w.impactEffectEnemy;

        copiedCameraShakeIntensity = w.cameraShakeIntensity;
        copiedCameraShakeDuration = w.cameraShakeDuration;
        copiedRumbleLeftFrequency = w.rumbleLeftFrequency;
        copiedRumbleRightFrequency = w.rumbleRightFrequency;
        copiedRumbleTime = w.rumbleTime;
        copiedUseSameImpactEffect = w.useSameImpactEffect;
        copiedWeaponSpawnPosition = w.weaponSpawnPosition;
        copiedWeaponSpawnRotation = w.weaponSpawnRotation;
        copiedWeaponSpawnScale = w.weaponSpawnScale;

        Repaint();
    }

    private void ClearValues()
    {
        weaponName = "Weapon Name";
        weaponSprite = null;

        weaponType = WeaponType.Gun;
        fireMode = FireMode.SemiAuto;
        fireRate = 0f;
        fireRange = 0f;
        damage = 0f;
        bulletsPerShot = 0;
        areaOfEffect = 0f;
        maxAmmo = 0;
        reloadTime = 0f;
        recoilForce = 0f;

        playerHealthModifier = 0f;
        playerSpeedModifier = 1f;

        bulletTrail = null;
        muzzleFlash = null;
        impactPrefab = null;
        impactEffectNonEnemy = null;
        impactEffectEnemy = null;

        copiedCameraShakeIntensity = 0f;
        copiedCameraShakeDuration = 0f;
        copiedRumbleLeftFrequency = 0f;
        copiedRumbleRightFrequency = 0f;
        copiedRumbleTime = 0f;
        copiedUseSameImpactEffect = false;
        copiedWeaponSpawnPosition = Vector3.zero;
        copiedWeaponSpawnRotation = Quaternion.identity;
        copiedWeaponSpawnScale = Vector3.one;

        selectedTemplateIndex = 0;
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Create New Weapon", EditorStyles.boldLabel);

        if (availableWeaponNames == null || availableWeaponNames.Length == 0) LoadAvailableWeapons();

        EditorGUILayout.BeginVertical("box");

        weaponName = EditorGUILayout.TextField(new GUIContent("Weapon Name", "The name of the new weapon prefab."), weaponName);
        weaponSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Weapon Sprite", "The visual sprite representing the weapon."), weaponSprite, typeof(Sprite), false);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup(new GUIContent("Copy from Prefab", "Select an existing weapon prefab to copy values from."), selectedTemplateIndex, availableWeaponNames);
        if (GUILayout.Button("Refresh", GUILayout.Width(60)))
        {
            LoadAvailableWeapons();
            newIndex = 0;
        }
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            ClearValues();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();

        if (newIndex != selectedTemplateIndex)
        {
            selectedTemplateIndex = newIndex;
            if (selectedTemplateIndex > 0)
            {
                CopyValuesFromWeapon(availableWeaponPrefabs[selectedTemplateIndex]);
            }
            else if (selectedTemplateIndex == 0)
            {
                ClearValues();
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.Space();

        weaponType = (WeaponType)EditorGUILayout.EnumPopup(new GUIContent("Weapon Type", "Choose between Gun or Melee."), weaponType);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (weaponType == WeaponType.Gun)
        {
            DrawGunFields();
        }
        else if (weaponType == WeaponType.Melee)
        {
            DrawMeleeFields();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Weapon", GUILayout.Height(30)))
        {
            CreateGunPrefab();
        }
    }

    private void DrawGunFields()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Gun Settings", EditorStyles.boldLabel);

        fireMode = (FireMode)EditorGUILayout.EnumPopup(new GUIContent("Fire Mode", "Determines how the weapon fires (e.g., Automatic, Semi-Automatic)."), fireMode);
        fireRate = EditorGUILayout.FloatField(new GUIContent("Fire Rate", "The rate of fire of the weapon."), fireRate);
        fireRange = EditorGUILayout.FloatField(new GUIContent("Fire Range", "The maximum distance the bullet can travel."), fireRange);
        damage = EditorGUILayout.FloatField(new GUIContent("Damage to Enemy", "Health removed from the enemy on hit."), damage);
        bulletsPerShot = EditorGUILayout.IntField(new GUIContent("Bullets per shot", "Number of bullets fired in a single shot (e.g., for shotguns)."), bulletsPerShot);
        areaOfEffect = EditorGUILayout.FloatField(new GUIContent("Area of Effect Spread", "Spread radius upon impact or multiple rays. Can be adjustable via scene visually with gizmos."), areaOfEffect);
        maxAmmo = EditorGUILayout.IntField(new GUIContent("Maximum Ammo", "Total ammunition capacity in a clip."), maxAmmo);
        reloadTime = EditorGUILayout.FloatField(new GUIContent("Reload Time", "Time in seconds required to reload."), reloadTime);
        recoilForce = EditorGUILayout.FloatField(new GUIContent("Recoil Force", "The upward recoil force applied to the camera when shooting."), recoilForce);

        EditorGUILayout.Space();
        GUILayout.Label("Modifiers", EditorStyles.boldLabel);
        playerHealthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped (+ number increases health by that amount, - decreases it)."), playerHealthModifier);
        playerSpeedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped (< 1 decreases the speed, > 1 increases it)."), playerSpeedModifier);

        EditorGUILayout.Space();
        GUILayout.Label("Visuals & Effects", EditorStyles.boldLabel);
        bulletTrail = (TrailRenderer)EditorGUILayout.ObjectField(new GUIContent("Bullet Trail", "The trail effect left by bullets."), bulletTrail, typeof(TrailRenderer), false);
        muzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Muzzle flash effect", "Effect played at the barrel when firing."), muzzleFlash, typeof(ParticleSystem), false);
        impactPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Bullet impact prefab", "Object spawned when a bullet hits."), impactPrefab, typeof(GameObject), false);
        impactEffectNonEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (Not on enemy)", "Particle effect played on hitting the environment."), impactEffectNonEnemy, typeof(ParticleSystem), false);
        impactEffectEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (On Enemy)", "Particle effect played on hitting an enemy."), impactEffectEnemy, typeof(ParticleSystem), false);

        EditorGUILayout.EndVertical();
    }

    private void DrawMeleeFields()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Melee Settings", EditorStyles.boldLabel);

        fireRate = EditorGUILayout.FloatField(new GUIContent("Attack Rate", "Speed of melee attacks, attacks per second."), fireRate);
        fireRange = EditorGUILayout.FloatField(new GUIContent("Attack Range", "The maximum reach of the melee weapon."), fireRange);
        damage = EditorGUILayout.FloatField(new GUIContent("Damage to Enemy", "The amount of health subtracted from the enemy per hit."), damage);

        EditorGUILayout.Space();
        GUILayout.Label("Modifiers", EditorStyles.boldLabel);
        playerHealthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped (+ number increases health by that amount, - decreases it)."), playerHealthModifier);
        playerSpeedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped (< 1 decreases the speed, > 1 increases it)."), playerSpeedModifier);

        EditorGUILayout.Space();
        GUILayout.Label("Visuals & Effects", EditorStyles.boldLabel);
        impactEffectNonEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (Not on enemy)", "Particle effect played on hitting the environment."), impactEffectNonEnemy, typeof(ParticleSystem), true);
        impactEffectEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (On Enemy)", "Particle effect played on hitting an enemy."), impactEffectEnemy, typeof(ParticleSystem), true);

        EditorGUILayout.EndVertical();
    }

    private void CreateGunPrefab()
    {
        if (weaponName == null)
        {
            Debug.LogError("Enter the Weapon Name");
            return;
        }

        if (weaponSprite == null)
        {
            Debug.LogError("Assign the Weapon Sprite");
            return;
        }

        GameObject gunObject = new GameObject(weaponName);

        Weapon gun = gunObject.AddComponent<Weapon>();
        gunObject.AddComponent<BlockAnimator>();

        gun.cameraShakeIntensity = copiedCameraShakeIntensity;
        gun.cameraShakeDuration = copiedCameraShakeDuration;
        gun.rumbleLeftFrequency = copiedRumbleLeftFrequency;
        gun.rumbleRightFrequency = copiedRumbleRightFrequency;
        gun.rumbleTime = copiedRumbleTime;
        gun.useSameImpactEffect = copiedUseSameImpactEffect;
        gun.weaponSpawnPosition = copiedWeaponSpawnPosition;
        gun.weaponSpawnRotation = copiedWeaponSpawnRotation;
        gun.weaponSpawnScale = copiedWeaponSpawnScale;

        GameObject gunVisuals = new GameObject("Visuals");
        gunVisuals.transform.SetParent(gunObject.transform);
        gunVisuals.transform.localPosition = Vector3.zero;
        gunVisuals.transform.localRotation = Quaternion.identity;
        gunVisuals.transform.localScale = Vector3.one;

        SpriteRenderer spriteRenderer = gunVisuals.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = weaponSprite;

        GameObject trailSpawnObject = new GameObject("Bullet Trail Spawn Position");
        trailSpawnObject.transform.SetParent(gunVisuals.transform);
        trailSpawnObject.transform.localPosition = Vector3.zero;
        trailSpawnObject.transform.localRotation = Quaternion.identity;
        trailSpawnObject.transform.localScale = Vector3.one;
        trailSpawnPos = trailSpawnObject.transform;

        SerializedObject serializedGun = new SerializedObject(gun);

        var propIsMelee = serializedGun.FindProperty("isMeleeWeapon");
        if (propIsMelee != null)
        {
            if (weaponType == WeaponType.Gun)
            {
                propIsMelee.boolValue = false;
            }
            else if (weaponType == WeaponType.Melee)
            {
                propIsMelee.boolValue = true;
            }
        }

            var propertyFireMode = serializedGun.FindProperty("fireMode");
            if (propertyFireMode != null) propertyFireMode.enumValueIndex = (int)fireMode;

            var propFireRate = serializedGun.FindProperty("fireRate");
            if (propFireRate != null) propFireRate.floatValue = fireRate;

            var propFireRange = serializedGun.FindProperty("range");
            if (propFireRange != null) propFireRange.floatValue = fireRange;

            var propDamage = serializedGun.FindProperty("damageToEnemy");
            if (propDamage != null) propDamage.floatValue = damage;

            var propBulletsPerShot = serializedGun.FindProperty("raysPerShot");
            if (propBulletsPerShot != null) propBulletsPerShot.intValue = bulletsPerShot;

            var propAreaOfEffect = serializedGun.FindProperty("aoeSpread");
            if (propAreaOfEffect != null) propAreaOfEffect.floatValue = areaOfEffect;

            var propMaxAmmo = serializedGun.FindProperty("maxAmmo");
            if (propMaxAmmo != null) propMaxAmmo.intValue = maxAmmo;

            var propReloadTime = serializedGun.FindProperty("reloadTime");
            if (propReloadTime != null) propReloadTime.floatValue = reloadTime;

            var propRecoilForce = serializedGun.FindProperty("recoilForce");
            if (propRecoilForce != null) propRecoilForce.floatValue = recoilForce;

            var propHealthMod = serializedGun.FindProperty("healthModifier");
            if (propHealthMod != null) propHealthMod.floatValue = playerHealthModifier;

            var propSpeedMod = serializedGun.FindProperty("speedModifier");
            if (propSpeedMod != null) propSpeedMod.floatValue = playerSpeedModifier;

            var propTrail = serializedGun.FindProperty("bulletTrail");
            if (propTrail != null) propTrail.objectReferenceValue = bulletTrail;

            var propSpawnPos = serializedGun.FindProperty("raySpawnPos");
            if (propSpawnPos != null) propSpawnPos.objectReferenceValue = trailSpawnPos;

            var propMuzzleFlash = serializedGun.FindProperty("muzzleFlash");
            if (propMuzzleFlash != null) propMuzzleFlash.objectReferenceValue = muzzleFlash;

            var propImpactPrefab = serializedGun.FindProperty("impactPrefab");
            if (propImpactPrefab != null) propImpactPrefab.objectReferenceValue = impactPrefab;

            var propImpactNonEnemy = serializedGun.FindProperty("impactEffectNonEnemy");
            if (propImpactNonEnemy != null) propImpactNonEnemy.objectReferenceValue = impactEffectNonEnemy;

            var propImpactEnemy = serializedGun.FindProperty("impactEffectEnemy");
            if (propImpactEnemy != null) propImpactEnemy.objectReferenceValue = impactEffectEnemy;

        var propTrailSpawnPos = serializedGun.FindProperty("trailSpawnPos");
        if (propTrailSpawnPos != null) propTrailSpawnPos.objectReferenceValue = trailSpawnPos;

            serializedGun.ApplyModifiedProperties();

            int weaponLayer = LayerMask.NameToLayer("Weapon");
            if (weaponLayer != -1)
            {
                SetLayerRecursively(gunObject, weaponLayer);
            }
            else
            {
                Debug.LogWarning("Layer 'Weapon' does not exist. Please add it in tags and layers manager.");
            }

            string folderPath = "Assets/Prefabs/Weapons";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string prefabPath = $"{folderPath}/{weaponName}.prefab";

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(gunObject, prefabPath);
            DestroyImmediate(gunObject);

            AssetDatabase.Refresh();

            if (savedPrefab != null)
            {
                GameObject weaponInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab);
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    Transform mainCam = player.transform.Find("Main Camera");
                    if (mainCam != null)
                    {
                        Transform weaponHolder = mainCam.Find("WeaponHolder");
                        if (weaponHolder != null)
                        {
                            weaponInstance.transform.SetParent(weaponHolder);
                            weaponInstance.transform.localPosition = Vector3.zero;
                            weaponInstance.transform.localRotation = Quaternion.identity;
                            weaponInstance.transform.localScale = Vector3.one;
                        }
                        else Debug.LogWarning("WeaponHolder not found under Main Camera.");
                    }
                    else Debug.LogWarning("Main Camera not found under Player.");
                }
                else Debug.LogWarning("Player not found in the scene.");
            }

            Debug.Log($"Gun prefab created: {prefabPath}");
        }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
