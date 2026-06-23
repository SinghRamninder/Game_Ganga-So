using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosiveServers : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Explosion Settings")]
    public ParticleSystem blastParticlePrefab;
    public ParticleSystem smokeParticlePrefab;
    public Vector3 smokeSpawnOffset = new Vector3(0, 5f, 0);
    public ParticleSystem sparkParticlePrefab;
    public Vector3 sparkSpawnOffset = Vector3.zero;
    public Vector3 explosionEpicenterOffset = Vector3.zero;
    
    public float maxExplosiveImpactArea = 5f;
    public float maxDamageToEnemy = 50f;
    public float maxDamageToPlayer = 40f;
    public float maxDamageToServer = 50f;
    public LayerMask environmentLayerMask;

    private bool hasExploded = false;
    private List<Material> glowMaterials = new List<Material>();
    private Color originalEmissionColor;
    private float originalIntensity;
    private bool isFlickering = false;
    private float flickerThreshold;

    void Start()
    {
        currentHealth = maxHealth;
        flickerThreshold = maxHealth * Random.Range(0.7f, 0.8f);

        Transform[] children = GetComponentsInChildren<Transform>(true);
        bool colorCaptured = false;
        foreach (Transform child in children)
        {
            if (child.name == "Glow")
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        glowMaterials.Add(mat);
                        if (!colorCaptured)
                        {
                            originalEmissionColor = mat.GetColor("_EmissionColor");
                            originalIntensity = Mathf.Max(originalEmissionColor.r, originalEmissionColor.g, originalEmissionColor.b);
                            if (originalIntensity == 0) originalIntensity = 1f;
                            colorCaptured = true;
                        }
                    }
                }
            }
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (hasExploded) return;

        currentHealth -= damage;

        if (sparkParticlePrefab != null)
        {
            ParticleSystem spark = Instantiate(sparkParticlePrefab, hitPoint + sparkSpawnOffset, Quaternion.LookRotation(-hitNormal));
            var emission = spark.emission;
            emission.rateOverTime = 0;
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, 10, 20, 1, 0.01f);
            burst.probability = 1f;
            emission.SetBursts(new ParticleSystem.Burst[] { burst });
            var main = spark.main;
            main.loop = false;
            
            spark.Play();
            Destroy(spark.gameObject, main.duration + main.startLifetime.constantMax);
        }

        UpdateGlow();

        if (currentHealth <= flickerThreshold && !isFlickering)
        {
            StartCoroutine(FlickerRoutine());
        }

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    private void UpdateGlow()
    {
        if (glowMaterials.Count > 0 && !isFlickering && currentHealth > 0)
        {
            float intensityMultiplier = Mathf.Clamp01(currentHealth / maxHealth);
            foreach (Material mat in glowMaterials)
            {
                mat.SetColor("_EmissionColor", originalEmissionColor * intensityMultiplier);
            }
        }
    }

    private IEnumerator FlickerRoutine()
    {
        isFlickering = true;
        while (currentHealth > 0 && currentHealth <= flickerThreshold)
        {
            if (glowMaterials.Count > 0)
            {
                float randomIntensity = Random.Range(0f, Mathf.Clamp01(currentHealth / maxHealth));
                foreach (Material mat in glowMaterials)
                {
                    mat.SetColor("_EmissionColor", originalEmissionColor * randomIntensity);
                }
            }
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));
        }
        isFlickering = false;
        UpdateGlow();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 epicenter = transform.position + explosionEpicenterOffset;

        foreach (Material mat in glowMaterials)
        {
            mat.SetColor("_EmissionColor", Color.black);
        }

        if (blastParticlePrefab != null)
        {
            ParticleSystem blast = Instantiate(blastParticlePrefab, epicenter, Quaternion.identity);
            blast.Play();
            Destroy(blast.gameObject, blast.main.duration + blast.main.startLifetime.constantMax);
        }

        if (smokeParticlePrefab != null)
        {
            ParticleSystem smoke = Instantiate(smokeParticlePrefab, transform.position + smokeSpawnOffset, Quaternion.identity);
            smoke.Play();
        }

        if (sparkParticlePrefab != null)
        {
            int sparkCount = Random.Range(8, 12);
            Collider col = GetComponent<Collider>();
            for (int i = 0; i < sparkCount; i++)
            {
                Vector3 randomPos = transform.position + sparkSpawnOffset;
                if (col is BoxCollider boxCol)
                {
                    randomPos = transform.TransformPoint(GetRandomPointOnBoxSurface(boxCol)) + sparkSpawnOffset;
                }
                else if (col != null)
                {
                    randomPos = new Vector3(
                        Random.Range(col.bounds.min.x, col.bounds.max.x),
                        Random.Range(col.bounds.min.y, col.bounds.max.y),
                        Random.Range(col.bounds.min.z, col.bounds.max.z)
                    ) + sparkSpawnOffset;
                }
                ParticleSystem spark = Instantiate(sparkParticlePrefab, randomPos, Quaternion.identity);
                var main = spark.main;
                main.loop = true;
                var emission = spark.emission;
                emission.rateOverTime = 0;
                
                ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, 10, 20, 2, 2f);
                burst.probability = 0.30f;
                emission.SetBursts(new ParticleSystem.Burst[] { burst });
                
                spark.Play();
            }
        }

        ApplyDamage();
        this.enabled = false; 
    }

    private void ApplyDamage()
    {
        Vector3 epicenter = transform.position + explosionEpicenterOffset;
        Collider[] colliders = Physics.OverlapSphere(epicenter, maxExplosiveImpactArea);
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
        bool playerDamaged = false;

        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            
            Vector3 targetPos = col.bounds.center;
            Vector3 dir = targetPos - epicenter;
            if (Physics.Raycast(epicenter, dir.normalized, out RaycastHit hit, dir.magnitude, environmentLayerMask))
            {
                continue;
            }

            float distance = Vector3.Distance(epicenter, col.transform.position);
            float damageMultiplier = 1f - Mathf.Clamp01(distance / maxExplosiveImpactArea);

            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                enemy.TakeDamage(maxDamageToEnemy * damageMultiplier);
                damagedEnemies.Add(enemy);
            }

            if (!playerDamaged && (col.CompareTag("Player") || col.GetComponentInParent<PlayerHealthManager>() != null))
            {
                if (PlayerHealthManager.instance != null)
                {
                    PlayerHealthManager.instance.TakeDamage(maxDamageToPlayer * damageMultiplier);
                    playerDamaged = true;
                }
            }

            ExplosiveServers server = col.GetComponent<ExplosiveServers>();
            if (server != null)
            {
                server.TakeDamage(maxDamageToServer * damageMultiplier, epicenter, (server.transform.position - epicenter).normalized);
            }

            ExplosiveBarrels barrel = col.GetComponent<ExplosiveBarrels>();
            if (barrel != null)
            {
                barrel.Explode();
            }
        }
    }

    private Vector3 GetRandomPointOnBoxSurface(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );

        int face = Random.Range(0, 6);
        switch (face)
        {
            case 0: point.x = extents.x; break;
            case 1: point.x = -extents.x; break;
            case 2: point.y = extents.y; break;
            case 3: point.y = -extents.y; break;
            case 4: point.z = extents.z; break;
            case 5: point.z = -extents.z; break;
        }

        return point + boxCollider.center;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 epicenter = transform.position + explosionEpicenterOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(epicenter, maxExplosiveImpactArea);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position + smokeSpawnOffset, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + sparkSpawnOffset, 0.3f);
    }
}
