using UnityEngine;
using UnityEngine.Playables;

public class EndCutsceneManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector endCutsceneTimeline;
    [SerializeField] private FinalShot finalShot;

    private bool timelineStarted = false;
    private bool finalShotCalled = false;

    // private void Awake()
    // {
    //     if (endCutsceneTimeline == null)
    //     {
    //         endCutsceneTimeline = GetComponent<PlayableDirector>();
    //     }
    // }

    // private void OnEnable()
    // {
    //     if (endCutsceneTimeline != null)
    //     {
    //         endCutsceneTimeline.played += OnTimelinePlayed;
    //         endCutsceneTimeline.stopped += OnTimelineStopped;
    //     }
    // }

    // private void OnDisable()
    // {
    //     if (endCutsceneTimeline != null)
    //     {
    //         endCutsceneTimeline.played -= OnTimelinePlayed;
    //         endCutsceneTimeline.stopped -= OnTimelineStopped;
    //     }
    // }

    // private void OnTimelinePlayed(PlayableDirector director)
    // {
    //     if (director != endCutsceneTimeline) return;

    //     timelineStarted = true;
    // }

    private void Update()
    {
        if (InputManager.instance.controls.Weapon.Attack.WasPressedThisFrame())
            {
                finalShot.BeginFinalShotMode();
            }
    }

    // private void OnTimelineStopped(PlayableDirector director)
    // {
    //     if (director != endCutsceneTimeline) return;
    //     if (!timelineStarted) return;
    //     if (finalShotCalled) return;

    //     finalShotCalled = true;

    //     if (finalShot != null)
    //     {
    //         finalShot.BeginFinalShotMode();
    //     }
    //     else
    //     {
    //         Debug.LogWarning("FinalShot reference is missing in EndCutsceneManager.");
    //     }
    // }

    // public void ResetEndCutsceneManager()
    // {
    //     timelineStarted = false;
    //     finalShotCalled = false;
    // }
}