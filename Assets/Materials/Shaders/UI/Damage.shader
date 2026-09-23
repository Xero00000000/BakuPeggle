Shader "Custom/Damage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        
        [Header(Lightning Damage Settings)]
        _Progress ("Intensidad Rayos", Range(0, 1)) = 0.0
        _RayThickness ("Grosor de Rayos", Range(0.1, 2.0)) = 0.6
        _LineCount ("Cantidad de Rayos", Float) = 28.0
        _AnimationSpeed ("Velocidad de Descarga", Float) = 30.0
        _Jaggedness ("Deformacion Zig-Zag", Float) = 12.0
        _CenterClearRadius ("Radio Limpio del Centro", Range(0.01, 0.5)) = 0.15
        _LineColor ("Color de Rayos / Energia", Color) = (0.2, 0.8, 1.0, 1.0)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "CanUseSpriteAtlas"="True"
        }

        Cull Off 
        ZWrite Off 
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float _Progress;
            float _RayThickness;
            float _LineCount;
            float _AnimationSpeed;
            float _Jaggedness;
            float _CenterClearRadius;
            fixed4 _LineColor;

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Coordenadas Polares centradas en la pantalla (-0.5 a 0.5)
                float2 uv = i.uv - 0.5;
                float radius = length(uv);        
                float angle = atan2(uv.y, uv.x); 

                // Tiempo a alta velocidad para chisporroteo eléctrico
                float timeFrame = floor(_Time.y * _AnimationSpeed);
                float fastFlicker = hash11(timeFrame * 0.19);

                // Distorsión en zig-zag (efecto relámpago)
                float jagNoise = (hash11(floor(radius * 25.0) + timeFrame) - 0.5) * (_Jaggedness * 0.01);
                angle += jagNoise;

                // Segmentar ángulo radial
                float normalizedAngle = (angle + 3.14159265) / (2.0 * 3.14159265);
                float lineIndex = floor(normalizedAngle * _LineCount);
                float angleFraction = frac(normalizedAngle * _LineCount);

                // Ruido estático por rama eléctrica
                float lineNoise = hash11(lineIndex * 17.13 + timeFrame * 0.41);
                float lengthNoise = hash11(lineIndex * 91.41 + timeFrame * 0.73);

                // Perfil anco y suave del rayo eléctrico (Grosor regulado por _RayThickness)
                float distanceToCenter = abs(angleFraction - 0.5) * 2.0;
                float lineShape = 1.0 - saturate(distanceToCenter / _RayThickness);
                lineShape = pow(lineShape, 2.5); // Exponente menor para que sea más ancha la base

                // Máscara Radial: Mantiene libre el centro de la pantalla
                float borderStart = _CenterClearRadius + (lengthNoise * 0.06);
                float radialMask = smoothstep(borderStart, borderStart + 0.15, radius);

                // Activar ramas al azar + parpadeo
                float lineActive = step(0.2, lineNoise) * (0.7 + 0.3 * fastFlicker);

                // Intensidad acumulada
                float lightningAlpha = lineShape * radialMask * lineActive * _Progress;

                // Color final del rayo con resplandor amplio
                fixed3 energyColor = _LineColor.rgb * (1.8 + fastFlicker);
                return fixed4(energyColor, lightningAlpha * _LineColor.a);
            }
            ENDCG
        }
    }
}