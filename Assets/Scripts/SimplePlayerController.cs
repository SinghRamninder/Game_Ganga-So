using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 39f;

    [Header("References")]
    public Camera playerCamera;
    public GameObject weaponHolder;

    private float verticalRotation = 0f;
    private CharacterController controller;
    private float yVelocity;
    private Vector3 initialWeaponPosition;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (weaponHolder != null)
            initialWeaponPosition = weaponHolder.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 180f, 0f);

        // Rotate and adjust position of the weapon so it acts like it's pivoting from the shoulder/hands
        if (weaponHolder != null)
        {
            weaponHolder.transform.localRotation = Quaternion.Euler(-verticalRotation, 0f, 0f);

            // Calculate a positional offset based on how far we are looking up or down
            float angleRatio = verticalRotation / maxLookAngle; // -1 to 1
            // Shift the weapon slightly up/down and forward/back to match natural arm movement
            weaponHolder.transform.localPosition = initialWeaponPosition + new Vector3(0f, -angleRatio * 0.15f, Mathf.Abs(angleRatio) * -0.1f);
        }

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = -transform.right * moveX - transform.forward * moveZ;
        Vector3 gravity = Physics.gravity * Time.deltaTime;
        yVelocity = controller.isGrounded ? -1f : yVelocity + gravity.y;
        move += new Vector3(0f, yVelocity, 0f);
        controller.Move(move * (moveSpeed * Time.deltaTime));
    }
}
