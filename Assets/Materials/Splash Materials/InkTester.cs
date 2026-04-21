using UnityEngine;

public class InkTester : MonoBehaviour
{
    public Camera cam;

    [SerializeField]
    protected float radius, hardness, strength;

    [SerializeField, Tooltip("Pick your ink color here!")]
    protected Color color = Color.red;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null)
                cam = Camera.main;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SplatableObject splatObj = hit.collider.GetComponent<SplatableObject>();
                if (splatObj != null)
                {
                    Debug.Log("Splatobj found!!");
                    splatObj.DrawSplat(hit.point, radius, hardness, strength, color);
                }
                else
                {
                    Debug.Log("Splatobj not found");
                }
            }
        }
    }
}