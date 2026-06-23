using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ExplosiveBarrels : MonoBehaviour
{
    [Header("Explosion Settings")]
    public ParticleSystem explosionParticlePrefab;
    public float maxExplosiveImpactArea = 5f;
    public float maxDamageToEnemy = 50f;
    public float maxDamageToPlayer = 40f;

    [Header("Splatter Settings")]
    public ParticleSystem splatterParticlePrefab;
    public Color splatterColor = Color.white;

    [Header("FMOD")]
    public EventReference explosionEventFmod;

    private bool hasExploded = false;

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (!explosionEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(explosionEventFmod, gameObject);
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        if (explosionParticlePrefab != null)
        {
            ParticleSystem explosion = Instantiate(explosionParticlePrefab, transform.position + new Vector3(0f, 5f, 0f), Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
        }

        if (splatterParticlePrefab != null)
        {
            ParticleSystem splatter = Instantiate(splatterParticlePrefab, transform.position + new Vector3(0f, 5f, 0f), Quaternion.identity);

            var psr = splatter.main;
            psr.startColor = splatterColor;

            splatter.Play();
            Destroy(splatter.gameObject, splatter.main.duration + splatter.main.startLifetime.constantMax);
        }

        ApplyDamage();

        Destroy(gameObject);
    }

    private void ApplyDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxExplosiveImpactArea);
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
        bool playerDamaged = false;

        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float damageMultiplier = 1f - Mathf.Clamp01(distance / maxExplosiveImpactArea);
                enemy.TakeDamage(maxDamageToEnemy * damageMultiplier);
                damagedEnemies.Add(enemy);
            }

            if (!playerDamaged && (col.CompareTag("Player") || col.GetComponentInParent<PlayerHealthManager>() != null))
            {
                if (PlayerHealthManager.instance != null)
                {
                    float distance = Vector3.Distance(transform.position, PlayerHealthManager.instance.transform.position);
                    if (distance <= maxExplosiveImpactArea)
                    {
                        float damageMultiplier = 1f - Mathf.Clamp01(distance / maxExplosiveImpactArea);
                        PlayerHealthManager.instance.TakeDamage(maxDamageToPlayer * damageMultiplier);
                    }
                    playerDamaged = true;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxExplosiveImpactArea);
    }
}
