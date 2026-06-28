using UnityEngine;

public class ActivateObjects : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToActivate;

    private void OnEnable()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}
