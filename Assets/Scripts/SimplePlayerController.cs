using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 0.1f;
    public float controllerSensitivity = 150f;
    public float maxLookAngle = 39f;
    public float jumpHeight = 1.5f;
    public float gravityMultiplier = 2.5f;
    public bool simpleMovement = false; 
    //public float acceleration = 15f;
    public float deceleration = 20f;

    [HideInInspector] public float gunSpeedModifier = 1f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;

    [Header("FMOD")]
    public EventReference dashEventFmod;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    [Header("Head Bob Settings")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;

    [Header("Footstep Audio")]
    public EventReference footstepEventFmod;
    public float stepInterval = 0.5f;

    [Header("References")]
    public Camera playerCamera;
    public GameObject weaponHolder;

    private float verticalRotation = 0f;
    private CharacterController controller;
    private float yVelocity;
    private Vector3 initialCameraPosition;
    private float bobTimer = 0f;
    private Vector3 currentVelocity;

    private Vector3 currentBobOffset = Vector3.zero;
    private Vector3 currentShakeOffset = Vector3.zero;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;
    private float footstepTimer = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
            initialCameraPosition = playerCamera.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (InputManager.instance.controls.Player.Dash.WasPressedThisFrame() && dashCooldownTimer <= 0 && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            // Testing
            if (!simpleMovement)
            {
                HandleMovement();
            }
            else
            {
                HandleMovementWithoutAD();
            }
            // Testing
        }

        HandleHeadBob();
        HandleCameraShake();
        HandleFootsteps();
        ApplyOffsets();
    }

    private void HandleLook()
    {
        if (PauseMenuController.Instance?.IsGamePaused == true)
            return;

        Vector2 lookInput = InputManager.instance.controls.Player.Look.ReadValue<Vector2>();

        bool isGamepad = InputManager.instance.controls.Player.Look.activeControl?.device is Gamepad;

        float currentSensitivity = isGamepad ? (controllerSensitivity * Time.deltaTime) : mouseSensitivity;

        float mouseX = lookInput.x * currentSensitivity;
        float mouseY = lookInput.y * currentSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 180f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = InputManager.instance.controls.Player.Move.ReadValue<Vector2>();

        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        Vector3 inputDirection = -transform.right * moveX - transform.forward * moveZ;
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);
        
        Vector3 targetVelocity = inputDirection * (moveSpeed * gunSpeedModifier);
        
        //float rate = inputDirection.magnitude > 0.1f ? acceleration : deceleration;
        
        //currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, rate * Time.deltaTime);

        if (inputDirection.magnitude > 0.1f)
        {
            currentVelocity = targetVelocity;
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deceleration * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            yVelocity = -2f;

            if (InputManager.instance.controls.Player.Jump.WasPressedThisFrame())
            {
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityMultiplier));
            }
        }
        else
        {
            yVelocity += (Physics.gravity.y * gravityMultiplier) * Time.deltaTime;
        }

        Vector3 finalMove = currentVelocity;
        finalMove.y = yVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    // Testing
    private void HandleMovementWithoutAD()
    {
        Vector2 moveInput = InputManager.instance.controls.Player.Move.ReadValue<Vector2>();

        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        Vector3 move = -transform.right * moveX - transform.forward * moveZ;

        move *= (moveSpeed * gunSpeedModifier);

        if (controller.isGrounded)
        {
            yVelocity = -2f;

            if (InputManager.instance.controls.Player.Jump.WasPressedThisFrame())
            {
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * (Physics.gravity.y * gravityMultiplier));
            }
        }
        else
        {
            yVelocity += (Physics.gravity.y * gravityMultiplier) * Time.deltaTime;
        }

        move.y = yVelocity;

        controller.Move(move * Time.deltaTime);
    }
    // Testing

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (!dashEventFmod.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(dashEventFmod, gameObject);
        }

        Vector2 moveInput = InputManager.instance.controls.Player.Move.ReadValue<Vector2>();
        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        Vector3 inputDirection = -transform.right * moveX - transform.forward * moveZ;

        if (inputDirection.magnitude < 0.1f)
        {
            inputDirection = -transform.forward;
        }
        else
        {
            inputDirection = inputDirection.normalized;
        }

        dashDirection = inputDirection;
    }

    private void HandleDash()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
            currentVelocity = dashDirection * moveSpeed;
        }

        if (controller.isGrounded)
        {
            yVelocity = -2f;
        }
        else
        {
            yVelocity += (Physics.gravity.y * gravityMultiplier) * Time.deltaTime;
        }

        Vector3 finalMove = dashDirection * dashSpeed;
        finalMove.y = yVelocity;

        controller.Move(finalMove * Time.deltaTime);
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

    private void HandleFootsteps()
    {
        float speed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        if (controller.isGrounded && speed > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (!footstepEventFmod.IsNull)
                {
                    RuntimeManager.PlayOneShotAttached(footstepEventFmod, gameObject);
                }

                footstepTimer = stepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void HandleCameraShake()
    {
        if (PauseMenuController.Instance?.IsGamePaused == true)
            return;

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
        if (PauseMenuController.Instance?.IsGamePaused == true)
            return;

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = initialCameraPosition + currentBobOffset + currentShakeOffset;
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

    public void SetSpeedModifier(float modifier)
    {
        gunSpeedModifier = modifier;
    }

    public void SetVerticalLook(float pitch)
{
    verticalRotation = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
}

}
