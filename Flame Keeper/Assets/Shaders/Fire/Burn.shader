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
			"RenderQueue"="Opaque"
		}
		Cull Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

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


        void surf (Input IN, inout SurfaceOutputStandard o)
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

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
