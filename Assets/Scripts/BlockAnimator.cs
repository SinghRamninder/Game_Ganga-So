using UnityEngine;

[ExecuteAlways]
public class BlockAnimator : MonoBehaviour
{
    private void OnValidate()
    {
        RemoveAnimator();
    }

    private void Awake()
    {
        RemoveAnimator();
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            CopyTransformValues();
        }
    }

    private void RemoveAnimator()
    {
        Animator anim = GetComponent<Animator>();

        if (anim != null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && anim != null)
                {
                    DestroyImmediate(anim, true);
                    Debug.LogWarning("Animator is not allowed on this gameobject.");
                }
            };
#else
            Destroy(anim);
            Debug.LogWarning("Animator is not allowed on this gameobject.");
#endif
        }
    }

    private void CopyTransformValues()
    {
        Transform weaponTransform = GetComponent<Transform>();
        Weapon weapon = GetComponent<Weapon>();

        if (weaponTransform != null && weapon != null)
        {
            weapon.weaponSpawnPosition = weaponTransform.localPosition;
            weapon.weaponSpawnRotation = weaponTransform.localRotation;
            weapon.weaponSpawnScale = weaponTransform.localScale;
        }
    }
}
