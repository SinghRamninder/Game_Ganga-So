using UnityEngine;

public class KeyCard : MonoBehaviour
{
    [SerializeField] private float maxUpAnimPos = 2f;
    [SerializeField] private float animationSpeed = 1f;

    private Camera mainCamera;
    private float startYPos;

    private void Start()
    {
        mainCamera = Camera.main;
        startYPos = transform.position.y;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time * animationSpeed, 1f);
        float upPos = Mathf.Lerp(startYPos, startYPos + maxUpAnimPos, t);

        transform.position = new Vector3(transform.position.x, upPos, transform.position.z);

        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.keyCards.Add(other.gameObject.name);
            }
        }
    }
}
