using UnityEditor;
using UnityEngine;

public class WeaponFolderWatcher : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        UpdateAllInventories();
    }

    static void UpdateAllInventories()
    {
        var inventories = Object.FindObjectsOfType<WeaponsInventory>();

        foreach (var inv in inventories)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Weapons" });

            inv.weaponPrefabs = new GameObject[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                inv.weaponPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            EditorUtility.SetDirty(inv);
        }
    }
}