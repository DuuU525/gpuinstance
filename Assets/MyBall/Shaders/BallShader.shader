Shader "Unlit/MyBall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_EdgeTex ("EdgeTexture", 2D) = "white" {}
        //_Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            //第一步： sharder 增加变体使用shader可以支持instance  
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            //剪裁后的instanceIDs
            StructuredBuffer<uint> clipBuffer;
            //存储三角形中心点坐标
            StructuredBuffer<float3> positionBuffer;
            //存储三角形的三个顶点索引
            StructuredBuffer<float3> trianglesBuffer;
            //所有顶点坐标
            StructuredBuffer<float3> verticesBuffer;

            struct appdata//如果是自定义mesh，需要mesh初始化vertices colors uv normals tangents..,这里才能使用
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                //第二步：instancID 加入顶点着色器输入结构 
                //UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                uint instanceID : SV_InstanceID;
                //第三步：instancID加入顶点着色器输出结构
                //UNITY_VERTEX_INPUT_INSTANCE_ID//uint instanceID : SV_InstanceID;同语义
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _EdgeTex;
            float4 _EdgeTex_ST;

            float4 _Color;
            
            v2f vert (appdata v, uint instanceID_idx : SV_InstanceID)
            {
                v2f o;
                //第四步：instanceid在顶点的相关设置  
                //UNITY_SETUP_INSTANCE_ID(v);
                //第五步：传递 instanceid 顶点到片元
                //UNITY_TRANSFER_INSTANCE_ID(v, o);
                uint instanceID = clipBuffer[instanceID_idx];


                float3 posCenter = positionBuffer[instanceID];//中心点坐标
                float3 ta = verticesBuffer[trianglesBuffer[instanceID].z];//三个顶点坐标，构成面abc
                float3 tb = verticesBuffer[trianglesBuffer[instanceID].y];
                float3 tc = verticesBuffer[trianglesBuffer[instanceID].x];
                float3 pos = ta * v.vertex.x + tb * v.vertex.y + tc * v.vertex.z;//v顶点投影到abc的坐标系中的坐标
                
                
                float4 posWorld = float4(pos, 1);//v的世界坐标
                o.vertex = mul(UNITY_MATRIX_VP, posWorld);//v的视口坐标
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);//纹理采样

                o.instanceID = instanceID;//实例id
                o.worldNormal = normalize(cross(tc - ta, tb - ta));//面abc的法线 //mul(v.normal, (float3x3)unity_WorldToObject);//
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //第六步：instanceid在片元的相关设置
                //UNITY_SETUP_INSTANCE_ID(i);
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);

                fixed3 albedo = tex2D(_MainTex, i.uv).rgb;//采样纹理
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;//环境光
                fixed3 worldNormal = normalize(i.worldNormal);
                fixed3 worldLigthDir = normalize(_WorldSpaceLightPos0.xyz);
                //fixed halfLambert = dot(worldNormal, worldLigthDir) * 0.5 + 0.5;//半lambert漫反射
                fixed halfLambert = saturate(dot(worldNormal, worldLigthDir));//漫反射（取0-1）
                fixed3 color = albedo * (ambient + halfLambert);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
