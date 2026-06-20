using UnityEngine;

public class AlwaysLookCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
