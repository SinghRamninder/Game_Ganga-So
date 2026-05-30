using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class BloodImpactProfile
{
    public int particleCount = 30;

    public ParticleSystem.MinMaxCurve speed = new ParticleSystem.MinMaxCurve(10f);

    [Range(0f, 1f)]
    public float spread = 0.25f;

    public ParticleSystem.MinMaxCurve gravityModifier = new ParticleSystem.MinMaxCurve(2f);
    public ParticleSystem.MinMaxCurve lifetime = new ParticleSystem.MinMaxCurve(0.5f);
    public ParticleSystem.MinMaxCurve size = new ParticleSystem.MinMaxCurve(1f);
}

public class ParticleCollisionSpawner : MonoBehaviour
{
    [HideInInspector] public GameObject prefabToSpawn;
    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    public int splatterNumber = 0;

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

            if (normal != Vector3.zero/* && IsFullyOnWall(pos, normal, 1.5f)*/)
            {
                //Quaternion rot = Quaternion.LookRotation(-normal) * Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                //GameObject spawnedSplat = Instantiate(prefabToSpawn, pos, rot);

                //MeshRenderer splatMesh = spawnedSplat.GetComponent<MeshRenderer>();
                //if (splatMesh != null)
                //{
                //    splatMesh.sortingLayerName = "Splatter";
                //    if (PlayerWeaponManager.instance != null)
                //        splatMesh.sortingOrder = PlayerWeaponManager.instance.weaponLayerNumber;

                //    Debug.Log(splatMesh.sortingLayerName);
                //    Debug.Log(splatMesh.sortingOrder);
                //}

                //if (PlayerWeaponManager.instance != null)
                //    PlayerWeaponManager.instance.weaponLayerNumber++;

                SplatterMeshManager.Instance.AddSplatter(splatterNumber, pos, normal);
            }
            else
            {
                Debug.Log("Not spawned!! Collision detected on Edge");
            }
        }
    }

    public void SpawnBlood(Vector3 bulletDirection, BloodImpactProfile profile)
    {
        bulletDirection.Normalize();

        ParticleSystem.MainModule main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = profile.gravityModifier;

        for (int i = 0; i < profile.particleCount; i++)
        {
            Vector3 spreadDirection = (bulletDirection + Random.insideUnitSphere * profile.spread).normalized;

            float speed = profile.speed.Evaluate(0f, Random.value);
            float size = profile.size.Evaluate(0f, Random.value);
            float lifetime = profile.lifetime.Evaluate(0f, Random.value);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position;
            emitParams.velocity = spreadDirection * speed;
            emitParams.startLifetime = lifetime;
            emitParams.startSize = size;

            ps.Emit(emitParams, 1);
        }

        Destroy(ps.gameObject, profile.lifetime.Evaluate(0f, Random.value));
    }


    bool IsFullyOnWall(Vector3 center, Vector3 normal, float size)
    {
        Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;

        if (right == Vector3.zero)
            right = Vector3.Cross(Vector3.forward, normal).normalized;

        Vector3 up = Vector3.Cross(normal, right);

        float half = size * 0.5f;

        Vector3[] corners =
        {
        center + right * half + up * half,
        center - right * half + up * half,
        center + right * half - up * half,
        center - right * half - up * half
    };

        foreach (Vector3 corner in corners)
        {
            Vector3 start = corner + normal * 0.05f;

            if (!Physics.Raycast(start, -normal, 0.1f))
            {
                return false;
            }
        }

        return true;
    }

}
