
Shader "azhao/DisplacementTest"
{
	Properties
	{
		_heightTex("heightTex", 2D) = "white" {}
		_heightTexVal("heightTexVal",float) = 0.01
		_TessValue("Max Tessellation", Range(1, 32)) = 15
		_normalTex("normalTex", 2D) = "white" {}
		_height("height", Float) = 0
		_displacement("displacement",float) = 1
		_minDist("minDist",float) = 10
		_maxDist("maxDist",float) = 25
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			half filler;
			float2 uv_texcoord;
		};

		uniform sampler2D _heightTex;
		uniform float4 _heightTex_ST;
		uniform float _height;
		uniform float _TessValue;
		uniform sampler2D _normalTex;
		uniform float4 _normalTex_ST;
		float _displacement;
		float _heightTexVal;
		float _minDist;
		float _maxDist;
		float4 tessFunction(appdata_full v0, appdata_full v1, appdata_full v2)
		{
			//这里要说明一下，传进来三个点，不能直接求平均值，而要逐个点去采样
			//因为只要有一个点在需要细分的范围内，这整个网格就需要细分，不然凹凸的边缘会和不需要细分的网格裂开
			float2 uv0 = v0.texcoord * _heightTex_ST.xy + _heightTex_ST.zw;
			float col0 = (tex2Dlod(_heightTex, float4(uv0, 0, 0.0)).r - 0.5);
			float2 uv1 = v1.texcoord * _heightTex_ST.xy + _heightTex_ST.zw;
			float col1 = (tex2Dlod(_heightTex, float4(uv1, 0, 0.0)).r - 0.5);
			float2 uv2 = v2.texcoord * _heightTex_ST.xy + _heightTex_ST.zw;
			float col2 = (tex2Dlod(_heightTex, float4(uv2, 0, 0.0)).r - 0.5);
			float col = max(abs(col0), abs(col1));
			col = max(col, abs(col2));
			col = step( _heightTexVal, col);
			col = col * _displacement;
			col = max(col, 0.01f);
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _minDist, _maxDist, col);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 uv_heightTex = v.texcoord * _heightTex_ST.xy + _heightTex_ST.zw;
			float temp_output_4_0 = ( tex2Dlod( _heightTex, float4( uv_heightTex, 0, 0.0) ).r - 0.5 );
			float3 appendResult13 = (float3(0.0 , ( temp_output_4_0 * _height ) , 0.0));
			v.vertex.xyz += appendResult13;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color16 = IsGammaSpace() ? float4(0.5660378,0.5660378,0.5660378,0) : float4(0.280335,0.280335,0.280335,0);
			float2 uv_normalTex = i.uv_texcoord * _normalTex_ST.xy + _normalTex_ST.zw;
			o.Normal = UnpackNormal(tex2D(_normalTex, uv_normalTex));
			o.Albedo = color16.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}