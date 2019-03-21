// Global variables
float3 _PlayerLightPosition;
float _PlayerMaxLightRange;
float _PlayerCurrentLightRange;
float _AttenutaionIntensity;
float _NormalLightIntensity;
sampler2D _PlayerRampTex;

// Custom surface
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

// Lighting function for the rings
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

// Glocal illumination function
inline void LightingRampByDistance_GI(
	SurfaceOutputCustom s,
	UnityGIInput data,
	inout UnityGI gi)
{
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Smoothness, s.Normal);
}
