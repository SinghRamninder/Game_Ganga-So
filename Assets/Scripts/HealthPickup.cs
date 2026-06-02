using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealthManager.instance.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
