Shader "Custom/MoonGlowNoFog"
{
    Properties
    {
        [MainTexture] _MainTex ("Moon Sprite", 2D) = "white" {}
        [HDR] _GlowColor ("Moon Color & Glow", Color) = (1,1,1,1)
        _Intensity ("Glow Multiplier", Range(1, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Fog { Mode Off } 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _GlowColor;
            float _Intensity;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                // This multiplies the image color by our HDR glow color and intensity
                return col * _GlowColor * _Intensity;
            }
            ENDCG
        }
    }
}