using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class FinalShot : MonoBehaviour
{
    [Header("Final Shot Camera")]
    [SerializeField] private GameObject finalShotCameraObject;
    [SerializeField] private Transform finalShotCameraTarget;

    [Header("Weapon View To Hide")]
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private Camera weaponCamera;

    [Header("Player Scripts To Disable")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Bullet Camera Movement")]
    [SerializeField] private float bulletFollowDuration = 0.75f;
    [SerializeField] private AnimationCurve bulletMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Slow Motion")]
    [SerializeField] private float slowMotionScale = 0.2f;

    [Header("Explosion")]
    [SerializeField] private ParticleSystem bigExplosionPrefab;

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.45f;
    [SerializeField] private float shakeStrength = 0.35f;

    [Header("Camera Throwback")]
    [SerializeField] private float throwbackDistance = 3f;
    [SerializeField] private float throwbackUpAmount = 0.6f;
    [SerializeField] private float throwbackDuration = 0.35f;

    [Header("White Flash")]
    [SerializeField] private Image whiteFlashImage;
    [SerializeField] private float whiteFlashHoldTime = 0.15f;
    [SerializeField] private float whiteFlashFadeTime = 0.8f;

    [Header("Next Cutscene")]
    [SerializeField] private PlayableDirector afterExplosionTimeline;

    private bool waitingForShot = false;
    private bool shotAlreadyPressed = false;

    private void Awake()
    {
        if (finalShotCameraObject != null)
        {
            finalShotCameraObject.SetActive(false);
        }

        if (whiteFlashImage != null)
        {
            Color c = whiteFlashImage.color;
            c.a = 0f;
            whiteFlashImage.color = c;
            whiteFlashImage.gameObject.SetActive(false);
        }

        enabled = false;
    }

    public void BeginFinalShotMode()
    {
        enabled = true;

        waitingForShot = true;
        shotAlreadyPressed = false;

        if (PlayerWeaponManager.instance != null)
        {
            PlayerWeaponManager.instance.blockNormalWeaponInput = true;
        }

        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (!waitingForShot) return;
        if (shotAlreadyPressed) return;

        if (PlayerWeaponManager.instance == null) return;
        if (PlayerWeaponManager.instance.currentWeaponScript == null) return;
        if (InputManager.instance == null) return;

        Weapon currentWeapon = PlayerWeaponManager.instance.currentWeaponScript;

        if (currentWeapon.isDualPistol)
        {
            if (InputManager.instance.controls.Weapon.DualPistolLeft.WasPressedThisFrame())
            {
                shotAlreadyPressed = true;
                StartCoroutine(FinalShotRoutine(true, false));
                return;
            }

            if (InputManager.instance.controls.Weapon.DualPistolRight.WasPressedThisFrame())
            {
                shotAlreadyPressed = true;
                StartCoroutine(FinalShotRoutine(false, true));
                return;
            }
        }
        else
        {
            if (InputManager.instance.controls.Weapon.Attack.WasPressedThisFrame())
            {
                shotAlreadyPressed = true;
                StartCoroutine(FinalShotRoutine(true, false));
                return;
            }
        }
    }

    private IEnumerator FinalShotRoutine(bool leftClick, bool rightClick)
    {
        waitingForShot = false;

        Weapon weapon = PlayerWeaponManager.instance.currentWeaponScript;

        BarrelTraceResult result = weapon.FireAndReturnBarrelHit(leftClick, rightClick);

        if (!result.hitBarrel)
        {
            Debug.LogWarning("Final shot did not hit the explosive barrel.");

            if (PlayerWeaponManager.instance != null)
            {
                PlayerWeaponManager.instance.blockNormalWeaponInput = false;
            }

            yield break;
        }

        HideWeaponView();

        Time.timeScale = slowMotionScale;

        Vector3 startPos = result.trailSpawnPosition;
        Vector3 endPos = result.hitPosition;

        if (finalShotCameraTarget != null)
        {
            finalShotCameraTarget.position = startPos;
        }

        if (finalShotCameraObject != null)
        {
            finalShotCameraObject.SetActive(true);
        }

        yield return MoveCameraTarget(startPos, endPos);

        TriggerExplosion(result);

        yield return ShakeCameraTarget();

        yield return ThrowCameraBack(startPos, endPos);

        yield return WhiteFlash();

        Time.timeScale = 1f;

        if (finalShotCameraObject != null)
        {
            finalShotCameraObject.SetActive(false);
        }

        if (afterExplosionTimeline != null)
        {
            afterExplosionTimeline.Play();
        }
    }

    private void HideWeaponView()
    {
        if (weaponHolder != null)
        {
            weaponHolder.SetActive(false);
        }

        if (weaponCamera != null)
        {
            weaponCamera.enabled = false;
        }
    }

    private IEnumerator MoveCameraTarget(Vector3 startPos, Vector3 endPos)
    {
        if (finalShotCameraTarget == null)
            yield break;

        float timer = 0f;

        while (timer < bulletFollowDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / bulletFollowDuration);
            float curvedT = bulletMoveCurve.Evaluate(t);

            finalShotCameraTarget.position = Vector3.Lerp(startPos, endPos, curvedT);

            yield return null;
        }

        finalShotCameraTarget.position = endPos;
    }

    private void TriggerExplosion(BarrelTraceResult result)
    {
        if (bigExplosionPrefab != null)
        {
            Instantiate(bigExplosionPrefab, result.hitPosition, Quaternion.identity);
        }

        if (result.barrel != null)
        {
            result.barrel.Explode();
        }
    }

    private IEnumerator ShakeCameraTarget()
    {
        if (finalShotCameraTarget == null)
            yield break;

        Vector3 originalPosition = finalShotCameraTarget.position;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            Vector3 randomOffset = Random.insideUnitSphere * shakeStrength;
            finalShotCameraTarget.position = originalPosition + randomOffset;

            yield return null;
        }

        finalShotCameraTarget.position = originalPosition;
    }

    private IEnumerator ThrowCameraBack(Vector3 shotStartPos, Vector3 explosionPos)
    {
        if (finalShotCameraTarget == null)
            yield break;

        Vector3 directionAwayFromExplosion = (shotStartPos - explosionPos).normalized;

        Vector3 startPos = finalShotCameraTarget.position;
        Vector3 endPos = startPos + directionAwayFromExplosion * throwbackDistance + Vector3.up * throwbackUpAmount;

        float timer = 0f;

        while (timer < throwbackDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / throwbackDuration);
            finalShotCameraTarget.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        finalShotCameraTarget.position = endPos;
    }

    private IEnumerator WhiteFlash()
    {
        if (whiteFlashImage == null)
            yield break;

        whiteFlashImage.gameObject.SetActive(true);

        Color c = whiteFlashImage.color;
        c.a = 1f;
        whiteFlashImage.color = c;

        yield return new WaitForSecondsRealtime(whiteFlashHoldTime);

        float timer = 0f;

        while (timer < whiteFlashFadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / whiteFlashFadeTime);

            c.a = Mathf.Lerp(1f, 0f, t);
            whiteFlashImage.color = c;

            yield return null;
        }

        c.a = 0f;
        whiteFlashImage.color = c;
        whiteFlashImage.gameObject.SetActive(false);
    }
}