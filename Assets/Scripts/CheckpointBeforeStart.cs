using UnityEngine;

public class CheckpointBeforeStart : MonoBehaviour
{
    [SerializeField] private AreaLogic areaLogic;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (areaLogic == null)
            {
                areaLogic = GetComponentInParent<AreaLogic>();
            }
            
            if (areaLogic != null)
            {
                areaLogic.ProvideBeforeStartCheckpoint(transform);
            }
        }
    }
}
