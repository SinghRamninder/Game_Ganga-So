using UnityEngine;
using Unity.Cinemachine;   // Cinemachine.* if you're on Cinemachine 2.x

// Drop this on the player (or any object). Wire EndCutscene() to the end of
// the timeline (a Signal, or PlayableDirector.stopped). It snaps the body to
// wherever the cutscene camera finished, so first-person view doesn't teleport
// back to the door when control returns.
public class CutsceneHandover : MonoBehaviour
{
    [Header("References")]
    public Transform player;                  // player root (SimplePlayerController + CharacterController)
    public Transform cam;                      // playerCamera, a child of the player root
    public CinemachineBrain brain;             // on the Main Camera
    public SimplePlayerController moveScript;
    public CharacterController controller;

    Vector3 headOffset;                        // camera's normal local position (the head rig)

    void Awake()
    {
        // Capture the rig offset NOW, before any cutscene clobbers the camera's
        // local transform. Don't read this at handover time — it'll be garbage by then.
        headOffset = cam.localPosition;
    }

    // Call the instant the timeline ends.
    public void EndCutscene()
    {
        // 1. Freeze the world pose the cutscene camera left us in.
        Vector3    finalPos = cam.position;
        Quaternion finalRot = cam.rotation;

        // 2. Hand the real camera back from Cinemachine to us.
        brain.enabled = false;

        // 3. Decompose that world rotation into THIS rig's yaw (body) + pitch (camera).
        Vector3 camFwd = finalRot * Vector3.forward;

        // --- Yaw -> body. This rig has a 180 deg camera offset (camera looks down the
        //     body's BACK), so the body must face OPPOSITE the camera's flat forward.
        Vector3 flatFwd = new Vector3(camFwd.x, 0f, camFwd.z);
        if (flatFwd.sqrMagnitude < 1e-6f)            // looking near-straight up/down
            flatFwd = -player.forward;                // keep current facing as fallback
        flatFwd.Normalize();
        Quaternion bodyRot = Quaternion.LookRotation(-flatFwd, Vector3.up);

        // --- Pitch -> verticalRotation. In this rig local cam forward.y = -sin(pitch),
        //     so pitch = -asin(camFwd.y). SetVerticalLook clamps it to maxLookAngle.
        float pitch = -Mathf.Asin(Mathf.Clamp(camFwd.y, -1f, 1f)) * Mathf.Rad2Deg;

        // 4. Stand the body so the camera (its child) lands where the cutscene cam was.
        Vector3 bodyPos = finalPos - bodyRot * headOffset;

        controller.enabled = false;                  // CharacterController blocks direct moves
        player.SetPositionAndRotation(bodyPos, bodyRot);
        controller.enabled = true;

        // 5. Feed the pitch back into the controller so it continues from here
        //    instead of snapping to its old stored value on the next frame.
        moveScript.SetVerticalLook(pitch);

        // 6. Resume gameplay. HandleLook + ApplyOffsets rebuild the camera's local
        //    rotation and position next frame, so no manual camera reset is needed.
        moveScript.enabled = true;
    }

    // Call this before STARTING a cutscene (and disable moveScript at the same time).
    public void BeginCutscene()
    {
        brain.enabled = true;        // re-arm Cinemachine so the cutscene cam can take over
        moveScript.enabled = false;  // stop the player script writing to the camera
    }
}