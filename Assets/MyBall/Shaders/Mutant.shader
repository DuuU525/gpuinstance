Shader "Unlit/Mutant"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            int _ModelTransCount;
            float _AnimBlendTime;
            float _AnimFrame;
            
            struct ModelAnimationClip
            {
                int startFrame;
                int endFrame;
                int looping;
                float len;
            };
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                int4 boneId : BLENDINDICES;
                float4 boneWeight : BLENDWEIGHTS;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 nDirWS : TEXCOORD0;
                float3 tDirWS : TEXCOORD1;
                float3 bDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float2 uv_normal : TEXCOORD4;
                float3 normal : TEXCOORD5;
            };

            
            StructuredBuffer<float4x4> _transBuffer;//移动变换矩阵
            StructuredBuffer<ModelAnimationClip> _AnimClipBuffer;//动画片段
            StructuredBuffer<float4x4> _AnimBuffer;//动画矩阵

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            //获得动画变换矩阵
            //float4x4 GetAnimTrans(uint instanceID_idx)
            //{
            //return float4x4();
            //}
            //混合动画网格
            float4 BlendSkinMesh()
            {

            }


            v2f vert (appdata v, uint instanceID_idx : SV_InstanceID)
            {
                v2f o;
                float4x4 trans = _transBuffer[instanceID_idx];//移动变换矩阵
                //float4x4 transAnim = GetAnimTrans(instanceID_idx);
                
                float4 posWorld = mul(unity_ObjectToWorld, v.vertex);//模型顶点转换到世界坐标系
                float4 posTrans = mul(trans, posWorld);//通过变换矩阵到对应世界坐标
                float3 posNormal = UnityObjectToWorldNormal(v.normal);//顶点法线转换到世界坐标系
                float3 normalTrans = normalize(mul(trans, float4(posNormal, 0)));//变换矩阵变换法线
                o.normal = normalTrans;//世界坐标系下的法线
                o.vertex = mul(UNITY_MATRIX_VP, posTrans);//顶点转换到视图/投影矩阵
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);//纹理采样
                o.uv_normal = TRANSFORM_TEX(v.uv, _NormalMap);//法线采样

                o.nDirWS = normalTrans;//世界坐标系下的法线
                o.tDirWS = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz , 0).xyz));
                o.bDirWS = normalize(cross(o.nDirWS, o.tDirWS) * v.tangent.w);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float3 nDirTS = tex2D(_NormalMap, i.uv_normal);
                float3x3 TBN = float3x3(i.tDirWS, i.bDirWS, i.nDirWS);
                float3 nDirWS = mul(nDirTS, TBN);//i.normal;// 

                fixed3 col = tex2D(_MainTex, i.uv).rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyx;
                fixed3 worldLigthDir = normalize(_WorldSpaceLightPos0.xyx);
                fixed halfLambert = saturate(dot(nDirWS, worldLigthDir));
                fixed3 color = col * (ambient + halfLambert);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
