// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Buffer/Normal"
{
    Properties
    {
        /*
            Any类型，根据右边给定的默认值决定类型
            "" {} 纹理
            1 Int
            1.0 Float 
            (1,1,1,1) Color
        */
        _MainTex ("Texture", Any) = "" {}
        // _MainTex ("Texture", Any) = "white" {} // 白色的默认纹理
        // _MainTex ("Texture", Any) = "bump" {} // 默认的内置法线贴图纹理
        _AddColor("AddColor", Color) = (0,0,0,0)
        [MaterialToggle] _UseAddColor("UseAddColor", Int) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Lighting Off
        
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Fog
        {
            Mode Off
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _AddColor;
            int _UseAddColor;
            sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            struct a2v {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(a2v v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color + (_UseAddColor ? _AddColor : 0);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); // 进行uv的变换
                // o.texcoord = v.texcoord;
                o.vertex = UnityPixelSnap(o.vertex);
                return o;
            }

            fixed4 frag(v2f f) : SV_Target {
                fixed4 col = tex2D(_MainTex, f.texcoord) * f.color; // uv纹理采样
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}