using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        LevelStreamer.Instance.LoadLevel();
    }
}
