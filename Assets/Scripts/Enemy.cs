using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [HideInInspector] public bool canShoot = false;
    [HideInInspector] public bool canShootProjectile = false;
    [HideInInspector] public Projectile projectile;
    public float shootRange = 20f;
    public float maxHealth = 100f;
    public float speed = 3f;
    public float damageToPlayer = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;

    [Header("Effects")]
    public ParticleSystem deathEffect;
    [HideInInspector] public BulletTrails bulletTrail;

    [Header("Animations")]
    public Animator animator;
    public string walkAnimString = "Walk";
    public string attackAnimString = "Attack";
    public string deathAnimString = "Death";

    private float currentHealth;
    private Transform playerTransform;
    private bool isDead = false;
    private float nextAttackTime = 0f;
    private Camera mainCam;

    private void Start()
    {
        currentHealth = maxHealth;
        mainCam = Camera.main;

        if (PlayerHealthManager.instance != null)
        {
            playerTransform = PlayerHealthManager.instance.transform;
        }

        PlayAnimation(walkAnimString);
    }

    private void Update()
    {
        if (isDead) return;

        if (mainCam != null)
        {
            Vector3 lookDirection = transform.position - mainCam.transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
        moveDirection.y = 0;

        if (!canShoot)
        {
            transform.position += moveDirection * speed * Time.deltaTime;

            if (distance <= attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + (1f / attackRate);
                }
            }
        }
        else
        {
            if (distance > shootRange)
            {
                transform.position += moveDirection * speed * Time.deltaTime;
            }
            else
            {
                if (Time.time >= nextAttackTime)
                {
                    Shoot(moveDirection);
                    nextAttackTime = Time.time + (1f / attackRate);
                }
            }
        }
    }

    private void Attack()
    {
        PlayAnimation(attackAnimString);

        if (PlayerHealthManager.instance != null)
        {
            PlayerHealthManager.instance.TakeDamage(damageToPlayer);
        }

        Invoke(nameof(ReturnToWalk), 1f / attackRate * 0.5f);
    }

    private void Shoot(Vector3 shootDirection)
    {
        if (Physics.Raycast(transform.position, shootDirection, out RaycastHit hit, shootRange))
        {
            Vector3 maxRangePoint = transform.position + (shootDirection * shootRange);

            if (hit.collider.CompareTag("Player"))
            {
                if (!canShootProjectile)
                {
                    StartCoroutine(SpawnTracer(transform.position, maxRangePoint));
                }
                else
                {
                    StartCoroutine(SpawnProjectile(transform.position, maxRangePoint));
                }
            }
        }
    }

    private void ReturnToWalk()
    {
        if (!isDead) PlayAnimation(walkAnimString);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        PlayAnimation(deathAnimString);

        if (deathEffect != null)
        {
            ParticleSystem ps = Instantiate(deathEffect, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        float delay = (animator != null && animator.enabled) ? 1.5f : 0f;
        Destroy(gameObject, delay);
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null && animator.isActiveAndEnabled && !string.IsNullOrEmpty(animName))
        {
            animator.Play(animName);
        }
    }

    private IEnumerator SpawnTracer(Vector3 spawnPos, Vector3 targetPos)
    {
        if (bulletTrail == null)
            yield return null;

        Vector3 startPos = Vector3.zero;

        float time = 0;

        BulletTrails bulletTrailSpawned = Instantiate(bulletTrail, spawnPos, Quaternion.identity);

        if (bulletTrailSpawned != null)
        {
            bulletTrailSpawned.damageToPlayer = damageToPlayer;
            startPos = bulletTrailSpawned.transform.position;
        }

        TrailRenderer spawnedTrail = bulletTrailSpawned.GetComponent<TrailRenderer>();

        while (time < 1)
        {
            if (bulletTrailSpawned == null) break;

            bulletTrailSpawned.transform.position = Vector3.Lerp(startPos, targetPos, time);
            time += Time.deltaTime / spawnedTrail.time;
            yield return null;
        }

        if (bulletTrailSpawned != null)
        {
            bulletTrailSpawned.transform.position = targetPos;
            Destroy(bulletTrailSpawned.gameObject);
        }
    }

    // Temporary (Can be merged with SpawnTracer)
    private IEnumerator SpawnProjectile(Vector3 spawnPos, Vector3 targetPos)
    {
        if (projectile == null)
            yield return null;

        Vector3 startPos = Vector3.zero;

        Projectile projectileSpawned = Instantiate(projectile, spawnPos, Quaternion.identity);

        if (projectileSpawned != null)
        {
            projectileSpawned.damageToPlayer = damageToPlayer;
            startPos = projectileSpawned.transform.position;
        }

        float elapsed = 0;
        float duration = 3f;

        while (elapsed < duration)
        {
            if (projectileSpawned == null) break;

            float t = elapsed / duration;

            projectileSpawned.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (projectileSpawned != null)
        {
            projectileSpawned.transform.position = targetPos;
            Destroy(projectileSpawned.gameObject);
        }
    }
    // Temporary (Can be merged with SpawnTracer)
}
