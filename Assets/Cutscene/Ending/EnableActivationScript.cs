using UnityEngine;

public class EnableActivationScript : MonoBehaviour
{
    private ActivateEnemiesSlowly activationScript;

    private void OnEnable()
    {
        activationScript = GetComponent<ActivateEnemiesSlowly>();
        if (activationScript != null)
        {
            activationScript.enabled = true;
        }
    }
}
