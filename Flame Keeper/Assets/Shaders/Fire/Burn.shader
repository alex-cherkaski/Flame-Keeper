Shader "FlameKeeperSurfaces/Burn"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

		_LowColor ("Low Heat Color", Color) = (1,1,1,1)
		_HighColor ("High Heat Color", Color) = (1,1,1,1)

		_ColorDistortion ("Color Distortion Internsity", float) = 1.0
		_HeatDistortion ("Heat Distortion", float) = 0.2
		_PanSpeed ("Pan Speed", float) = 1.0
		_Contrast ("Contrast Intensity", float) = 1.0

		_BurnHeight ("Burn Height (World Space)", float) = 0.0
		_DissolveOffset ("Dissolve Offset", float) = 0.2

		_FlowTex ("Flow Map", 2D) = "clear" {}
		_CloudTex ("Could Map", 2D) = "clear" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

		Cull Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf RampByDistance fullforwardshadows addshadow
		#include "UnityPBSLighting.cginc"
		#include "AutoLight.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		// Globals
		float3 _PlayerLightPosition;
		float _PlayerMaxLightRange;
		float _PlayerCurrentLightRange;
		float _AttenutaionIntensity;
		float _NormalLightIntensity;
		sampler2D _PlayerRampTex;

        sampler2D _MainTex;

		sampler2D _FlowTex;
		sampler2D _CloudTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

		half4 _LowColor;
		half4 _HighColor;

		float _ColorDistortion;
		float _HeatDistortion;
		float _PanSpeed;
		float _Contrast;

		float _BurnHeight;
		float _DissolveOffset;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

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

        void surf (Input IN, inout SurfaceOutputCustom o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

			float2 worldUV = IN.uv_MainTex;

			// Unpack from flow map
			float2 flowDistortion = UnpackNormal(tex2D(_FlowTex, worldUV)).xy;
			flowDistortion *= _ColorDistortion;

			// Calculate burn dissolve step
			float2 stepDistortion = UnpackNormal(tex2D(_FlowTex, worldUV + _Time.x * _PanSpeed)).xy;
			stepDistortion *= _HeatDistortion;
			half4 stepSample = tex2D(_CloudTex, worldUV + stepDistortion);

			float burn = step(IN.worldPos.y - stepSample.r, _BurnHeight);
			float dissolve = step(IN.worldPos.y - stepSample.r, _BurnHeight - _DissolveOffset);
			clip(0.99 - dissolve);

			float2 cloudUV = worldUV + _Time.x * _PanSpeed;
			half4 clouds = tex2D(_CloudTex, cloudUV + flowDistortion);
			float4 fireColor = lerp(_LowColor, _HighColor, clouds);
			fireColor = pow(fireColor, _Contrast) * _Contrast;
			o.Emission = fireColor * burn;

			o.WorldPosition = IN.worldPos;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
