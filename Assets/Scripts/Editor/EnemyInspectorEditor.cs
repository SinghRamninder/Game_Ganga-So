using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Enemy))]
public class EnemyInspectorEditor : Editor
{
    private string[] shootOptions = { "Bullet", "Projectile" };

    public override void OnInspectorGUI()
    {
        Enemy enemy = (Enemy)target;

        EditorGUI.BeginChangeCheck();

        enemy.canShoot = EditorGUILayout.Toggle("Can Shoot?", enemy.canShoot);
        if (enemy.canShoot)
        {
            int currentIndex = enemy.canShootProjectile ? 1 : 0;
            int newIndex = EditorGUILayout.Popup(new GUIContent("What?"), currentIndex, shootOptions);
            enemy.canShootProjectile = (newIndex == 1);

            if (enemy.canShootProjectile)
            {
                enemy.projectile = (Projectile)EditorGUILayout.ObjectField("Projectile", enemy.projectile, typeof(Projectile), true);
                enemy.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", enemy.projectileSpeed);
            }
            else
            {
                enemy.bulletTrail = (BulletTrails)EditorGUILayout.ObjectField("Bullet Trail", enemy.bulletTrail, typeof(BulletTrails), true);
                enemy.bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", enemy.bulletSpeed);
            }

            enemy.shootRange = EditorGUILayout.FloatField("Shoot Range", enemy.shootRange);
            enemy.attackRate = EditorGUILayout.FloatField("Shoot Rate", enemy.attackRate);
        }
        else
        {
            enemy.attackRange = EditorGUILayout.FloatField("Attack Range", enemy.attackRange);
            enemy.attackRate = EditorGUILayout.FloatField("Attack Rate", enemy.attackRate);
        }

        EditorGUILayout.Space(10);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(enemy);
            PrefabUtility.RecordPrefabInstancePropertyModifications(enemy);
        }

        base.OnInspectorGUI();
    }
}
