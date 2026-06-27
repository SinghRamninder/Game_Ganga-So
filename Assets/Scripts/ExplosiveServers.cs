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
    public Vector3 smokeSpawnScale = Vector3.one;
    public ParticleSystem sparkParticlePrefab;
    [HideInInspector] public Vector3 sparkSpawnOffset = Vector3.zero;
    [Tooltip("How far the explosion sparks are pushed out along the face normal so they sit just outside the surface.")]
    public float sparkSurfaceOffset = 0.1f;
    public Vector3 explosionEpicenterOffset = Vector3.zero;

    public float cameraShakeIntensity = 1f;
    public float cameraShakeDuration = 0.5f;
    
    [Tooltip("Explosion area size along each axis. Larger values affect more space on that axis.")]
    public Vector3 maxExplosiveImpactArea = new Vector3(5f, 5f, 5f);
    public float maxDamageToEnemy = 50f;
    public float maxDamageToPlayer = 40f;
    public float maxDamageToServer = 50f;
    public LayerMask environmentLayerMask;
    [HideInInspector] public Vector3 epicenter;

    [HideInInspector] public bool hasExploded = false;
    private List<Material> glowMaterials = new List<Material>();
    private Dictionary<Material, Color> baseEmissionColors = new Dictionary<Material, Color>();
    private Dictionary<Material, Color> expectedEmissionColors = new Dictionary<Material, Color>();
    private VideoScreen[] videoScreens;
    private bool isFlickering = false;
    private float flickerThreshold;
    private Coroutine flickerCoroutine;
    private List<GameObject> activeParticles = new List<GameObject>();

    void Start()
    {
        currentHealth = maxHealth;
        flickerThreshold = maxHealth * Random.Range(0.7f, 0.8f);
        epicenter = transform.position + explosionEpicenterOffset;

        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == "Glow")
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    if (mat.HasProperty("_EmissionColor") && !glowMaterials.Contains(mat))
                    {
                        glowMaterials.Add(mat);
                        baseEmissionColors[mat] = mat.GetColor("_EmissionColor");
                        expectedEmissionColors[mat] = baseEmissionColors[mat];
                    }
                }
            }
        }

        videoScreens = GetComponentsInChildren<VideoScreen>(true);
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (sparkParticlePrefab != null)
        {
            ParticleSystem spark = Instantiate(sparkParticlePrefab, hitPoint + sparkSpawnOffset, Quaternion.LookRotation(-hitNormal));
            activeParticles.Add(spark.gameObject);
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

        if (hasExploded) return;

        currentHealth -= damage;

        UpdateGlow();

        if (currentHealth <= flickerThreshold && !isFlickering)
        {
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }

        if (currentHealth <= 0)
        {
            Explode();
        }
    }

    private bool ColorsAreDifferent(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) > 0.05f || Mathf.Abs(a.g - b.g) > 0.05f || 
               Mathf.Abs(a.b - b.b) > 0.05f || Mathf.Abs(a.a - b.a) > 0.05f;
    }

    private void SyncBaseColors()
    {
        foreach (Material mat in glowMaterials)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                Color current = mat.GetColor("_EmissionColor");
                if (ColorsAreDifferent(current, expectedEmissionColors[mat]))
                {
                    // Calculate the base color as if current is divided by our last multiplier
                    // But if it was completely turned off or changed drastically, just take the new one directly
                    float diff = Mathf.Max(Mathf.Abs(current.r - expectedEmissionColors[mat].r), 
                                         Mathf.Max(Mathf.Abs(current.g - expectedEmissionColors[mat].g), 
                                                   Mathf.Abs(current.b - expectedEmissionColors[mat].b)));
                    if (diff > 0.1f)
                    {
                        baseEmissionColors[mat] = current;
                        expectedEmissionColors[mat] = current;
                    }
                }
            }
        }
    }

    private void ApplyColorMultiplier(float multiplier)
    {
        foreach (Material mat in glowMaterials)
        {
            Color newColor = baseEmissionColors[mat] * multiplier;
            mat.SetColor("_EmissionColor", newColor);
            expectedEmissionColors[mat] = newColor;
        }
    }

    public void RefreshEmissionColorReferences()
    {
        foreach (Material mat in glowMaterials)
        {
            if (mat != null && mat.HasProperty("_EmissionColor"))
            {
                Color current = mat.GetColor("_EmissionColor");
                baseEmissionColors[mat] = current;
                expectedEmissionColors[mat] = current;
            }
        }
    }

    private void UpdateGlow()
    {
        if (glowMaterials.Count > 0 && !isFlickering && currentHealth > 0)
        {
            SyncBaseColors();
            float intensityMultiplier = Mathf.Clamp01(currentHealth / maxHealth);
            ApplyColorMultiplier(intensityMultiplier);
        }
    }

    private IEnumerator FlickerRoutine()
    {
        isFlickering = true;
        while (currentHealth > 0 && currentHealth <= flickerThreshold)
        {
            if (glowMaterials.Count > 0)
            {
                SyncBaseColors();
                float randomIntensity = Random.Range(0f, Mathf.Clamp01(currentHealth / maxHealth));
                ApplyColorMultiplier(randomIntensity);
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

        if (videoScreens != null)
        {
            foreach (VideoScreen vs in videoScreens)
            {
                if (vs != null)
                {
                    vs.gameObject.SetActive(false);
                }
            }
        }

        if (blastParticlePrefab != null)
        {
            ParticleSystem blast = Instantiate(blastParticlePrefab, epicenter, Quaternion.identity);
            activeParticles.Add(blast.gameObject);
            blast.Play();
            Destroy(blast.gameObject, blast.main.duration + blast.main.startLifetime.constantMax);
        }

        if (smokeParticlePrefab != null)
        {
            ParticleSystem smoke = Instantiate(smokeParticlePrefab, transform.position + smokeSpawnOffset, Quaternion.Euler(-90f, 0f, 0f));
            activeParticles.Add(smoke.gameObject);
            smoke.transform.localScale = smokeSpawnScale;
            smoke.Play();
        }

        if (sparkParticlePrefab != null)
        {
            int sparkCount = Random.Range(8, 12);
            Collider col = GetComponent<Collider>();
            for (int i = 0; i < sparkCount; i++)
            {
                Vector3 randomPos = transform.position + sparkSpawnOffset;
                Quaternion sparkRotation = Quaternion.identity;
                if (col is BoxCollider boxCol)
                {
                    Vector3 localPoint = GetRandomPointOnBoxSurface(boxCol, out Vector3 localNormal);
                    Vector3 worldNormal = transform.TransformDirection(localNormal).normalized;
                    randomPos = transform.TransformPoint(localPoint) + worldNormal * sparkSurfaceOffset;
                    sparkRotation = Quaternion.LookRotation(worldNormal);
                }
                else if (col != null)
                {
                    randomPos = new Vector3(
                        Random.Range(col.bounds.min.x, col.bounds.max.x),
                        Random.Range(col.bounds.min.y, col.bounds.max.y),
                        Random.Range(col.bounds.min.z, col.bounds.max.z)
                    ) + sparkSpawnOffset;
                }
                ParticleSystem spark = Instantiate(sparkParticlePrefab, randomPos, sparkRotation);
                activeParticles.Add(spark.gameObject);
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

        if (SimplePlayerController.Instance != null)
        {
            SimplePlayerController.Instance.TriggerCameraShake(cameraShakeIntensity, cameraShakeDuration);
        }

        ApplyDamage();
        this.enabled = false; 
    }

    public void ResetServer()
    {
        this.enabled = true;
        hasExploded = false;
        currentHealth = maxHealth;

        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
        isFlickering = false;

        foreach (Material mat in glowMaterials)
        {
            mat.SetColor("_EmissionColor", baseEmissionColors[mat]);
            expectedEmissionColors[mat] = baseEmissionColors[mat];
        }

        foreach (GameObject p in activeParticles)
        {
            if (p != null)
            {
                Destroy(p);
            }
        }
        activeParticles.Clear();

        if (videoScreens != null)
        {
            foreach (VideoScreen vs in videoScreens)
            {
                if (vs != null)
                {
                    vs.gameObject.SetActive(true);
                }
            }
        }
    }

    private void ApplyDamage()
    {
        epicenter = transform.position + explosionEpicenterOffset;
        Vector3 halfExtents = maxExplosiveImpactArea * 0.5f;
        Collider[] colliders = Physics.OverlapBox(epicenter, halfExtents, Quaternion.identity);
        HashSet<Enemy> damagedEnemies = new HashSet<Enemy>();
        bool playerDamaged = false;

        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                float damageMultiplier = GetDamageMultiplier(epicenter, enemy.transform.position, maxExplosiveImpactArea);
                enemy.TakeDamage(maxDamageToEnemy * damageMultiplier);
                damagedEnemies.Add(enemy);
            }

            if (!playerDamaged && (col.CompareTag("Player") || col.GetComponentInParent<PlayerHealthManager>() != null))
            {
                if (PlayerHealthManager.instance != null)
                {
                    float damageMultiplier = GetDamageMultiplier(epicenter, PlayerHealthManager.instance.transform.position, maxExplosiveImpactArea);
                    PlayerHealthManager.instance.TakeDamage(maxDamageToPlayer * damageMultiplier);
                    playerDamaged = true;
                }
            }

            ExplosiveServers server = col.GetComponent<ExplosiveServers>();
            if (server != null)
            {
                float damageMultiplier = GetDamageMultiplier(epicenter, server.epicenter, maxExplosiveImpactArea);
                server.TakeDamage(maxDamageToServer * damageMultiplier, epicenter, (server.epicenter - epicenter).normalized);
            }

            ExplosiveBarrels barrel = col.GetComponent<ExplosiveBarrels>();
            if (barrel != null)
            {
                barrel.Explode();
            }
        }
    }

    private Vector3 GetRandomPointOnBoxSurface(BoxCollider boxCollider, out Vector3 localNormal)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );

        int face = Random.Range(0, 5);
        switch (face)
        {
            case 0: point.x = extents.x; localNormal = Vector3.right; break;
            case 1: point.x = -extents.x; localNormal = Vector3.left; break;
            case 2: point.y = extents.y; localNormal = Vector3.up; break;
            case 3: point.z = extents.z; localNormal = Vector3.forward; break;
            default: point.z = -extents.z; localNormal = Vector3.back; break;
        }

        return point + boxCollider.center;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 epicenter = transform.position + explosionEpicenterOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(epicenter, maxExplosiveImpactArea);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position + smokeSpawnOffset, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + sparkSpawnOffset, 0.3f);
    }

    private float GetDamageMultiplier(Vector3 center, Vector3 targetPosition, Vector3 areaSize)
    {
        Vector3 delta = targetPosition - center;
        float normalizedX = areaSize.x > 0f ? Mathf.Abs(delta.x) / areaSize.x : 0f;
        float normalizedY = areaSize.y > 0f ? Mathf.Abs(delta.y) / areaSize.y : 0f;
        float normalizedZ = areaSize.z > 0f ? Mathf.Abs(delta.z) / areaSize.z : 0f;
        float normalizedDistance = Mathf.Max(normalizedX, Mathf.Max(normalizedY, normalizedZ));
        return 1f - Mathf.Clamp01(normalizedDistance);
    }
}
