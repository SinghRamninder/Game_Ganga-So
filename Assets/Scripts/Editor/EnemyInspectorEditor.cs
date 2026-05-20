using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Enemy))]
public class EnemyInspectorEditor : Editor
{
    private string[] shootOptions = { "Bullet", "Projectile" };
    private int index = 0;

    public override void OnInspectorGUI()
    {
        Enemy enemy = (Enemy)target;

        enemy.canShoot = EditorGUILayout.Toggle("Can Shoot?", enemy.canShoot);
        if (enemy.canShoot)
        {
            int newIndex = EditorGUILayout.Popup(new GUIContent("What?"), index, shootOptions);
            if (newIndex != index)
            {
                index = newIndex;
            }

            if (shootOptions[index] == "Projectile")
            {
                enemy.canShootProjectile = true;
                enemy.projectile = (Projectile)EditorGUILayout.ObjectField("Projectile", enemy.projectile, typeof(Projectile), true);
            }
            else if (shootOptions[index] == "Bullet")
            {
                enemy.canShootProjectile = false;
                enemy.bulletTrail = (BulletTrails)EditorGUILayout.ObjectField("Bullet Trail", enemy.bulletTrail, typeof(BulletTrails), true);
            }
        }

        EditorGUILayout.Space(10);

        base.OnInspectorGUI();
    }
}
