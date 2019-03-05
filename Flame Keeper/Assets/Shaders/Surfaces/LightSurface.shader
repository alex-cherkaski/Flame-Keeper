Shader "FlameKeeperSurfaces/LightSurface"
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

		CGPROGRAM

		#pragma surface surf RampByDistance fullforwardshadows vertex:vert
		#include "UnityPBSLighting.cginc"
		#include "AutoLight.cginc"

		// Globals
		float3 _PlayerLightPosition;
		float _PlayerMaxLightRange;
		float _PlayerCurrentLightRange;
		float _AttenutaionIntensity;
		float _NormalLightIntensity;
		sampler2D _PlayerRampTex;

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

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 WorldPosition;
		};

		inline half4 LightingRampByDistance(SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
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

			// Only use ramped lighting on world position lights
			// You can turn off ramped lighting by setting the light's alpha to zero, which doesn't have any other effect
			float useRamp = min(_LightColor0.a, _WorldSpaceLightPos0.w);

			// Need to pass in world position here! But we can't do it through surface shaders!
			// Soultion: Genereate the vert frag shader from this and manually pass it in
			float distance = length(float3(_WorldSpaceLightPos0.xyz - s.WorldPosition));

			half3 color;
			if (useRamp > 0.5)
			{
				// Use ramped lighting for this light
				float falloff = 1.0 - (distance / 8.0); // Divide by max range of player light
				float cutoff = saturate(1.0 - floor(distance / _PlayerCurrentLightRange)); // Divide by current light range, should smooth this out
				float rampedFalloff = tex2D(_PlayerRampTex, half2(falloff, falloff)).r;

				half3 lightColor = lerp(_LightColor0.rgb, gi.light.color, _AttenutaionIntensity);
				float normalIntensity = lerp(1.0, nl, _NormalLightIntensity);
				color = BRDF3_Direct(s.Albedo, specColor, rlPow4, s.Smoothness) * rampedFalloff * lightColor * normalIntensity * cutoff * falloff;
			}
			else
			{
				// Normal lighting
				color = BRDF3_Direct(s.Albedo, specColor, rlPow4, s.Smoothness) * gi.light.color * nl;
			}
			color += BRDF3_Indirect(s.Albedo, specColor, gi.indirect, grazingTerm, fresnelTerm);

			half4 c = half4(color, 1);

			//half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
			c.rgb += UNITY_BRDF_GI(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
			c.a = outputAlpha;
			return c;
		}

		inline void LightingRampByDistance_GI(
			SurfaceOutputCustom s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Smoothness, s.Normal);
		}


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
