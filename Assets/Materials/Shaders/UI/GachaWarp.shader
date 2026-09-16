Shader "Custom/AnimeWhiteSpeedLinesUI"
{
    Properties
    {
        _MainTex ("UI Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0, 1)) = 0.0
        _LineCount ("Cantidad de Lineas", Float) = 50.0
        _AnimationSpeed ("Velocidad de Animacion", Float) = 24.0
        _CenterClearRadius ("Radio Limpio del Centro", Range(0.1, 0.5)) = 0.3
        _LineColor ("Color de Lineas", Color) = (1, 1, 1, 0.95)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _Progress;
            float _LineCount;
            float _AnimationSpeed;
            float _CenterClearRadius;
            fixed4 _LineColor;

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Centrar coordenadas UV (-0.5 a 0.5)
                float2 uv = i.uv - 0.5;

                // Coordenadas Polares
                float angle = atan2(uv.y, uv.x); 
                float radius = length(uv);        

                // Animación por fotogramas estilo Anime (24 fps)
                float timeFrame = floor(_Time.y * _AnimationSpeed);

                // Segmentar ángulo radial
                float normalizedAngle = (angle + 3.14159265) / (2.0 * 3.14159265);
                float lineIndex = floor(normalizedAngle * _LineCount);
                float angleFraction = frac(normalizedAngle * _LineCount);

                // Ruido estático por línea y fotograma
                float lineNoise = hash11(lineIndex * 17.13 + timeFrame * 0.41);
                float lengthNoise = hash11(lineIndex * 91.41 + timeFrame * 0.73);

                // Perfil afilado de cada línea
                float lineShape = smoothstep(0.0, 0.5, 0.5 - abs(angleFraction - 0.5));
                lineShape = pow(lineShape, 2.0);

                // Máscara FIJA de los bordes: El centro nunca es invadido
                float borderStart = _CenterClearRadius + (lengthNoise * 0.08);
                float radialMask = smoothstep(borderStart, borderStart + 0.15, radius);

                // Filtrar líneas aleatorias por fotograma
                float lineActive = step(0.3, lineNoise);

                // La transición solo controla el FADE (Aparición/Desaparición)
                float alpha = lineShape * radialMask * lineActive * _Progress;

                return fixed4(_LineColor.rgb, alpha * _LineColor.a);
            }
            ENDCG
        }
    }
}