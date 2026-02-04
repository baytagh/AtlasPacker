Shader "Buffer/NormalClearing"
{
    Properties
    {
        _MainTex("Main Texture", Any) = "" {}
        _BasePos("Base Pos", Vector) = (0,0,1,1) // 绘制区域
        [MaterialToggle] PixelSnap("Pixel Snap", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "QUEUE" = "TransParent"
        }

        Pass
        {
            Tags
            {
                "RenderType"="Transparent" "QUEUE" = "TransParent"
            }
            ZTest Always
            ZWrite Off
            Fog
            {
                Mode Off
            }
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
                float3 verview : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BasePos;
            
            inline bool Between(float val, float min_val, float max_val) {
                return val >= min_val && val <= max_val;
            }
            
            inline bool IsInRect(float2 pos, float l, float t, float r, float b) {
                return Between(pos.x, l, r) && Between(pos.y, t, b);
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.verview = UnityObjectToViewPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 偏移uv
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // 要绘制的位置
                float x = _BasePos.x;
                float y = _BasePos.y;
                float w = _BasePos.z;
                float h = _BasePos.w;

                if (!IsInRect(i.verview.xy, x, y, x + w, y + h)) {
                    // 当不在绘制范围时
                    return fixed4(0, 0, 0, 0);
                }
                float uvx = (i.verview.x - x) / w;
                float uvy = (i.verview.y - y) / h;
                float2 uv = float2(uvx, uvy);
                // sample the texture
                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}