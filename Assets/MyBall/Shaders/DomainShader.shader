Shader "Unlit/DomainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HightMap("Hight Map", 2D) = "bump" {}
        _Xifen("Xifen", Range(0,2000)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma hull hs
            #pragma domain ds
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 tangent : TANGENT;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Xifen;
            /*
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            */


            [UNITY_domain("tri")]
            [UNITY_partitioning("fractional_odd")]
            [UNITY_outputtopology("triangle_cw")]
            [UNITY_patchconstantfunc("hsconst")]
            [UNITY_outputcontrolpoints(3)]
            v2f hs (InputPatch<v2f,3> v, uint id : SV_OutputControlPointID) {
                return v[id];
            }

            struct TessellationFactors {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            TessellationFactors hsconst (InputPatch<v2f,3> v) {
                TessellationFactors o;
                float4 tf;
                tf = float4(_Xifen,_Xifen,_Xifen,_Xifen);
                o.edge[0] = tf.x; 
                o.edge[1] = tf.y; 
                o.edge[2] = tf.z; 
                o.inside = tf.w;
                return o;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //v.vertex +=  tex2D(_MainTex,float4(o.uv,0,0) );
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [UNITY_domain("tri")]
            v2f ds (TessellationFactors tessFactors, const OutputPatch<v2f,3> vi, float3 bary : SV_DomainLocation) {
                
                appdata v;

                v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
                v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
                v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
                v.uv = vi[0].uv*bary.x + vi[1].uv*bary.y + vi[2].uv*bary.z;

                v2f o = vert (v);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
