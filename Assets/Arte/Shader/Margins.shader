Shader "Custom/2D/Margins"
{
    Properties
    {
        [PerRendererData] _MainTex ("Base / Placa Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Movimiento Continuo del Objeto)]
        _ObjectFloatSpeed ("Velocidad Flote Objeto (X, Y)", Vector) = (1.2, 1.8, 0, 0)
        _ObjectFloatAmount ("Amplitud Flote Objeto (X, Y)", Vector) = (0.03, 0.05, 0, 0)
        _ObjectWobbleAngle ("Balanceo Rotacion Objeto (Grados)", Range(0.0, 15.0)) = 2.0
        _ObjectWobbleSpeed ("Velocidad Balanceo Objeto", Float) = 1.5

        [Header(Borde o Margen Negro Exterior)]
        _OuterBorderColor ("Color Borde Exterior", Color) = (0,0,0,1)
        _OuterBorderSize ("Grosor Borde Exterior", Range(0.0, 0.25)) = 0.04
        _OuterBorderSoftness ("Suavizado Borde Exterior", Range(0.001, 0.08)) = 0.01

        [Header(Direccion y Rotacion del Efecto)]
        [KeywordEnum(All, Top, Right, Bottom, Left)] _EffectSide ("Lado del Efecto", Float) = 0
        _EffectRotation ("Rotacion Sentido (Grados)", Range(0.0, 360.0)) = 0.0
        _SideFalloff ("Suavizado Lado", Range(0.05, 1.0)) = 0.35

        [Header(Textura Interna Tipo Lampara de Lava)]
        _TileTex ("Textura Interna", 2D) = "white" {}
        _TileTint ("Color Textura Interna", Color) = (1,1,1,1)
        _LavaFlowSpeed ("Velocidad Flujo (X, Y)", Vector) = (0.08, 0.28, 0, 0)
        _LavaDistortion ("Fuerza Ondulacion Lava", Range(0.0, 0.25)) = 0.08
        _LavaScale ("Escala Ondas Lava", Range(1.0, 12.0)) = 4.0
        _LavaThreshold ("Umbral Burbujas Lava", Range(0.0, 0.95)) = 0.35
        _LavaSmoothness ("Suavizado Burbujas Lava", Range(0.01, 0.4)) = 0.12
        _InnerMargin ("Margen Interior Limite", Range(0.0, 0.35)) = 0.08
        _EdgeSoftness ("Suavizado Limite Interior", Range(0.001, 0.1)) = 0.015

        [Header(Tiling y Offset Aleatorios Lava)]
        _RandomSpeed ("Velocidad Variacion Aleatoria", Float) = 1.2
        _TileMin ("Tiling Minimo (X, Y)", Vector) = (0.8, 0.8, 0, 0)
        _TileMax ("Tiling Maximo (X, Y)", Vector) = (1.6, 1.6, 0, 0)
        _OffsetMin ("Offset Minimo (X, Y)", Vector) = (-0.25, -0.25, 0, 0)
        _OffsetMax ("Offset Maximo (X, Y)", Vector) = (0.25, 0.25, 0, 0)

        [Header(Textura Superior)]
        _TopTex ("Textura Superior", 2D) = "white" {}
        _TopTint ("Color Textura Superior", Color) = (1,1,1,1)
        _TopScrollSpeed ("Velocidad Scroll Superior (X, Y)", Vector) = (0.0, 0.0, 0, 0)
        _TopOpacity ("Opacidad Textura Superior", Range(0.0, 1.0)) = 0.5
        _RespectInnerMarginTop ("Limitar Superior al Margen (0-1)", Range(0.0, 1.0)) = 0.0

        [Header(Tiling y Offset Aleatorios Textura Superior)]
        _TopRandomSpeed ("Velocidad Variacion Superior", Float) = 1.4
        _TopTileMin ("Tiling Minimo Superior (X, Y)", Vector) = (0.9, 0.9, 0, 0)
        _TopTileMax ("Tiling Maximo Superior (X, Y)", Vector) = (1.5, 1.5, 0, 0)
        _TopOffsetMin ("Offset Minimo Superior (X, Y)", Vector) = (-0.2, -0.2, 0, 0)
        _TopOffsetMax ("Offset Maximo Superior (X, Y)", Vector) = (0.2, 0.2, 0, 0)

        [Header(Resplandor por Todo el Contorno)]
        _GlowColor ("Color Resplandor", Color) = (0.2, 0.9, 1.0, 1.0)
        [HDR] _GlowIntensity ("Intensidad Resplandor", Range(0.0, 5.0)) = 2.0
        _BorderThickness ("Grosor del Borde", Range(0.005, 0.2)) = 0.04
        _TravelSpeed ("Velocidad Recorrido", Float) = 1.5
        _PulseCount ("Cantidad de Pulsos", Range(1.0, 8.0)) = 2.0
        _PulseSharpness ("Concentracion del Pulso", Range(1.0, 16.0)) = 5.0
        _FullPerimeterGlow ("Glow en Todo el Contorno (0-1)", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _EFFECTSIDE_ALL _EFFECTSIDE_TOP _EFFECTSIDE_RIGHT _EFFECTSIDE_BOTTOM _EFFECTSIDE_LEFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_TileTex);
            SAMPLER(sampler_TileTex);
            float4 _TileTex_ST;

            TEXTURE2D(_TopTex);
            SAMPLER(sampler_TopTex);
            float4 _TopTex_ST;

            float4 _Color;
            float2 _ObjectFloatSpeed;
            float2 _ObjectFloatAmount;
            float _ObjectWobbleAngle;
            float _ObjectWobbleSpeed;

            float4 _OuterBorderColor;
            float _OuterBorderSize;
            float _OuterBorderSoftness;
            float _EffectSide;
            float _EffectRotation;
            float _SideFalloff;
            float4 _TileTint;
            float2 _LavaFlowSpeed;
            float _LavaDistortion;
            float _LavaScale;
            float _LavaThreshold;
            float _LavaSmoothness;
            float _InnerMargin;
            float _EdgeSoftness;

            float _RandomSpeed;
            float2 _TileMin;
            float2 _TileMax;
            float2 _OffsetMin;
            float2 _OffsetMax;

            float4 _TopTint;
            float2 _TopScrollSpeed;
            float _TopOpacity;
            float _RespectInnerMarginTop;

            float _TopRandomSpeed;
            float2 _TopTileMin;
            float2 _TopTileMax;
            float2 _TopOffsetMin;
            float2 _TopOffsetMax;

            float4 _GlowColor;
            float _GlowIntensity;
            float _BorderThickness;
            float _TravelSpeed;
            float _PulseCount;
            float _PulseSharpness;
            float _FullPerimeterGlow;

            float Hash11(float n)
            {
                return frac(sin(n * 127.1 + 311.7) * 43758.5453123);
            }

            float SmoothRandom1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(Hash11(i), Hash11(i + 1.0), u);
            }

            float2 RotateVec2(float2 v, float degreesAngle)
            {
                float rad = radians(degreesAngle);
                float s = sin(rad);
                float c = cos(rad);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float t = _Time.y;
                float3 posOS = IN.positionOS.xyz;

                float wobbleDeg = sin(t * _ObjectWobbleSpeed) * _ObjectWobbleAngle;
                posOS.xy = RotateVec2(posOS.xy, wobbleDeg);

                posOS.x += sin(t * _ObjectFloatSpeed.x) * _ObjectFloatAmount.x;
                posOS.y += cos(t * _ObjectFloatSpeed.y) * _ObjectFloatAmount.y;

                OUT.positionHCS = TransformObjectToHClip(posOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float2 RotateUVAroundCenter(float2 uv, float degreesAngle)
            {
                return RotateVec2(uv - 0.5, degreesAngle) + 0.5;
            }

            float ComputeSideMask(float2 dirUV, float falloff)
            {
                #if defined(_EFFECTSIDE_TOP)
                    return smoothstep(0.5 - falloff, 0.5 + falloff, dirUV.y);
                #elif defined(_EFFECTSIDE_RIGHT)
                    return smoothstep(0.5 - falloff, 0.5 + falloff, dirUV.x);
                #elif defined(_EFFECTSIDE_BOTTOM)
                    return smoothstep(0.5 + falloff, 0.5 - falloff, dirUV.y);
                #elif defined(_EFFECTSIDE_LEFT)
                    return smoothstep(0.5 + falloff, 0.5 - falloff, dirUV.x);
                #else
                    return 1.0;
                #endif
            }

            float ComputeInnerRectMask(float2 uv, float margin, float softness)
            {
                float minBound = margin;
                float maxBound = 1.0 - margin;

                float mx = smoothstep(minBound - softness, minBound + softness, uv.x) *
                           (1.0 - smoothstep(maxBound - softness, maxBound + softness, uv.x));

                float my = smoothstep(minBound - softness, minBound + softness, uv.y) *
                           (1.0 - smoothstep(maxBound - softness, maxBound + softness, uv.y));

                return saturate(mx * my);
            }

            float ComputeOuterBorderMask(float2 uv, float borderSize, float softness)
            {
                float distToEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                return 1.0 - smoothstep(borderSize - softness, borderSize + softness, distToEdge);
            }

            float ComputeEdgeProximity(float2 uv, float thickness)
            {
                float distToEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                return 1.0 - smoothstep(0.0, thickness, distToEdge);
            }

            float ComputeRectPerimeter01(float2 uv)
            {
                float2 p = uv - 0.5;
                float2 a = abs(p);
                if (a.x > a.y)
                {
                    return (p.x > 0.0) ? (0.25 + 0.125 * (p.y / max(a.x, 0.0001) + 1.0))
                                       : (0.75 - 0.125 * (p.y / max(a.x, 0.0001) + 1.0));
                }
                else
                {
                    return (p.y > 0.0) ? (0.0 + 0.125 * (p.x / max(a.y, 0.0001) + 1.0))
                                       : (0.5 - 0.125 * (p.x / max(a.y, 0.0001) + 1.0));
                }
            }

            float ComputeLavaBlobField(float2 uv, float timeVal, float scale, float2 flowSpeed)
            {
                float2 flowUV = uv * scale - timeVal * flowSpeed;
                float w1 = sin(flowUV.x * 3.14159265 + sin(flowUV.y * 2.5 + timeVal * 0.9));
                float w2 = cos(flowUV.y * 3.14159265 - cos(flowUV.x * 2.2 - timeVal * 0.7));
                float w3 = sin((flowUV.x + flowUV.y) * 2.0 + timeVal * 1.1);
                return saturate(0.5 + 0.25 * w1 + 0.2 * w2 + 0.1 * w3);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y;
                float rt = t * _RandomSpeed;

                float2 randTileT = float2(SmoothRandom1D(rt + 1.3), SmoothRandom1D(rt + 7.9));
                float2 randOffsetT = float2(SmoothRandom1D(rt + 19.4), SmoothRandom1D(rt + 33.7));

                float2 dynTiling = lerp(_TileMin, _TileMax, randTileT) * _TileTex_ST.xy;
                float2 dynOffset = lerp(_OffsetMin, _OffsetMax, randOffsetT) + _TileTex_ST.zw;

                float2 effectUV = RotateUVAroundCenter(uv, _EffectRotation);
                float sideMask = ComputeSideMask(effectUV, _SideFalloff);
                float activeSideMask = lerp(sideMask, 1.0, _FullPerimeterGlow);

                half4 baseSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * IN.color;

                float lavaField = ComputeLavaBlobField(uv, t, _LavaScale, _LavaFlowSpeed);
                float lavaBlobMask = smoothstep(_LavaThreshold - _LavaSmoothness, _LavaThreshold + _LavaSmoothness, lavaField);

                float2 warpOffset = float2(
                    sin(uv.y * _LavaScale * 3.0 + t * 1.4) + cos((uv.x + uv.y) * _LavaScale * 2.0 - t * 0.8),
                    cos(uv.x * _LavaScale * 3.0 - t * 1.2) + sin((uv.x - uv.y) * _LavaScale * 2.0 + t * 1.0)
                ) * _LavaDistortion;

                float2 lavaUV = uv * dynTiling + dynOffset + warpOffset - (t * _LavaFlowSpeed * 0.35);
                half4 tileSample = SAMPLE_TEXTURE2D(_TileTex, sampler_TileTex, lavaUV) * _TileTint;

                float innerMask = ComputeInnerRectMask(uv, _InnerMargin, _EdgeSoftness);
                float tileAlpha = tileSample.a * lavaBlobMask * innerMask * baseSample.a * sideMask;

                half3 composedRGB = lerp(baseSample.rgb, tileSample.rgb, tileAlpha);

                float trt = t * _TopRandomSpeed;
                float2 topRandTileT = float2(SmoothRandom1D(trt + 41.2), SmoothRandom1D(trt + 53.6));
                float2 topRandOffsetT = float2(SmoothRandom1D(trt + 67.8), SmoothRandom1D(trt + 89.1));

                float2 topDynTiling = lerp(_TopTileMin, _TopTileMax, topRandTileT) * _TopTex_ST.xy;
                float2 topDynOffset = lerp(_TopOffsetMin, _TopOffsetMax, topRandOffsetT) + _TopTex_ST.zw;

                float2 topUV = uv * topDynTiling + topDynOffset + (t * _TopScrollSpeed);
                half4 topSample = SAMPLE_TEXTURE2D(_TopTex, sampler_TopTex, topUV) * _TopTint;
                float topInnerMask = lerp(1.0, innerMask, _RespectInnerMarginTop);
                float topAlpha = topSample.a * _TopOpacity * topInnerMask * baseSample.a;

                composedRGB = lerp(composedRGB, topSample.rgb, topAlpha);

                float outerBorderMask = ComputeOuterBorderMask(uv, _OuterBorderSize, _OuterBorderSoftness) * step(0.001, baseSample.a) * _OuterBorderColor.a;
                composedRGB = lerp(composedRGB, _OuterBorderColor.rgb, outerBorderMask);

                float borderBand = ComputeEdgeProximity(uv, _BorderThickness) * step(0.001, baseSample.a);
                float perim01 = frac(ComputeRectPerimeter01(effectUV) + (_EffectRotation / 360.0));

                float wavePhase = (perim01 * _PulseCount) - (t * _TravelSpeed);
                float travelingWave = pow(saturate(0.5 + 0.5 * cos(wavePhase * 6.2831853)), _PulseSharpness);

                float shimmer = 0.75 + 0.25 * sin(t * 4.5 + perim01 * 18.8495559);
                float glowMask = borderBand * travelingWave * shimmer * activeSideMask;

                composedRGB += _GlowColor.rgb * glowMask * _GlowIntensity;

                return half4(composedRGB, baseSample.a);
            }
            ENDHLSL
        }
    }
}