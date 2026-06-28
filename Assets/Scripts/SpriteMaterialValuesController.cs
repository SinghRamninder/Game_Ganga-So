using UnityEngine;

[ExecuteAlways]
public class SpriteMaterialValuesController : MonoBehaviour
{
    [Header("Dissolve Values")]
    [SerializeField, Range(0f, 1f)] private float dissolveAmount = 0f;
    [SerializeField, Range(0f, 500f)] private float dissolveScale = 30f;
    [SerializeField, Range(0f, 1f)] private float outlineThickness = 0.023f;

    [SerializeField, ColorUsage(true, true)]
    private Color outlineColor = Color.red;

    [SerializeField, Range(0f, 1f)] private float spiralStrength = 0f;
    [SerializeField, Range(0f, 1f)] private float verticalDissolve = 0f;

    private Animator animator;

    private SpriteRenderer[] spriteRenderers;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int DissolveScaleID = Shader.PropertyToID("_DissolveScale");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int SpiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private static readonly int VerticalDissolveID = Shader.PropertyToID("_VerticalDissolve");

    private bool isAnimationDone = false;

    private void OnEnable()
    {
        if (!isAnimationDone)
        {
            RefreshRenderers();
            ApplyValuesToMaterial();
        }
    }

    private void OnValidate()
    {
        if (!isAnimationDone)
        {
            RefreshRenderers();
            ApplyValuesToMaterial();
        }
    }

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void LateUpdate()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Default"))
            {
                if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
                {
                    if (!isAnimationDone)
                    {
                        isAnimationDone = true;
                        ApplyValuesToMaterial();
                    }
                }
            }
        }

        if (!isAnimationDone)
        {
            ApplyValuesToMaterial();
        }
    }

    private void RefreshRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private void ApplyValuesToMaterial()
    {
        if (spriteRenderers == null || propertyBlock == null)
            return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer sr = spriteRenderers[i];

            if (sr == null)
                continue;

            sr.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(DissolveAmountID, dissolveAmount);
            propertyBlock.SetFloat(DissolveScaleID, dissolveScale);
            propertyBlock.SetFloat(OutlineThicknessID, outlineThickness);
            propertyBlock.SetColor(OutlineColorID, outlineColor);
            propertyBlock.SetFloat(SpiralStrengthID, spiralStrength);
            propertyBlock.SetFloat(VerticalDissolveID, verticalDissolve);

            sr.SetPropertyBlock(propertyBlock);
        }

        //if (isAnimationDone)
        //{
        //    mainSpriteObject.SetActive(true);
        //    animationSpriteObject.SetActive(false);
        //    enemyScript.enabled = true;
        //}
    }
}