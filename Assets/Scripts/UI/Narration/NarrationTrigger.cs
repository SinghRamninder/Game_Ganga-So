using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NarrationTrigger : MonoBehaviour
{
    [SerializeField] private NarrationData data; // ScriptableObject, see below
    [SerializeField] private bool triggerOnce = true;
    private bool hasFired;

    private void OnTriggerEnter(Collider other)
    {
        if (hasFired && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        NarrationManager.Instance.Play(data);
        hasFired = true;
    }
}
