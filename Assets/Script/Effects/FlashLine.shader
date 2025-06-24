Shader "Custom/FlashLine"
{
    Properties
    {
        _MainColor ("Flash Color", Color) = (1,1,1,1)
        _Power ("Glow Sharpness", Float) = 4.0
        _Fade ("Fade", Range(0,1)) = 1.0
        _Intensity ("Glow Intensity", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Lighting Off

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

            fixed4 _MainColor;
            float _Power;
            float _Fade;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float centerDist = abs(i.uv.x - 0.5); // 中心からの距離
                float alpha = pow(1.0 - centerDist, _Power) * _Fade;

                // 発光強調
                float3 emissionColor = _MainColor.rgb * _Intensity;
                return fixed4(emissionColor, alpha);
            }
            ENDCG
        }
    }
}
