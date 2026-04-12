using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float speed = 3f;
    public float damageToPlayer = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;

    [Header("Effects")]
    public ParticleSystem deathEffect;

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

        // Billboard effect (face main camera)
        if (mainCam != null)
        {
            Vector3 lookDirection = transform.position - mainCam.transform.position;
            lookDirection.y = 0; // Keep upright
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > attackRange)
        {
            // Move towards player
            Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
            moveDirection.y = 0;
            transform.position += moveDirection * speed * Time.deltaTime;
        }
        else
        {
            // Attack
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + (1f / attackRate);
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
        
        // After attacking, optionally return to walking animation
        Invoke(nameof(ReturnToWalk), 1f / attackRate * 0.5f);
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
}
