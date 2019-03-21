Shader "FlameKeeperSurfaces/LightSurfaceNoCull"
{
	Properties
	{
		_Color("Color", color) = (0.5, 0.2, 0.3, 1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		[Header(Standard Parameters)]
		_Emission("Emission Color", color) = (0,0,0,0)
		_EmissionIntensity("Emission Intensity", float) = 1
		_Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0,1)) = 0

		[Header(Normal Mapping)]
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMapIntensity("Normal Map Intensity", Range(0,1)) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Cull Off

		CGPROGRAM

		#pragma surface surf RampByDistance fullforwardshadows vertex:vert

		#include "UnityPBSLighting.cginc"
		#include "AutoLight.cginc"
		#include "FlameKeeperInclude.cginc"

		half4 _Color;
		sampler2D _MainTex;

		half4 _Emission;
		half _EmissionIntensity;
		half _Metallic;
		half _Smoothness;

		sampler2D _NormalMap;
		half _NormalMapIntensity;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 worldPos;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		} // Vertex pass

		void surf(Input IN, inout SurfaceOutputCustom o)
		{
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;

			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Emission = _Emission.rgb * _EmissionIntensity;

			o.Normal = lerp(o.Normal, UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)), _NormalMapIntensity);
			o.WorldPosition = IN.worldPos;
		} // Surface pass

	ENDCG
	}

	FallBack "Diffuse"
}
