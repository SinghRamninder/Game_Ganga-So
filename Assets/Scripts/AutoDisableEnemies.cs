using UnityEngine;

public class AutoDisableEnemies : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            other.gameObject.SetActive(false);
            Debug.LogWarning($"AutoDisableEnemies: Disabled enemy {other.gameObject.name} on trigger enter.");
        }
    }
}
