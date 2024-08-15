Shader "Unlit/Edage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeTex ("EdgeTexture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _EdgeTex;
            float4 _EdgeTex_ST;

            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                return float4(_Color.rgb, 1);
                //fixed4 col;
                //col = tex2D(_MainTex, i.uv);
                //col.rgb *= _Color.rgb;
                //fixed4 colEdge = tex2D(_EdgeTex, i.uv)；
                
                //return col;
            }
            ENDCG
        }
    }
}
