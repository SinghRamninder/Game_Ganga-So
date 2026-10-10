using UnityEngine;

public class AutoDisableEnemies : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        enemy.gameObject.SetActive(false);
        Debug.LogWarning($"AutoDisableEnemies: Disabled enemy {enemy.gameObject.name} on trigger enter.", enemy);
    }
}
