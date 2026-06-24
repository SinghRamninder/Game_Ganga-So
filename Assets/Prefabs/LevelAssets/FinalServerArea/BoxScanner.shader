Shader "Custom/BoxScanner"
{
    Properties
    {
        [HDR] _ScanColor   ("Scan Color", Color)      = (0, 1, 1, 1)
        [HDR] _EdgeColor   ("Edge / Fresnel Color", Color) = (0, 0.6, 1, 1)

        _ScanY        ("Scan Y", Float)               = 0
        _UseWorldY    ("Use World Y", Range(0,1))    = 0
        _BandWidth    ("Band Width", Range(0.001,5)) = 0.15
        _Trail        ("Trail Length", Range(0.001,10)) = 1.0
        _TrailStrength("Trail Strength", Range(0,1))  = 0.3

        _FresnelPower ("Fresnel Power", Float)        = 3.0
        _EdgeStrength ("Edge Strength", Range(0,2))   = 0.5

        _BaseAlpha    ("Base Cage Alpha", Range(0,1)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ScannerUnlit"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off          // two-sided: see scan on front and back faces

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionOS  : TEXCOORD2;
            };

            // Per-material props (SRP Batcher compatible: keep in one CBUFFER)
            CBUFFER_START(UnityPerMaterial)
                half4  _ScanColor;
                half4  _EdgeColor;
                float  _ScanY;
                float  _UseWorldY;
                float  _BandWidth;
                float  _Trail;
                float  _TrailStrength;
                float  _FresnelPower;
                float  _EdgeStrength;
                float  _BaseAlpha;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.positionWS  = pos.positionWS;
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionOS  = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float scanYCoord = lerp(IN.positionOS.y, IN.positionWS.y, saturate(_UseWorldY));

                // --- Scan band: bright line at _ScanY, falling off over _BandWidth ---
                float dist = abs(scanYCoord - _ScanY);
                float band = 1.0 - saturate(dist / max(_BandWidth, 1e-4));

                // --- Trail: residual glow on the "already scanned" side (below the line) ---
                float belowMask = step(scanYCoord, _ScanY);                       // 1 below the line
                float below     = saturate(1.0 - (_ScanY - scanYCoord) / max(_Trail, 1e-4));
                float trail     = below * belowMask * _TrailStrength;

                float scanMask = max(band, trail);

                // --- Fresnel rim so the cage edges always read, even un-scanned ---
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));
                float  fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // --- Compose ---
                half3 emission = _ScanColor.rgb * band
                               + _ScanColor.rgb * trail
                               + _EdgeColor.rgb * fresnel;

                float alpha = saturate(scanMask
                                       + fresnel * _EdgeStrength
                                       + _BaseAlpha);

                return half4(emission, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
