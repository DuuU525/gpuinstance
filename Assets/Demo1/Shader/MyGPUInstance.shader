Shader "Unlit/MyGPUInstance"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            
            //剪裁后的instanceIDs
            StructuredBuffer<uint> clipBuffer;
            //存储三角形中心点坐标
            StructuredBuffer<float3> positionBuffer;
            //存储三角形的三个顶点索引
            StructuredBuffer<float3> trianglesBuffer;
            //所有顶点坐标
            StructuredBuffer<float3> verticesBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                //第二步：instancID 加入顶点着色器输入结构 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                //第三步：instancID加入顶点着色器输出结构
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v, uint instanceID_ : SV_InstanceID)
            {
                v2f o;
                //第四步：instanceid在顶点的相关设置  
                UNITY_SETUP_INSTANCE_ID(v);
                //第五步：传递 instanceid 顶点到片元
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                uint instanceID = clipBuffer[instanceID_];
                
                float3 posCenter = positionBuffer[instanceID];//中心点坐标
                float3 ta = verticesBuffer[trianglesBuffer[instanceID].z];
                float3 tb = verticesBuffer[trianglesBuffer[instanceID].y];
                float3 tc = verticesBuffer[trianglesBuffer[instanceID].x];
                float3 pos = ta * v.vertex.x + tb * v.vertex.y + tc * v.vertex.z;

                float4 posWorld = float4(pos, 1); //v.vertex;
                o.vertex = mul(UNITY_MATRIX_VP, posWorld);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //第六步：instanceid在片元的相关设置
                UNITY_SETUP_INSTANCE_ID(i);
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
