Shader "Unlit/HalfAlpha"
{
    Properties
	{
		_color("颜色",Color) = (1,1,1,1)
		_emiss("增幅",Float) = 1
		_rimPow("边缘强度",Range(0,5)) = 1
		_alpha("alpha",Range(0,1)) = 1
	}

	SubShader
	{
		Pass
		{

		    Cull off
			ZWrite on
			ColorMask 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 pos:POSITION;
            };
            struct v2f
            {
                float4 pos:SV_POSITION;
            };
            v2f vert(appdata i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.pos);
                return o;
            }
            float4 frag(v2f o):SV_Target
            {
                return float4(0,0,0,0);
            }
		    ENDCG
		}

		Pass
		{
		    Blend SrcAlpha OneMinusSrcAlpha
	        Tags{"Queue" = "Transparent"}
            ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
		    struct appdata
	        {
                float4 pos:POSITION;		
                float4 normal:NORMAL;
            };
            struct v2f
            {
                float4 pos:SV_POSITION;
                float3 normal_world:TEXCOORD0;
                float3 view_world:TEXCOORD1;
            };
            float4 _color;
            float _emiss;
            float _rimPow;
            float _alpha;
            v2f vert(appdata i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.pos);
                float3 normalWorld = mul(i.normal, unity_WorldToObject).xyz;
                o.normal_world = normalize(normalWorld);
                float4 worldPos = mul(unity_ObjectToWorld, i.pos);
                float3 viewWorld = _WorldSpaceCameraPos.xyz - worldPos.xyz;
                o.view_world = normalize(viewWorld);
                return o;
            }

            float4 frag(v2f o):SV_Target
            {
                float NdotV = dot(o.normal_world,o.view_world);
                float4 col = _color*_emiss;
                float rim = 1-saturate(NdotV);
                rim = pow(rim, _rimPow);
                col.a = rim*_emiss*_alpha;
                return col;
            }
		    ENDCG
		}
	}
}