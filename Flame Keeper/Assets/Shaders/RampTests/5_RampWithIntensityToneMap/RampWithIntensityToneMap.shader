Shader "Testing/5_RampWithIntensityToneMap"
{
	Properties
	{
		_Color("Color", color) = (0.5, 0.2, 0.3, 1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_RampTex("Ramp Texture", 2D) = "clear" {}

		[Header(Standard Parameters)]
		_Emission("Emission Color", color) = (0,0,0,0)
		_EmissionIntensity("Emission Intensity", float) = 1
		_Metallic("Metallic", Range(0, 1)) = 0
		_Smoothness("Smoothness", Range(0,1)) = 0

		[Header(Normal Mapping)]
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMapIntensity("Normal Map Intensity", Range(0,1)) = 1.0

		[Header(Tone Mapping)]
		_Gain("Lightmap tone-mapping Gain", Float) = 1
		_Knee("Lightmap tone-mapping Knee", Float) = 0.5
		_Compress("Lightmap tone-mapping Compress", Float) = 0.33
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

		half4 _Emission;
		half _EmissionIntensity;
		half _Metallic;
		half _Smoothness;

		sampler2D _NormalMap;
		half _NormalMapIntensity;

		half _Gain;
		half _Knee;
		half _Compress;		

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_NormalMap;
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

			// Need to pass in world position here! But we can't do it through surface shaders!
			// Soultion: Genereate the vert frag shader from this and manually pass it in
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

		inline half3 TonemapLight(half3 i) 
		{
			i *= _Gain;
			return (i > _Knee) ? (((i - _Knee)*_Compress) + _Knee) : i;
		}

		inline void LightingRampByDistance_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);

			gi.light.color = TonemapLight(gi.light.color);
			#ifdef DIRLIGHTMAP_SEPARATE
			#ifdef LIGHTMAP_ON
				gi.light2.color = TonemapLight(gi.light2.color);
			#endif
			#ifdef DYNAMICLIGHTMAP_ON
				gi.light3.color = TonemapLight(gi.light3.color);
			#endif
			#endif
			gi.indirect.diffuse = TonemapLight(gi.indirect.diffuse);
			gi.indirect.specular = TonemapLight(gi.indirect.specular);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;

			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Emission = _Emission.rgb * _EmissionIntensity;

			o.Normal = lerp(o.Normal, UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)), _NormalMapIntensity);
		}

		ENDCG
	}
	FallBack "Diffuse"
}
