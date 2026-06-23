using UnityEngine;

public class TurnOnScanAnimation : MonoBehaviour
{
    [SerializeField] private ServerScanAnimation serverScanAnimation;

    private void OnEnable()
    {
        if (serverScanAnimation == null)
        {
            serverScanAnimation = GetComponent<ServerScanAnimation>();
        }
        
        if (serverScanAnimation != null)
        {
            serverScanAnimation.enabled = true;
        }
    }
}
