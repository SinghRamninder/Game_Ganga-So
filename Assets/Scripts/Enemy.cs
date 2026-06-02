using System;
using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public Animator animator;
    [Header("Animations")]
    public string walkAnimString = "Walk";
    private bool walkAnimPlaying = false;
    public string attackAnimString = "Attack";
    public string deathAnimString = "Death";
    private bool isIdle = true;

    [Header("Sound settings")]
    public AudioClip walkSound;
    [Range(0f, 1f)] public float walkVolume = 1f;
    public AudioClip attackSound;
    [Range(0f, 1f)] public float attackVolume = 1f;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1f;
    public GameObject audioSource;

    private float currentHealth;
    private Transform playerTransform;
    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    private Camera mainCam;

    private Vector3 playerLastPosition;
    private bool firstHitWithPlayer = false;

    private List<Projectile> projectiles = new List<Projectile>();

    private Vector3 positionLastFrameForSound;
    private float nextWalkSoundTime = 0f;

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

        positionLastFrameForSound = transform.position;
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
        
        if (Physics.Raycast(rayShootPos, rayShootDirection, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
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

        CheckAndPlayWalkSound();
    }

    private void CheckAndPlayWalkSound()
    {
        bool isMoving = false;
        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 currentHorizontalPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastHorizontalPos = new Vector3(positionLastFrameForSound.x, 0, positionLastFrameForSound.z);

        if (rb != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > 0.1f)
            {
                isMoving = true;
            }
        }

        if (!isMoving && Vector3.Distance(currentHorizontalPos, lastHorizontalPos) > 0.001f)
        {
            isMoving = true;
        }

        if (isMoving && !isAttacking)
        {
            if (audioSource != null && walkSound != null && Time.time >= nextWalkSoundTime)
            {
                StartCoroutine(AudioManager.Instance.PlaySound(audioSource, walkSound, walkVolume, true));
                nextWalkSoundTime = Time.time + walkSound.length;
            }
        }

        positionLastFrameForSound = transform.position;
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        PlayAnimation(attackAnimString);
        if (audioSource != null && attackSound != null)
            StartCoroutine(AudioManager.Instance.PlaySound(audioSource, attackSound, attackVolume, true));

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

        if (clipInfo[0].clip.length > attackRate)
        {
            yield return new WaitForSeconds(1f / clipInfo[0].clip.length * 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1f / attackRate * 0.5f);
        }

        isAttacking = false;
        ReturnToWalk();
    }

    private void Shoot(Vector3 shootPos, Vector3 shootDirection)
    {
        PlayAnimation(attackAnimString);
        if (audioSource != null && attackSound != null)
            StartCoroutine(AudioManager.Instance.PlaySound(audioSource, attackSound, attackVolume, true));

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

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            Destroy(collider);
        }

        PlayAnimation(deathAnimString);
        if (audioSource != null && deathSound != null)
            StartCoroutine(AudioManager.Instance.PlaySound(audioSource, deathSound, deathVolume, true));

        if (deathEffect != null)
        {
            ParticleSystem ps = Instantiate(deathEffect, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        float delay = 0f;

        if (animator != null && animator.enabled)
        {
            if (audioSource != null && deathSound != null)
            {
                delay = deathSound.length > clipInfo[0].clip.length ? deathSound.length : clipInfo[0].clip.length;
            }
            else
            {
                delay = clipInfo[0].clip.length;
            }
        }
        else
        {
            delay = 0f;
        }

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
        projectiles.Add(projectileSpawned);

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

    private void OnDestroy()
    {
        foreach (Projectile current in projectiles)
        {
            if (current != null)
                Destroy(current.gameObject);
        }
    }

    //private void OnDisable()
    //{
    //    foreach (Projectile current in projectiles)
    //    {
    //        Debug.Log("Disable called");
    //        Destroy(current.gameObject);
    //    }
    //}
}
