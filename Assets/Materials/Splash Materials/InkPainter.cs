using UnityEngine;

public class InkPainter : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Texture2D brushTexture;
    public Material drawMaterial;

    [Range(0.01f, 1f)]
    public float brushSize = 0.15f;

    public Color inkColor = Color.red;

    public void Paint(Vector2 uv)
    {
        Debug.Log("Paint called on: " + gameObject.name);
        Debug.Log("RenderTexture: " + (renderTexture != null ? renderTexture.name : "NULL"));
        Debug.Log("BrushTexture: " + (brushTexture != null ? brushTexture.name : "NULL"));
        Debug.Log("DrawMaterial: " + (drawMaterial != null ? drawMaterial.name : "NULL"));

        if (renderTexture == null || brushTexture == null || drawMaterial == null)
            return;

        drawMaterial.SetTexture("_BrushTex", brushTexture);
        drawMaterial.SetVector("_UV", uv);
        drawMaterial.SetFloat("_BrushSize", brushSize);
        drawMaterial.SetColor("_InkColor", inkColor);

        RenderTexture temp = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format);

        Graphics.Blit(renderTexture, temp);
        Graphics.Blit(temp, renderTexture, drawMaterial);

        RenderTexture.ReleaseTemporary(temp);
    }
}