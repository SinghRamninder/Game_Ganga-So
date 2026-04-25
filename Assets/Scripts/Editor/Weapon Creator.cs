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

    private float playerHealthModifier;
    private float playerSpeedModifier;

    private TrailRenderer bulletTrail;
    private Transform trailSpawnPos;

    private ParticleSystem muzzleFlash;
    private GameObject impactPrefab;
    private ParticleSystem impactEffectNonEnemy;
    private ParticleSystem impactEffectEnemy;



    private WeaponType weaponType = WeaponType.Gun;

    [MenuItem("Tools/ Weapon Creator")]
    public static void OpenWindow()
    {
        GetWindow<WeaponCreator>("Weapon Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Create New Weapon", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        weaponName = EditorGUILayout.TextField(new GUIContent("Weapon Name", "The name of the new weapon prefab."), weaponName);
        weaponSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Weapon Sprite", "The visual sprite representing the weapon."), weaponSprite, typeof(Sprite), false);
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
        areaOfEffect = EditorGUILayout.FloatField(new GUIContent("Area of Effect Spread", "Spread radius upon impact or multiple rays."), areaOfEffect);
        maxAmmo = EditorGUILayout.IntField(new GUIContent("Maximum Ammo", "Total ammunition capacity in a clip."), maxAmmo);
        reloadTime = EditorGUILayout.FloatField(new GUIContent("Reload Time", "Time in seconds required to reload."), reloadTime);

        EditorGUILayout.Space();
        GUILayout.Label("Modifiers", EditorStyles.boldLabel);
        playerHealthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped."), playerHealthModifier);
        playerSpeedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped."), playerSpeedModifier);

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
        playerHealthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped."), playerHealthModifier);
        playerSpeedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped."), playerSpeedModifier);

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

            string folderPath = "Assets/Prefabs/Weapons";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string prefabPath = $"{folderPath}/{weaponName}.prefab";

            PrefabUtility.SaveAsPrefabAsset(gunObject, prefabPath);
            DestroyImmediate(gunObject);

            AssetDatabase.Refresh();

            Debug.Log($"Gun prefab created: {prefabPath}");
        }
    }
