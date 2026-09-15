Shader "UI/DiamondTransition"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _GridSize ("Grid Size", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            float _Progress;
            float _GridSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Ajuste de relación de aspecto para mantener rombos simétricos
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 st = i.uv;
                st.x *= aspect;

                // Repetición en cuadrícula
                float2 gridUV = frac(st * _GridSize) - 0.5;

                // Distancia Manhattan (Geometría de rombo)
                float diamondDist = abs(gridUV.x) + abs(gridUV.y);

                // Multiplicador 1.5 para asegurar que cubra bordes y esquinas
                float threshold = _Progress * 1.5;

                // Transición a color negro
                float alpha = step(diamondDist, threshold);

                return fixed4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
}