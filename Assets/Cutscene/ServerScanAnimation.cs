using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerScanAnimation : MonoBehaviour
{
    [Header("Animation Timing")]
    [SerializeField] private float masterAnimationTime = 4f;
    [SerializeField] private float scanYStart = -0.54f;
    [SerializeField] private float scanYEnd = 1.31f;

    [Header("Glow Target")]
    [SerializeField] private Color glowTargetColor = new Color32(191, 0, 0, 255);
    [SerializeField] private float glowTargetIntensity = 1.39f;

    private readonly List<Material> outsideMaterials = new List<Material>();
    private readonly List<Material> glowMaterials = new List<Material>();
    private readonly List<Material> videoMaterials = new List<Material>();

    private Coroutine animationRoutine;
    private bool isCached;
    private bool isAnimating;

    private void Awake()
    {
        CacheReferences();
        enabled = false;
    }

    private void OnEnable()
    {
        if (!isCached || isAnimating || !Application.isPlaying)
        {
            return;
        }

        animationRoutine = StartCoroutine(RunAnimation());
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        isAnimating = false;
    }

    public void PlayAnimation()
    {
        if (!isCached)
        {
            CacheReferences();
        }

        if (isAnimating)
        {
            return;
        }

        enabled = true;
    }

    private void CacheReferences()
    {
        if (isCached)
        {
            return;
        }

        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == "Outside")
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    outsideMaterials.Add(renderer.material);
                }
            }

            if (child.name == "Glow")
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    glowMaterials.Add(renderer.material);
                }
            }

            VideoScreen videoScreen = child.GetComponent<VideoScreen>();
            if (videoScreen != null)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    videoMaterials.Add(renderer.material);
                    SetMaterialAlpha(renderer.material, 0f);
                }
            }
        }

        isCached = true;
    }

    private void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
        else if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");
            c.a = alpha;
            mat.SetColor("_Color", c);
        }
    }

    private IEnumerator RunAnimation()
    {
        isAnimating = true;

        float animationDuration = Mathf.Max(0.01f, masterAnimationTime);

        for (int i = outsideMaterials.Count - 1; i >= 0; i--)
        {
            if (outsideMaterials[i] != null && outsideMaterials[i].HasProperty("_ScanY"))
            {
                outsideMaterials[i].SetFloat("_ScanY", scanYStart);
            }
        }

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);

            UpdateOutsideScan(normalizedTime);
            UpdateGlowSequence(normalizedTime);
            UpdateVideoSequence(normalizedTime);

            yield return null;
        }

        UpdateOutsideScan(1f);
        UpdateGlowSequence(1f);
        UpdateVideoSequence(1f);

        animationRoutine = null;
        isAnimating = false;
        enabled = false;
    }

    private void UpdateOutsideScan(float normalizedTime)
    {
        float scanY = Mathf.Lerp(scanYStart, scanYEnd, normalizedTime);

        for (int i = 0; i < outsideMaterials.Count; i++)
        {
            Material material = outsideMaterials[i];
            if (material != null && material.HasProperty("_ScanY"))
            {
                material.SetFloat("_ScanY", scanY);
            }
        }
    }

    private void UpdateGlowSequence(float normalizedTime)
    {
        if (glowMaterials.Count == 0)
        {
            return;
        }

        float intensityMultiplier = Mathf.Pow(2, glowTargetIntensity);
        Color targetEmissionColor = new Color(glowTargetColor.r * intensityMultiplier, glowTargetColor.g * intensityMultiplier, glowTargetColor.b * intensityMultiplier, 1f);
        Color currentColor = Color.Lerp(Color.black, targetEmissionColor, normalizedTime);

        for (int index = 0; index < glowMaterials.Count; index++)
        {
            Material material = glowMaterials[index];
            if (material == null)
            {
                continue;
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", currentColor);
            }
        }
    }

    private void UpdateVideoSequence(float normalizedTime)
    {
        if (videoMaterials.Count == 0)
        {
            return;
        }

        for (int index = 0; index < videoMaterials.Count; index++)
        {
            Material material = videoMaterials[index];
            if (material == null)
            {
                continue;
            }

            SetMaterialAlpha(material, normalizedTime);
        }
    }
}
