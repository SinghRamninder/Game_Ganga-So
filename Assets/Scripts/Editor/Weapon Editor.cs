using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Weapon weapon = (Weapon)target;

        weapon.isMeleeWeapon = EditorGUILayout.Toggle("Is Melee Weapon", weapon.isMeleeWeapon);
        EditorGUILayout.Space();

        if (weapon.isMeleeWeapon)
        {
            GUILayout.Label("Melee Settings", EditorStyles.boldLabel);
            weapon.fireRate = EditorGUILayout.FloatField(new GUIContent("Attack Rate", "Speed of melee attacks, attacks per second."), weapon.fireRate);
            weapon.range = EditorGUILayout.FloatField(new GUIContent("Attack Range", "The maximum reach of the melee weapon."), weapon.range);
            weapon.damageToEnemy = EditorGUILayout.FloatField(new GUIContent("Damage to Enemy", "The amount of health subtracted from the enemy per hit."), weapon.damageToEnemy);

            EditorGUILayout.Space();
            GUILayout.Label("Modifiers", EditorStyles.boldLabel);
            weapon.healthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped (+ number increases health by that amount, - decreases it)."), weapon.healthModifier);
            weapon.speedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped (< 1 decreases the speed, > 1 increases it)."), weapon.speedModifier);

            EditorGUILayout.Space();
            GUILayout.Label("Visuals & Effects", EditorStyles.boldLabel);
            weapon.impactEffectNonEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (Not on enemy)", "Particle effect played on hitting the environment."), weapon.impactEffectNonEnemy, typeof(ParticleSystem), true);
            weapon.impactEffectEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (On Enemy)", "Particle effect played on hitting an enemy."), weapon.impactEffectEnemy, typeof(ParticleSystem), true);

            EditorGUILayout.Space();
            GUILayout.Label("Feedback & Feel", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Camera settings");
            weapon.cameraShakeIntensity = EditorGUILayout.FloatField(new GUIContent("Camera Shake Intensity", "Strength of camera shake on hit."), weapon.cameraShakeIntensity);
            weapon.cameraShakeDuration = EditorGUILayout.FloatField(new GUIContent("Camera Shake Duration", "Time in seconds camera shakes."), weapon.cameraShakeDuration);
            EditorGUILayout.Space();
            GUILayout.Label("Controller vibration settings");
            weapon.rumbleLeftFrequency = EditorGUILayout.Slider(new GUIContent("Rumble Left Freq", "Controller left motor rumble frequency (0-1)."), weapon.rumbleLeftFrequency, 0f, 1f);
            weapon.rumbleRightFrequency = EditorGUILayout.Slider(new GUIContent("Rumble Right Freq", "Controller right motor rumble frequency (0-1)."), weapon.rumbleRightFrequency, 0f, 1f);
            weapon.rumbleTime = EditorGUILayout.FloatField(new GUIContent("Rumble Time", "Controller rumble duration in seconds."), weapon.rumbleTime);
        }
        else
        {
            GUILayout.Label("Gun Settings", EditorStyles.boldLabel);
            weapon.fireMode = (FireMode)EditorGUILayout.EnumPopup(new GUIContent("Fire Mode", "Determines how the weapon fires (e.g., Automatic, Semi-Automatic)."), weapon.fireMode);
            weapon.fireRate = EditorGUILayout.FloatField(new GUIContent("Fire Rate", "The rate of fire of the weapon."), weapon.fireRate);
            weapon.range = EditorGUILayout.FloatField(new GUIContent("Fire Range", "The maximum distance the bullet can travel."), weapon.range);
            weapon.damageToEnemy = EditorGUILayout.FloatField(new GUIContent("Damage to Enemy", "Health removed from the enemy on hit."), weapon.damageToEnemy);
            weapon.raysPerShot = EditorGUILayout.IntField(new GUIContent("Bullets per shot", "Number of bullets fired in a single shot (e.g., for shotguns)."), weapon.raysPerShot);
            weapon.aoeSpread = EditorGUILayout.FloatField(new GUIContent("Area of Effect Spread", "Spread radius upon impact or multiple rays. Can be adjustable via scene visually with gizmos."), weapon.aoeSpread);
            weapon.maxAmmo = EditorGUILayout.IntField(new GUIContent("Maximum Ammo", "Total ammunition capacity in a clip."), weapon.maxAmmo);
            weapon.reloadTime = EditorGUILayout.FloatField(new GUIContent("Reload Time", "Time in seconds required to reload."), weapon.reloadTime);
            weapon.recoilForce = EditorGUILayout.FloatField(new GUIContent("Recoil Force", "The upward recoil force applied to the camera when shooting."), weapon.recoilForce);

            EditorGUILayout.Space();
            GUILayout.Label("Modifiers", EditorStyles.boldLabel);
            weapon.healthModifier = EditorGUILayout.FloatField(new GUIContent("Player Health Modifier", "Alters player max health while equipped (+ number increases health by that amount, - decreases it)."), weapon.healthModifier);
            weapon.speedModifier = EditorGUILayout.FloatField(new GUIContent("Player Speed Modifier", "Alters player movement speed while equipped (< 1 decreases the speed, > 1 increases it)."), weapon.speedModifier);

            EditorGUILayout.Space();
            GUILayout.Label("Visuals & Effects", EditorStyles.boldLabel);
            weapon.bulletTrail = (TrailRenderer)EditorGUILayout.ObjectField(new GUIContent("Bullet Trail", "The trail effect left by bullets."), weapon.bulletTrail, typeof(TrailRenderer), true);
            weapon.trailSpawnPos = (Transform)EditorGUILayout.ObjectField(new GUIContent("Bullet Trail Spawn Pos", "Where the bullet trail starts."), weapon.trailSpawnPos, typeof(Transform), true);
            weapon.muzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Muzzle flash effect", "Effect played at the barrel when firing."), weapon.muzzleFlash, typeof(ParticleSystem), true);
            //weapon.gunAnimator = (Animator)EditorGUILayout.ObjectField(new GUIContent("Gun Animator", "Animator used to play shooting animations on this weapon."), weapon.gunAnimator, typeof(Animator), true);
            weapon.impactPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Bullet impact prefab", "Object spawned when a bullet hits."), weapon.impactPrefab, typeof(GameObject), true);
            weapon.impactEffectNonEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (Not on enemy)", "Particle effect played on hitting the environment."), weapon.impactEffectNonEnemy, typeof(ParticleSystem), true);
            weapon.impactEffectEnemy = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Impact (On Enemy)", "Particle effect played on hitting an enemy."), weapon.impactEffectEnemy, typeof(ParticleSystem), true);

            EditorGUILayout.Space();
            GUILayout.Label("Feedback & Feel", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Camera settings");
            weapon.cameraShakeIntensity = EditorGUILayout.FloatField(new GUIContent("Camera Shake Intensity", "Strength of camera shake on hit."), weapon.cameraShakeIntensity);
            weapon.cameraShakeDuration = EditorGUILayout.FloatField(new GUIContent("Camera Shake Duration", "Time in seconds camera shakes."), weapon.cameraShakeDuration);
            
            EditorGUILayout.Space();
            GUILayout.Label("Controller vibration settings");
            weapon.rumbleLeftFrequency = EditorGUILayout.Slider(new GUIContent("Rumble Left Freq", "Controller left motor rumble frequency (0-1)."), weapon.rumbleLeftFrequency, 0f, 1f);
            weapon.rumbleRightFrequency = EditorGUILayout.Slider(new GUIContent("Rumble Right Freq", "Controller right motor rumble frequency (0-1)."), weapon.rumbleRightFrequency, 0f, 1f);
            weapon.rumbleTime = EditorGUILayout.FloatField(new GUIContent("Rumble Time", "Controller rumble duration in seconds."), weapon.rumbleTime);
        }

        EditorGUILayout.Space();

        base.OnInspectorGUI();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Save Changes"))
            {
                SaveToPrefab(weapon.gameObject);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(weapon);
        }
    }

    private void SaveToPrefab(GameObject instance)
    {
        GameObject originalPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);

        if (originalPrefab == null)
        {
            string cleanName = instance.name.Replace("(Clone)", "").Trim();
            string[] guids = AssetDatabase.FindAssets($"{cleanName} t:GameObject", new[] { "Assets/Prefabs/Weapons" });

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        if (originalPrefab != null)
        {
            Weapon instanceWeapon = instance.GetComponent<Weapon>();
            Weapon prefabWeapon = originalPrefab.GetComponent<Weapon>();

            if (instanceWeapon != null && prefabWeapon != null)
            {
                EditorUtility.CopySerialized(instanceWeapon, prefabWeapon);
                EditorUtility.SetDirty(originalPrefab);
                AssetDatabase.SaveAssets();

                Debug.Log($"Weapon changes successfully saved to prefab: {originalPrefab.name}");
            }
        }
        else
        {
            Debug.LogWarning("GameObject is not connected to any prefab and could not be found in Assets/Prefabs/Weapons.");
        }
    }
}
