Shader "FlameKeeperStandard/RampLitSurface"
{
	Properties
	{
		[Header(Standard Parameters)]
		_Color("Color", color) = (0.5, 0.2, 0.3, 1.0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[Header(Normal Mapping)]
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMapIntensity("Normal Map Intensity", Range(0,1)) = 1.0

		[Header(Ramp Texture)]
		_RampTex("Ramp Texture", 2D) = "clear" {}

		[Header(Tone Mapping)]
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
			sampler2D _MainTex;
			sampler2D _RampTex;
			sampler2D _NormalMap;

			half _Glossiness;
			half _Metallic;

			half _NormalMapIntensity;

			half _Gain;
			half _Knee;
			half _Compress;

			struct Input
			{
				float2 uv_MainTex;
				float2 uv_NormalMap;
				float2 uv_DetailTex;
			};

			inline half3 TonemapLight(half3 i) 
			{
				i *= _Gain;
				return (i > _Knee) ? (((i - _Knee)*_Compress) + _Knee) : i;
			}

			// Pulled out the standard unity lighting so I can just work off of that
			inline half4 LightingStandardToneMappedGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
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

				// Ramp lighting
				half3 ramp = tex2D(_RampTex, float2(nl, nl)).rgb;

				// Vectorize Pow4 to save instructions
				half2 rlPow4AndFresnelTerm = Pow4(half2(dot(reflDir, gi.light.dir), 1 - nv));  // use R.L instead of N.H to save couple of instructions
				half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
				half fresnelTerm = rlPow4AndFresnelTerm.y;

				half grazingTerm = saturate(s.Smoothness + (1 - oneMinusReflectivity));

				half3 color = BRDF3_Direct(s.Albedo, specColor, rlPow4, s.Smoothness) * half4(ramp,1);
				color *= gi.light.color;
				color += BRDF3_Indirect(s.Albedo, specColor, gi.indirect, grazingTerm, fresnelTerm);

				half4 c = half4(color, 1);

				//half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
				c.rgb += UNITY_BRDF_GI(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
				c.a = outputAlpha;
				return c;
			}

			inline void LightingStandardToneMappedGI_GI(
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
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

				o.Albedo = c.rgb * _Color;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;

				// Determine normal based on intensity
				o.Normal = lerp(o.Normal, UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)), _NormalMapIntensity);
			}

			ENDCG
		}

	FallBack "Diffuse"
}








/*
Shader "Custom/RampSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
*/