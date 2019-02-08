Shader "FlameKeeperStandard/RampLitSurface"
{
	Properties
	{
		_Color("Color", color) = (0.5, 0.2, 0.3, 1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_RampTex("Ramp Texture", 2D) = "clear" {}
		_Gain("Lightmap tone-mapping Gain", Float) = 1
		_Knee("Lightmap tone-mapping Knee", Float) = 0.5
		_Compress("Lightmap tone-mapping Compress", Float) = 0.33
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf StandardToneMappedGI fullforwardshadows

		#include "UnityPBSLighting.cginc"

		half4 _Color;
		half _Gain;
		half _Knee;
		half _Compress;
		sampler2D _MainTex;
		sampler2D _RampTex;

		inline half3 TonemapLight(half3 i) {
			i *= _Gain;
			return (i > _Knee) ? (((i - _Knee)*_Compress) + _Knee) : i;
		}

		// Pulled out the standard unity lighting so I can just work off of that
		inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			return half4(0, 0, 0, 0);
		}

		inline void LightingStandardToneMappedGI_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
