using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pickables))]
public class PickableInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Pickables pickables = (Pickables)target;

        serializedObject.Update();

        SerializedProperty typeProperty = serializedObject.FindProperty("Type");
        SerializedProperty weaponProperty = serializedObject.FindProperty("weaponToEquip");
        SerializedProperty healAmountProperty = serializedObject.FindProperty("healAmount");
        SerializedProperty keyCardIDProperty = serializedObject.FindProperty("keyCardID");
        SerializedProperty keyCardSpriteProperty = serializedObject.FindProperty("keyCardSprite");

        EditorGUILayout.PropertyField(typeProperty, new GUIContent("Pickable Type"));

        PickableType selectedType = (PickableType)typeProperty.enumValueIndex;

        if (selectedType == PickableType.Weapon)
        {
            EditorGUILayout.PropertyField(weaponProperty, new GUIContent("Weapon to Equip"));
        }
        else if (selectedType == PickableType.KeyCard)
        {
            EditorGUILayout.PropertyField(keyCardIDProperty, new GUIContent("Key Card ID"));
            EditorGUILayout.PropertyField(keyCardSpriteProperty, new GUIContent("Key Card Sprite"));
        }
        else if (selectedType == PickableType.HealthPack)
        {
            EditorGUILayout.PropertyField(healAmountProperty, new GUIContent("Heal Amount"));
        }

        EditorGUILayout.Space(10);

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "Type",
            "weaponToEquip",
            "healAmount",
            "keyCardID",
            "keyCardSprite"
        );

        serializedObject.ApplyModifiedProperties();
    }
}