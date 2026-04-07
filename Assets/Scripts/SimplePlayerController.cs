using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.1f;
    public float controllerSensitivity = 150f;
    public float maxLookAngle = 39f;

    [Header("Head Bob Settings")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;

    [Header("References")]
    public Camera playerCamera;
    public GameObject weaponHolder;

    private float verticalRotation = 0f;
    private CharacterController controller;
    private float yVelocity;
    private Vector3 initialWeaponPosition;
    private Vector3 initialCameraPosition;
    private float bobTimer = 0f;

    // Movement & Effects Offsets
    private Vector3 weaponPitchOffset = Vector3.zero;
    private Vector3 currentBobOffset = Vector3.zero;
    private Vector3 currentShakeOffset = Vector3.zero;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
            initialCameraPosition = playerCamera.transform.localPosition;

        if (weaponHolder != null)
            initialWeaponPosition = weaponHolder.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleHeadBob();
        HandleCameraShake();
        ApplyOffsets();
    }

    private void HandleLook()
    {
        Vector2 lookInput = InputManager.instance.controls.Player.Look.ReadValue<Vector2>();

        bool isGamepad = InputManager.instance.controls.Player.Look.activeControl?.device is Gamepad;

        float currentSensitivity = isGamepad ? (controllerSensitivity * Time.deltaTime) : mouseSensitivity;

        float mouseX = lookInput.x * currentSensitivity;
        float mouseY = lookInput.y * currentSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 180f, 0f);

        if (weaponHolder != null)
        {
            weaponHolder.transform.localRotation = Quaternion.Euler(-verticalRotation, 0f, 0f);

            float angleRatio = verticalRotation / maxLookAngle; // -1 to 1
            weaponPitchOffset = new Vector3(0f, -angleRatio * 0.15f, Mathf.Abs(angleRatio) * -0.1f);
        }

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

    private void HandleHeadBob()
    {
        if (playerCamera == null) return;

        float speed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        if (speed > 0.1f && controller.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float newY = Mathf.Sin(bobTimer) * bobAmount;
            float newX = Mathf.Cos(bobTimer / 2f) * bobAmount * 0.5f;
            currentBobOffset = new Vector3(newX, newY, 0f);
        }
        else
        {
            bobTimer = 0f;
            currentBobOffset = Vector3.Lerp(currentBobOffset, Vector3.zero, Time.deltaTime * 5f);
        }
    }

    private void HandleCameraShake()
    {
        if (shakeDuration > 0f)
        {
            currentShakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            currentShakeOffset = Vector3.zero;
            shakeDuration = 0f;
        }
    }

    private void ApplyOffsets()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = initialCameraPosition + currentBobOffset + currentShakeOffset;
        }

        if (weaponHolder != null)
        {
            weaponHolder.transform.localPosition = initialWeaponPosition + weaponPitchOffset + currentBobOffset + (currentShakeOffset * 0.5f);
        }
    }

    public void TriggerCameraShake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
    }

    public void TriggerWeaponRecoil(float recoilForce)
    {
        verticalRotation -= recoilForce;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
    }
}
