using UnityEngine;

public class InkTester : MonoBehaviour
{
    public Camera cam;

    [SerializeField]
    protected float radius, hardness, strength;

    [SerializeField, Tooltip("Pick your ink color here!")]
    protected Color paintColor = Color.red;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (cam == null)
                cam = Camera.main;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Paintable paintableObj = hit.collider.GetComponent<Paintable>();
                if (paintableObj != null)
                {
                    Debug.Log("Paintable object found");
                    PaintManager.instance.paint(paintableObj, hit.point, radius, hardness, strength, paintColor);
                }
                else
                {
                    Debug.Log("Paintable object not found");
                }
            }
        }
    }
}