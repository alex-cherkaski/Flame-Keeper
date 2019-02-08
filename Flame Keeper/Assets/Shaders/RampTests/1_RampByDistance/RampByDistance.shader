Shader "Testing/1_RampByDistance"
{
	Properties
	{
		_Color("Color", color) = (0.5, 0.2, 0.3, 1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_RampTex("Ramp Texture", 2D) = "clear" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM

		#pragma surface surf RampByDistance fullforwardshadows
		#include "UnityPBSLighting.cginc"
		#include "AutoLight.cginc"

		half4 _Color;
		sampler2D _MainTex;
		sampler2D _RampTex;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos; // Get world position of our vertex
		};

		inline half4 LightingRampByDistance(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			return half4(s.Albedo, 1.0);
		}

		inline void LightingRampByDistance_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			// Get distance from light in the range of [0,1] (0: far away  1: on top of light source)
			float distance = length(float3(_WorldSpaceLightPos0.xyz - IN.worldPos.xyz));
			float RANGE_OF_LIGHT = 1.0 / _LightPositionRange.w;
			float falloff = clamp(1.0 - (distance / RANGE_OF_LIGHT), 0, 1);

			float isPointLight = _WorldSpaceLightPos0.w; // This is 1 for world space lights, 0 for directional lights

			// Using distance, sample the ramp texture to get falloff
			float rampedFalloff = tex2D(_RampTex, half2(falloff, falloff)).r * isPointLight;

			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color * rampedFalloff;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
