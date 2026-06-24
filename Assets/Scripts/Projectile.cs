using UnityEngine;
using FMODUnity;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damageToPlayer = 0f;

    [Header("FMOD")]
    public EventReference impactEventFmod;

    private Vector3 targetPos;
    private float moveSpeed;
    private bool isSetup = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Setup(Vector3 targetPosition, float speed, float damage)
    {
        targetPos = targetPosition;
        moveSpeed = speed;
        damageToPlayer = damage;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            Debug.LogWarning("Projectile needs a Rigidbody for physics movement.");
            Destroy(gameObject);
            return;
        }

        isSetup = true;
    }

    private void FixedUpdate()
    {
        if (!isSetup || rb == null) return;

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        if (Vector3.Distance(newPosition, targetPos) <= 0.1f)
        {
            Destroy(gameObject);
        }
    }

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
