using UnityEngine;
using System.Collections.Generic;

public class ParticleCollisionSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (ps == null || prefabToSpawn == null) return;

        int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 pos = collisionEvents[i].intersection;
            Vector3 normal = collisionEvents[i].normal;

            if (normal != Vector3.zero)
            {
                // Align with normal and add a random Z rotation
                Quaternion rot = Quaternion.LookRotation(-normal) * Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                Instantiate(prefabToSpawn, pos, rot);
            }
        }
    }
}
