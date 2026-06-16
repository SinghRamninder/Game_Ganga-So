Shader "Custom/2D/Sprite-Unlit-Dissolve-Skinned"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        _DissolveAmount("DissolveAmount", Range(0, 1)) = 0
        _DissolveScale("DissolveScale", Range(0, 500)) = 30
        _OutlineThickness("OutlineThickness", Range(0, 1)) = 0.036
        [HDR]_OutlineColor("OutlineColor", Color) = (1, 0.35, 0, 1)
        _SpiralStrength("SpiralStrength", Range(0, 100)) = 0
        _VerticalDissolve("VerticalDissolve", Range(0, 1.1)) = 0

        // Legacy/default sprite properties
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
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

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

                // Important for rigged 2D sprites
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            // GPU Instancing + Sprite Skinning
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;

                float _DissolveAmount;
                float _DissolveScale;
                float _OutlineThickness;
                half4 _OutlineColor;
                float _SpiralStrength;
                float _VerticalDissolve;
            CBUFFER_END

            float RandomValue(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                f = f * f * (3.0 - 2.0 * f);

                float a = RandomValue(i);
                float b = RandomValue(i + float2(1.0, 0.0));
                float c = RandomValue(i + float2(0.0, 1.0));
                float d = RandomValue(i + float2(1.0, 1.0));

                float bottom = lerp(a, b, f.x);
                float top = lerp(c, d, f.x);

                return lerp(bottom, top, f.y);
            }

            float SimpleNoise(float2 uv, float scale)
            {
                uv *= scale;

                float noise = 0.0;
                noise += ValueNoise(uv) * 0.5;
                noise += ValueNoise(uv * 2.0) * 0.25;
                noise += ValueNoise(uv * 4.0) * 0.25;

                return noise;
            }

            float2 TwirlUV(float2 uv, float strength)
            {
                float2 center = float2(0.5, 0.5);
                float2 delta = uv - center;

                float angle = strength * length(delta);

                float s = sin(angle);
                float c = cos(angle);

                float2 rotated;
                rotated.x = c * delta.x - s * delta.y;
                rotated.y = s * delta.x + c * delta.y;

                return rotated + center;
            }

            Varyings UnlitVertex(Attributes input)
            {
                // This is the important part for rigged Sprite Skin animation
                UNITY_SKINNED_VERTEX_COMPUTE(input);

                SetUpSpriteInstanceProperties();

                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);

                o.color = input.color * _Color * unity_SpriteColor;

                return o;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                // This keeps Unity's normal SpriteRenderer behaviour:
                // sprite texture, sprite color, renderer color, external alpha, etc.
                half4 col = CommonUnlitFragment(input, input.color);

                float2 uv = input.uv;

                // Spiral/twirl dissolve noise
                float2 twirledUV = TwirlUV(uv, _SpiralStrength);
                float noise = SimpleNoise(twirledUV, max(_DissolveScale, 0.0001));

                // Normal dissolve
                float dissolveThreshold = 1.0 - _DissolveAmount;

                float dissolveVisible = step(noise, dissolveThreshold + _OutlineThickness);
                float dissolveCore = step(noise, dissolveThreshold);
                float dissolveOutline = saturate(dissolveVisible - dissolveCore);

                // Vertical dissolve
                float verticalThreshold = 1.0 - _VerticalDissolve;

                float verticalVisible = step(uv.y, verticalThreshold + _OutlineThickness);
                float verticalCore = step(uv.y, verticalThreshold);
                float verticalOutline = saturate(verticalVisible - verticalCore);

                // Combine both outlines
                float outlineMask = saturate(dissolveOutline + verticalOutline);

                // Apply outline color
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, outlineMask);

                // Apply dissolve alpha
                col.a *= dissolveVisible * verticalVisible;

                return col;
            }

            ENDHLSL
        }
    }
}