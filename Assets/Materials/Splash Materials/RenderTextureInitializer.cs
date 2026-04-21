using UnityEngine;

public class RenderTextureInitializer : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Color clearColor = Color.black;

    void Start()
    {
        if (renderTexture == null) return;

        RenderTexture current = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, clearColor);
        RenderTexture.active = current;
    }
}