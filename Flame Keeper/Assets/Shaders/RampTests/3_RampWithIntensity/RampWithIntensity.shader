Shader "Testing/3_RampWithIntensity"
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
			s.Normal = normalize(s.Normal);

			half oneMinusReflectivity;
			half3 specColor;
			s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

			// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
			// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
			half outputAlpha;
			s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

			half3 reflDir = reflect(viewDir, s.Normal);

			half nl = saturate(dot(s.Normal, gi.light.dir));
			half nv = saturate(dot(s.Normal, viewDir));

			// Vectorize Pow4 to save instructions
			half2 rlPow4AndFresnelTerm = Pow4(half2(dot(reflDir, gi.light.dir), 1 - nv));  // use R.L instead of N.H to save couple of instructions
			half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
			half fresnelTerm = rlPow4AndFresnelTerm.y;

			half grazingTerm = saturate(s.Smoothness + (1 - oneMinusReflectivity));

			// ---------------
			// Our code here!
			// ---------------
			float isPointLight = _WorldSpaceLightPos0.w; 
			float distance = length(float3(_WorldSpaceLightPos0.xyz));
			float RANGE_OF_LIGHT = 1.0 / _LightPositionRange.w;
			float falloff = 1.0 - (distance / RANGE_OF_LIGHT);
			float rampedFalloff = tex2D(_RampTex, half2(falloff, falloff)).r * isPointLight;

			half3 color = BRDF3_Direct(s.Albedo, specColor, rlPow4, s.Smoothness);
			color *= gi.light.color * nl;
			color += BRDF3_Indirect(s.Albedo, specColor, gi.indirect, grazingTerm, fresnelTerm);

			half4 c = half4(color, 1);

			//half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
			c.rgb += UNITY_BRDF_GI(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
			c.a = outputAlpha;
			return c;
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
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
