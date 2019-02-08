Shader "Testing/2_RampByNormal"
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

		#pragma surface surf RampByNormal fullforwardshadows
		#include "UnityPBSLighting.cginc"

		half4 _Color;
		sampler2D _MainTex;
		sampler2D _RampTex;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos; // Get world position of our vertex
			float3 worldNormal; // Get world normal of our vertex
		};

		inline half4 LightingRampByNormal(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			return half4(s.Albedo, 1.0);
		}

		inline void LightingRampByNormal_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			// Get angle (dot product) between surface normal and light direction
			// 0 -> vectors are orthogonal, no light
			// 1 -> direct hit to light source
			float3 dirToLight = normalize(_WorldSpaceLightPos0.xyz - IN.worldPos.xyz);
			float normalDistance = saturate(dot(IN.worldNormal, dirToLight));

			// Get distance from light in the range of [0,1] (0: far away  1: on top of light source)
			float distance = length(float3(_WorldSpaceLightPos0.xyz - IN.worldPos.xyz));
			float RANGE_OF_LIGHT = 10.0;
			float falloff = clamp(1.0 - (distance / RANGE_OF_LIGHT), 0, 1);

			// Using distance, sample the ramp texture to get falloff
			float isPointLight = _WorldSpaceLightPos0.w; // This is 1 for world space lights, 0 for directional lights
			float rampedFalloff = tex2D(_RampTex, half2(normalDistance, normalDistance)).r * isPointLight;

			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color * rampedFalloff * falloff;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
