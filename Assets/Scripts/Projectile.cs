using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damageToPlayer = 0f;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collided with player " + damageToPlayer);

            if (PlayerHealthManager.instance != null)
            {
                PlayerHealthManager.instance.TakeDamage(damageToPlayer);
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Collided with something else: " + collision.gameObject.name);
            Destroy(gameObject);
        }
    }
}
