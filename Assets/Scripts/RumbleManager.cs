using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;

    private Gamepad gamepad;

    private Coroutine stopRumbleAfterTime;

    private string currentControlScheme;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        InputManager.instance.playerInput.onControlsChanged += SwitchControls;
    }

    public void RumblePulse (float lowFrequency, float highFrequency, float duration)
    {
        if (currentControlScheme != "Gamepad") return;

        gamepad = Gamepad.current;

        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);

            if (stopRumbleAfterTime != null)
            {
                StopCoroutine(stopRumbleAfterTime);
            }

            stopRumbleAfterTime = StartCoroutine(StopRumble(duration, gamepad));
        }
    }

    private IEnumerator StopRumble(float duration, Gamepad gamepad)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gamepad.SetMotorSpeeds(0f, 0f);
    }

    private void SwitchControls(PlayerInput input)
    {
        currentControlScheme = input.currentControlScheme;
    }

    private void OnDisable()
    {
        InputManager.instance.playerInput.onControlsChanged -= SwitchControls;
    }
}
