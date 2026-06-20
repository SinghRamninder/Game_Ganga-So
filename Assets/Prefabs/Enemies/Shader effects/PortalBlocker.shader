Shader "Custom/URP/PortalBlocker"
{
    Properties
    {
        [Header(Portal Look)]
        [HDR] _PortalColor      ("Portal Color (HDR)", Color) = (0.45, 0.1, 1.0, 1)
        [HDR] _PortalColorB     ("Portal Color Secondary (HDR)", Color) = (1.0, 0.15, 0.85, 1)
        _Intensity              ("Intensity", Range(0, 12)) = 1.5
        _BaseAlpha              ("Base Transparency", Range(0, 1)) = 0.85

        [Header(Flow Animation)]
        _NoiseScale             ("Noise Scale", Float) = 4
        _ScrollSpeed            ("Scroll Speed", Float) = 0.35
        _SwirlStrength          ("Swirl Strength", Float) = 1.2
        _Distortion             ("Distortion", Range(0, 1)) = 0.18

        [Header(Appear Reveal from Top)]
        _AppearAmount           ("Appear Amount", Range(0, 1)) = 1
        [Toggle] _FlipReveal    ("Flip Reveal Direction", Float) = 0
        _EdgeJitter             ("Edge Jitter (dissolve roughness)", Range(0, 1)) = 0.25
        _EdgeSoftness           ("Edge Softness", Range(0.001, 0.5)) = 0.08
        [HDR] _EdgeColor        ("Reveal Edge Color (HDR)", Color) = (3, 0.6, 2, 1)
        _EdgeWidth              ("Reveal Edge Width", Range(0.001, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"  = "UniversalPipeline"
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PortalUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off          // two-sided so the barrier reads from either approach

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _PortalColor;
                float4 _PortalColorB;
                float  _Intensity;
                float  _BaseAlpha;
                float  _NoiseScale;
                float  _ScrollSpeed;
                float  _SwirlStrength;
                float  _Distortion;
                float  _AppearAmount;
                float  _FlipReveal;
                float  _EdgeJitter;
                float  _EdgeSoftness;
                float4 _EdgeColor;
                float  _EdgeWidth;
            CBUFFER_END

            // ---- value noise + fbm (no texture needed) ----
            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);              // smoothstep interpolation
                float a = hash22(i + float2(0, 0)).x;
                float b = hash22(i + float2(1, 0)).x;
                float c = hash22(i + float2(0, 1)).x;
                float d = hash22(i + float2(1, 1)).x;
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float amp = 0.5;
                [unroll]
                for (int k = 0; k < 4; k++)
                {
                    v += amp * valueNoise(p);
                    p *= 2.0;
                    amp *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float2 uv = IN.uv;
                float  t  = _Time.y * _ScrollSpeed;

                // --- swirl the UVs around the center for the portal vortex ---
                float2 c   = uv - 0.5;
                float  ang = _SwirlStrength * length(c);
                float  sn  = sin(ang);
                float  cs  = cos(ang);
                float2 swirled = float2(cs * c.x - sn * c.y,
                                        sn * c.x + cs * c.y) + 0.5;

                // --- domain-warp the sample coords so the flow churns ---
                float2 warp;
                warp.x = fbm(swirled * _NoiseScale + float2(t, 0.0));
                warp.y = fbm(swirled * _NoiseScale + float2(0.0, t) + 17.3);
                float2 sampleUV = swirled * _NoiseScale
                                + (warp - 0.5) * _Distortion * _NoiseScale;

                // --- two scrolling noise layers = nether-portal-style flow ---
                float n1   = fbm(sampleUV + float2(0.0, -t));
                float n2   = fbm(sampleUV * 1.7 + float2(-t * 0.6, t * 0.3));
                float flow = saturate(n1 * 0.65 + n2 * 0.5);

                float3 portal = lerp(_PortalColor.rgb, _PortalColorB.rgb, flow);
                portal *= (0.5 + flow);          // hot spots bloom brighter
                portal *= _Intensity;

                // --- appear reveal from the top (with jagged dissolve edge) ---
                // vGrad: 0 at the revealing edge, 1 at the opposite edge.
                float vGrad = lerp(1.0 - uv.y, uv.y, _FlipReveal);

                float jitterN  = fbm(uv * _NoiseScale * 0.5 + t * 0.2);
                float orderMap = vGrad + (jitterN - 0.5) * 2.0 * _EdgeJitter;

                // remap appear so 0 = nothing, 1 = everything (covers jitter range)
                float cut  = lerp(-_EdgeJitter, 1.0 + _EdgeJitter, _AppearAmount);
                float edge = cut - orderMap;     // > 0 = revealed

                float body     = smoothstep(0.0, _EdgeSoftness, edge);
                float edgeBand = saturate(1.0 - abs(edge) / _EdgeWidth);

                float3 col   = portal + _EdgeColor.rgb * edgeBand;
                float  alpha = saturate(body * _BaseAlpha + edgeBand);

                // hard guarantee: appear == 0 -> fully invisible
                alpha *= step(0.0001, _AppearAmount);

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
