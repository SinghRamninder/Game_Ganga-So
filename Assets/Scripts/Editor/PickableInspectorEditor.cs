using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pickables))]
public class PickableInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Pickables pickables = (Pickables)target;

        pickables.Type = (PickableType)EditorGUILayout.EnumPopup(new GUIContent("Pickable Type"), pickables.Type);

        if (pickables.Type == PickableType.Weapon)
        {
            pickables.weaponToEquip = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Weapon to equip"), pickables.weaponToEquip, typeof(GameObject), true);
        }
        else if (pickables.Type == PickableType.HealthPack)
        {
            pickables.healAmount = EditorGUILayout.IntField(new GUIContent("Heal Amount"), pickables.healAmount);
        }

        EditorGUILayout.Space(10);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(pickables);
        }

        base.OnInspectorGUI();
    }
}
