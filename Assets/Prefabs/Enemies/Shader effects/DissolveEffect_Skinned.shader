Shader "Custom/DissolveEffect_Skinned"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Sprite Texture", 2D) = "white" {}
        _DissolveAmount   ("Dissolve Amount",    Range(0, 1))     = 0
        _DissolveScale    ("Dissolve Scale",     Range(0, 500))   = 30
        _OutlineThickness ("Outline Thickness",  Range(0, 1))     = 0.1
        [HDR] _OutlineColor ("Outline Color",    Color)           = (0.9471698, 0, 0.4228832, 1)
        _SpiralStrength   ("Spiral Strength",    Range(0, 100))   = 1
        _VerticalDissolve ("Vertical Dissolve",  Range(0, 1.1))   = 0.5

        [HideInInspector] _Color               ("Tint",           Color)  = (1,1,1,1)
        [HideInInspector] PixelSnap            ("Pixel snap",     Float)  = 0
        [HideInInspector] _RendererColor       ("RendererColor",  Color)  = (1,1,1,1)
        [HideInInspector] _AlphaTex            ("External Alpha", 2D)     = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [MaterialToggle]  _ZWrite              ("ZWrite",         Float)  = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        Blend  SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull   Off
        ZWrite [_ZWrite]

        // ── Pass 1 – Universal2D ──────────────────────────────────────
        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM

            #pragma vertex   DissolveVert
            #pragma fragment DissolveFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            // _MainTex / sampler_MainTex / _MainTex_TexelSize come from 2DCommon.hlsl
            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                float  _DissolveAmount;
                float  _DissolveScale;
                float  _OutlineThickness;
                float4 _OutlineColor;
                float  _SpiralStrength;
                float  _VerticalDissolve;
            CBUFFER_END

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

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                float r0, r1, r2, r3;
                Hash_Tchou_2_1_float(i,                r0);
                Hash_Tchou_2_1_float(i + float2(1, 0), r1);
                Hash_Tchou_2_1_float(i + float2(0, 1), r2);
                Hash_Tchou_2_1_float(i + float2(1, 1), r3);
                return lerp(lerp(r0, r1, f.x), lerp(r2, r3, f.x), f.y);
            }

            float SimpleNoise(float2 uv, float scale)
            {
                return ValueNoise(uv *  scale)         * 0.125
                     + ValueNoise(uv * (scale * 0.5))  * 0.25
                     + ValueNoise(uv * (scale * 0.25)) * 0.5;
            }

            float2 Twirl(float2 uv, float strength)
            {
                float2 d = uv - 0.5;
                float  a = strength * length(d);
                float  c = cos(a), s = sin(a);
                return float2(c * d.x - s * d.y + 0.5,
                              s * d.x + c * d.y + 0.5);
            }

            Varyings DissolveVert(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);   // ← apply bone deformation first
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 DissolveFrag(Varyings input) : SV_Target
            {
                float2 uv       = input.uv;
                float4 texColor = (float4)(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color);

                float2 twirlUV = Twirl(uv, _SpiralStrength);
                float  noise   = SimpleNoise(twirlUV, _DissolveScale);
                float  inv     = 1.0 - _DissolveAmount;
                float  nFull   = step(noise, inv + _OutlineThickness);
                float  nEdge   = step(noise, inv);
                float  nBand   = nFull - nEdge;

                float  vInv    = 1.0 - _VerticalDissolve;
                float  vFull   = step(uv.y, vInv + _OutlineThickness);
                float  vEdge   = step(uv.y, vInv);
                float  vBand   = vFull - vEdge;

                float4 noiseColor = texColor - nBand + nBand * _OutlineColor;
                float4 finalColor = vBand * _OutlineColor + noiseColor;
                float  alpha      = texColor.a * nFull * vFull;

                return half4(finalColor.rgb, alpha);
            }

            ENDHLSL
        }

        // ── Pass 2 – UniversalForward ─────────────────────────────────
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM

            #pragma vertex   DissolveVert
            #pragma fragment DissolveFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                float  _DissolveAmount;
                float  _DissolveScale;
                float  _OutlineThickness;
                float4 _OutlineColor;
                float  _SpiralStrength;
                float  _VerticalDissolve;
            CBUFFER_END

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

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                float r0, r1, r2, r3;
                Hash_Tchou_2_1_float(i,                r0);
                Hash_Tchou_2_1_float(i + float2(1, 0), r1);
                Hash_Tchou_2_1_float(i + float2(0, 1), r2);
                Hash_Tchou_2_1_float(i + float2(1, 1), r3);
                return lerp(lerp(r0, r1, f.x), lerp(r2, r3, f.x), f.y);
            }

            float SimpleNoise(float2 uv, float scale)
            {
                return ValueNoise(uv *  scale)         * 0.125
                     + ValueNoise(uv * (scale * 0.5))  * 0.25
                     + ValueNoise(uv * (scale * 0.25)) * 0.5;
            }

            float2 Twirl(float2 uv, float strength)
            {
                float2 d = uv - 0.5;
                float  a = strength * length(d);
                float  c = cos(a), s = sin(a);
                return float2(c * d.x - s * d.y + 0.5,
                              s * d.x + c * d.y + 0.5);
            }

            Varyings DissolveVert(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 DissolveFrag(Varyings input) : SV_Target
            {
                float2 uv       = input.uv;
                float4 texColor = (float4)(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * input.color);

                float2 twirlUV = Twirl(uv, _SpiralStrength);
                float  noise   = SimpleNoise(twirlUV, _DissolveScale);
                float  inv     = 1.0 - _DissolveAmount;
                float  nFull   = step(noise, inv + _OutlineThickness);
                float  nEdge   = step(noise, inv);
                float  nBand   = nFull - nEdge;

                float  vInv    = 1.0 - _VerticalDissolve;
                float  vFull   = step(uv.y, vInv + _OutlineThickness);
                float  vEdge   = step(uv.y, vInv);
                float  vBand   = vFull - vEdge;

                float4 noiseColor = texColor - nBand + nBand * _OutlineColor;
                float4 finalColor = vBand * _OutlineColor + noiseColor;
                float  alpha      = texColor.a * nFull * vFull;

                return half4(finalColor.rgb, alpha);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}