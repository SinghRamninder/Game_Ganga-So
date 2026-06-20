Shader "Custom/URP/2D/Sprite-Unlit-CutoutDepth"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        // Set per-character by the CharacterDepthAnchor script (world pos in xyz, w=1 to use it).
        [HideInInspector] _DepthAnchorWS ("Depth Anchor WS", Vector) = (0,0,0,0)

        // Legacy / sprite plumbing.
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "AlphaTest"
            "RenderType"     = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                float  _Cutoff;
                float4 _DepthAnchorWS;
            CBUFFER_END

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);                 // bone deformation
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;

                // --- Shared depth flatten -------------------------------------------
                // Flatten this vertex to a single anchor depth. When the script feeds a
                // world anchor (the character root), EVERY part of the character shares
                // one depth plane -> their order is decided by draw order at all camera
                // distances (no perspective-driven popping), while the character as a
                // whole still depth-tests against glass / portal / other enemies.
                // Falls back to the object's own origin if no anchor is set (w == 0).
                float4 anchorCS = (_DepthAnchorWS.w > 0.5)
                    ? TransformWorldToHClip(_DepthAnchorWS.xyz)
                    : TransformObjectToHClip(float3(0.0, 0.0, 0.0));
                o.positionCS.z = (anchorCS.z / anchorCS.w) * o.positionCS.w;
                // --------------------------------------------------------------------

                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                half4 c = CommonUnlitFragment(input, input.color);
                clip(c.a - _Cutoff);
                return c;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
