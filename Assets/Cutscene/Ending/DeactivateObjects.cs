using UnityEngine;

public class DeactivateObjects : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToDeactivate;

    private void OnEnable()
    {
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
