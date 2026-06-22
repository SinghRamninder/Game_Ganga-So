using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[CreateAssetMenu(fileName = "EnemyAudioProfile", menuName = "Audio/Enemy Audio Profile")]
public class EnemyAudioProfile : ScriptableObject
{
    [Header("Locomotion")]
    public EventReference footstepEventFmod;

    [Header("Combat")]
    public EventReference attackEventFmod;   // shared by melee Attack() and ranged Shoot()
    public EventReference hitEventFmod;       // single event; body part sent as an FMOD labeled parameter
    public EventReference deathEventFmod;

    [Header("AI state")]
    public EventReference alertEventFmod;     // first player acquisition

    public void PlayFootstep(GameObject target) => PlayOneShot(footstepEventFmod, target);
    public void PlayAttack(GameObject target) => PlayOneShot(attackEventFmod, target);
    public void PlayDeath(GameObject target) => PlayOneShot(deathEventFmod, target);
    public void PlayAlert(GameObject target) => PlayOneShot(alertEventFmod, target);

    public void PlayHit(GameObject target, BodyPart bodyPart)
    {
        if (hitEventFmod.IsNull) return;
        EventInstance instance = RuntimeManager.CreateInstance(hitEventFmod);
        instance.setParameterByNameWithLabel("BodyPart", bodyPart.ToString()); // labeled param, authored in FMOD Studio
        RuntimeManager.AttachInstanceToGameObject(instance, target.transform);
        instance.start();
        instance.release();
    }

    private void PlayOneShot(EventReference eventRef, GameObject target)
    {
        if (eventRef.IsNull) return;
        RuntimeManager.PlayOneShotAttached(eventRef, target);
    }
}
