using UnityEngine;
using FMODUnity;

public class BulletTrails : MonoBehaviour
{
    [HideInInspector] public float damageToPlayer = 0f;

    [Header("FMOD")]
    public EventReference impactEventFmod;

    private void OnTriggerEnter(Collider collision)
    {
        if (!impactEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(impactEventFmod, gameObject);
        }

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
