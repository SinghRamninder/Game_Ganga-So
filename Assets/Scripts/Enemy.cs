using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [HideInInspector] public bool canShoot = false;
    [HideInInspector] public bool canShootProjectile = false;
    [HideInInspector] public Projectile projectile;
    [HideInInspector] public float shootRange = 20f;
    [HideInInspector] public float bulletSpeed = 40f;
    [HideInInspector] public float projectileSpeed = 15f;
    public float rayShootOfset = 5f;
    public float maxHealth = 100f;
    public float speed = 3f;
    public float damageToPlayer = 10f;
    [HideInInspector] public float attackRange = 1.5f;
    [HideInInspector] public float attackRate = 1f;
    public bool moveTowardsLastPlayerPosition = true;

    [Header("Effects")]
    public ParticleSystem deathEffect;
    [HideInInspector] public BulletTrails bulletTrail;

    [Header("Animations")]
    public Animator animator;
    public string walkAnimString = "Walk";
    private bool walkAnimPlaying = false;
    public string attackAnimString = "Attack";
    public string deathAnimString = "Death";
    private bool isIdle = true;

    private float currentHealth;
    private Transform playerTransform;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private Camera mainCam;

    private Vector3 playerLastPosition;
    private bool firstHitWithPlayer = false;

    private void Start()
    {
        playerLastPosition = transform.position;
        currentHealth = maxHealth;
        mainCam = Camera.main;

        if (PlayerHealthManager.instance != null)
        {
            playerTransform = PlayerHealthManager.instance.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
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

        Vector3 rayShootPos = new Vector3(transform.position.x, transform.position.y + rayShootOfset, transform.position.z);
        Vector3 rayShootDirection = (playerTransform.position - rayShootPos).normalized;

        //Debug.DrawRay(rayShootPos, rayShootDirection * 100f, Color.red);
        
        if (Physics.Raycast(rayShootPos, rayShootDirection, out RaycastHit hit, 200f, Physics.AllLayers))
        {
            if (hit.collider.CompareTag("Player"))
            {
                playerLastPosition = hit.collider.transform.position;
                playerLastPosition.y = transform.position.y;

                firstHitWithPlayer = true;

                if (!canShoot)
                {
                    if (!isAttacking)
                    {
                        transform.position += moveDirection * speed * Time.deltaTime;
                    }

                    if (distance <= attackRange)
                    {
                        if (Time.time >= nextAttackTime && !isAttacking)
                        {
                            StartCoroutine(Attack());
                            nextAttackTime = Time.time + (1f / attackRate);
                        }
                    }
                    else if(!walkAnimPlaying && !isAttacking)
                    {
                        ReturnToWalk();
                    }
                }
                else
                {
                    if (distance > shootRange)
                    {
                        transform.position += moveDirection * speed * Time.deltaTime;

                        if (!walkAnimPlaying)
                        {
                            ReturnToWalk();
                        }
                    }
                    else
                    {
                        if (!isIdle)
                        {
                            ReturnToIdle();
                        }

                        if (Time.time >= nextAttackTime)
                        {
                            Shoot(rayShootPos, rayShootDirection);
                            nextAttackTime = Time.time + (1f / attackRate);
                        }
                    }
                }
            }
            else if (moveTowardsLastPlayerPosition && firstHitWithPlayer)
            {
                Vector3 newMoveDirection = (playerLastPosition - transform.position).normalized;
                newMoveDirection.y = 0;

                if (Vector3.Distance(transform.position, playerLastPosition) > 0.1f)
                {
                    transform.position += newMoveDirection * speed * Time.deltaTime;

                    if (!walkAnimPlaying)
                    {
                        ReturnToWalk();
                    }
                }
                else if (!isIdle)
                {
                    ReturnToIdle();
                }
            }
        }
        else if (moveTowardsLastPlayerPosition && firstHitWithPlayer)
        {
            Vector3 newMoveDirection = (playerLastPosition - transform.position).normalized;
            newMoveDirection.y = 0;

            if (Vector3.Distance(transform.position, playerLastPosition) > 0.1f)
            {
                transform.position += newMoveDirection * speed * Time.deltaTime;

                if (!walkAnimPlaying)
                {
                    ReturnToWalk();
                }
            }
            else if (!isIdle)
            {
                ReturnToIdle();
            }
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        PlayAnimation(attackAnimString);

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        yield return new WaitForSeconds(clipInfo[0].clip.length);

        if (PlayerHealthManager.instance != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= attackRange)
            {
                PlayerHealthManager.instance.TakeDamage(damageToPlayer);
            }
        }

        yield return new WaitForSeconds(1f / clipInfo[0].clip.length * 0.5f);

        isAttacking = false;
        ReturnToWalk();
    }

    private void Shoot(Vector3 shootPos, Vector3 shootDirection)
    {
        PlayAnimation(attackAnimString);

        if (Physics.Raycast(shootPos, shootDirection, out RaycastHit hit, shootRange))
        {
            Vector3 maxRangePoint = shootPos + (shootDirection * shootRange);

            if (hit.collider.CompareTag("Player"))
            {
                if (!canShootProjectile)
                {
                    StartCoroutine(SpawnTracer(shootPos, maxRangePoint));
                }
                else
                {
                    StartCoroutine(SpawnProjectile(shootPos, maxRangePoint));
                }
            }
        }
    }

    private void ReturnToWalk()
    {
        walkAnimPlaying = true;
        isIdle = false;

        if (!isDead) PlayAnimation(walkAnimString);
    }

    private void ReturnToIdle()
    {
        walkAnimPlaying = false;
        isIdle = true;

        if (!isDead) PlayAnimation("Default");
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
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<CapsuleCollider>());

        PlayAnimation(deathAnimString);

        if (deathEffect != null)
        {
            ParticleSystem ps = Instantiate(deathEffect, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        float delay = (animator != null && animator.enabled) ? clipInfo[0].clip.length : 0f;
        Destroy(gameObject, delay);
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null && animator.isActiveAndEnabled && !string.IsNullOrEmpty(animName))
        {
            animator.Play(animName);
            animator.Update(0f);
        }
    }

    private IEnumerator SpawnTracer(Vector3 spawnPos, Vector3 targetPos)
    {
        if (bulletTrail == null)
            yield break;

        BulletTrails bulletTrailSpawned = Instantiate(bulletTrail, spawnPos, Quaternion.identity);

        if (bulletTrailSpawned == null)
            yield break;

        bulletTrailSpawned.damageToPlayer = damageToPlayer;

        Rigidbody rb = bulletTrailSpawned.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("BulletTrail needs a Rigidbody for physics movement.");
            Destroy(bulletTrailSpawned.gameObject);
            yield break;
        }

        while (bulletTrailSpawned != null)
        {
            Vector3 newPosition = Vector3.MoveTowards(
                rb.position,
                targetPos,
                bulletSpeed * Time.fixedDeltaTime
            );

            rb.MovePosition(newPosition);

            if (Vector3.Distance(newPosition, targetPos) <= 0.1f)
            {
                Destroy(bulletTrailSpawned.gameObject);
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    // Temporary (Can be merged with SpawnTracer)
    private IEnumerator SpawnProjectile(Vector3 spawnPos, Vector3 targetPos)
    {
        if (projectile == null)
            yield break;

        Projectile projectileSpawned = Instantiate(projectile, spawnPos, Quaternion.identity);

        if (projectileSpawned == null)
            yield break;

        projectileSpawned.damageToPlayer = damageToPlayer;

        Rigidbody rb = projectileSpawned.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("Projectile needs a Rigidbody for physics movement.");
            Destroy(projectileSpawned.gameObject);
            yield break;
        }

        while (projectileSpawned != null)
        {
            Vector3 newPosition = Vector3.MoveTowards(
                rb.position,
                targetPos,
                projectileSpeed * Time.fixedDeltaTime
            );

            rb.MovePosition(newPosition);

            if (Vector3.Distance(newPosition, targetPos) <= 0.1f)
            {
                Destroy(projectileSpawned.gameObject);
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
    // Temporary (Can be merged with SpawnTracer)
}
