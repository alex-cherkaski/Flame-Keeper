// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Fluids/Water"
{
	// These are exposed in the unity inspector
	Properties
	{
		[MaterialToggle] _OrthographicCamera("Is Camera Orthographic?", Float) = 0

		_WaterTint("Water Tint", Color) = (0,0,1,1)

		_WaveHeight("Wave Height", float) = 0.2
		_WaveSpeed("Wave Speed", float) = 1.0
		_WaveFrequency("Wave Frequency", float) = 1.0

		_RefractDistortion("Refraction Distortion", float) = 1.0
		_ReflectDistortion("Reflection Distortion", float) = 0.1
		_RefractIntensity("Refraction Intensity", float) = 1.0
		_ReflectIntensity("Reflection Intensity", float) = 0.5

		_FresnelNoseScale("Fresnel Noise", float) = 4.0

		_FoamDistance("Foam Distance", float) = 1.0
		_FoamColor("Foam Color", Color) = (1,1,1,1)
		_FoamNoiseScale("Foam Noisiness", float) = 1

		_NoiseTex("Noise Map", 2D) = "white" {}

		_AmbientLightFactor("Ambient Light Factor", Range(0, 1)) = 0.2

		[HideInInspector]
		_ReflectionTex("Reflection Texture", 2D) = "clear" {}
	}

	SubShader
	{

		// Make sure we render this along with other transparent objects.
		// Our unity project is set up for "forward rendering", so we are guaranteed
		// that all opaque objects are fully rendered and shaded before we render anything
		// marked as transparent. 
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"LightMode" = "ForwardBase"
		}

		LOD 100
		Blend One Zero
		Cull Off

		// Special unity shader pass, this grabs what is currently rendered for the frame
		// and stores it in a texture called "_GrabTexture"
		GrabPass
		{
			"_GrabTexture"
		}

		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" 

			// Tells unity which functions we want to use for the vertex / fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			// I dunno what this does, guess unity needs for fog or whatever
			#pragma multi_compile_fog

			// Have to define all our properties as variables for each pass
			float _OrthographicCamera;

			// Globals
			float4 _PlayerLightPosition;
			float _PlayerCurrentLightRange;
			sampler2D _PlayerRampTex;
			float _GlobalTime;

			sampler2D _GrabTexture;
			sampler2D _CameraDepthTexture; // Pre-set by unity
			half4 _WaterTint;
			float _WaveHeight;
			float _WaveSpeed;
			float _WaveFrequency;
			float _RefractDistortion;
			float _ReflectDistortion;
			float _RefractIntensity;
			float _ReflectIntensity;
			float _FresnelNoseScale;
			float _FoamDistance;
			half4 _FoamColor;
			float _FoamNoiseScale;
			sampler2D _NoiseTex;
			sampler2D _ReflectionTex;
			float _AmbientLightFactor;

			float4 _RippleCameraPosition;
			float _RippleCamSize;
			sampler2D _RippleTex;

			// Data we tell unity to give us for the vertex shader
            struct vertInput
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

			// Data we create and pass into the fragment shader
            struct fragInput
            {
                UNITY_FOG_COORDS(0)
				float4 screenPos : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float3 normal : TEXCOORD3;
				float4 uvGrab : TEXCOORD4;

                float4 vertex : SV_POSITION;
            };

			// Vertex shader definition
            fragInput vert (vertInput v)
            {
                fragInput o;
				UNITY_INITIALIZE_OUTPUT(fragInput, o); // Initializes everything to 0

				o.vertex = UnityObjectToClipPos(v.vertex); // Transform from object space to camera space
				o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Transform from object space to world space
				o.screenPos = ComputeScreenPos(o.vertex); // Compute position of vertex on the screen
				COMPUTE_EYEDEPTH(o.screenPos.w); // I'm storing z-depth of vertex in the last component of the screen position

				// Raises each vertex in a wavy pattern
				float3 up = mul(unity_WorldToObject, float3(0,1,0));
				v.vertex.xyz += up * (sin((_GlobalTime * _WaveSpeed) + (o.worldPos.x + o.worldPos.z) * _WaveFrequency) * _WaveHeight);

                UNITY_TRANSFER_FOG(o, o.vertex); // Sets the fog coordinates that unity uses

				// Compatibilaity for OpenGL vs DirectX
				// DirectX: top of the texture has UV.y = 0
				// OpenGL: top of the texture has UV.y = 1
				#if UNITY_UV_STARTS_AT_TOP
					float flip = -1.0;
				#else
					float flip = 1.0;
				#endif

				// Compute UV coords of grab texture based on position of our shader object 
				o.uvGrab.xy = (float2(o.vertex.x, o.vertex.y*flip) + o.vertex.w) * 0.5;
				o.uvGrab.zw = o.vertex.zw;

				o.normal = mul(unity_ObjectToWorld, v.normal);

                return o;
            }

			// Fragment shader definition
            fixed4 frag (fragInput i) : SV_Target
            {
				fixed4 col = _WaterTint;

				// Get a random value from noise map
				fixed noise = tex2D(_NoiseTex, i.worldPos.xz * 0.1 + _Time.y * _WaveSpeed * 0.05).r;
				fixed fnoise = noise * _FresnelNoseScale; // Noise for fresnel dependent effects (reflection / refraction)
				fixed snoise = noise * _FoamNoiseScale; // Noise for shore effect

				// Handle all ripples
				float ripplesTex = 0;
				float2 uv = i.worldPos.xz - _RippleCameraPosition.xz;
				uv = uv / (_RippleCamSize * 2);
				uv += 0.5;
				ripplesTex += tex2D(_RippleTex, uv).b; // Ripple texture is always orthographic

				float ripples = step(0.99, ripplesTex * 2);

				// Distort our uv coord a bit so we get a refraction and reflection effect
				half2 bumpRefract = normalize(half2(sin(_Time.y * _WaveSpeed * 0.9 + (ripplesTex * 10.0) + fnoise), cos(_Time.y * _WaveSpeed + (ripplesTex * 10.0) + fnoise))) * _RefractDistortion;
				half2 bumpReflect = normalize(half2(cos(_Time.y * _WaveSpeed * 0.9 + (ripplesTex * 10.0) + fnoise), sin(_Time.y * _WaveSpeed + (ripplesTex * 10.0) + fnoise))) * _ReflectDistortion;
				i.uvGrab.xy += bumpRefract * i.vertex.z;

				// Sample our refraction texture (whats already rendered to the screen)
				// Sample out reflection texture (whatever our reflection camera is giving us)
				fixed4 refract = fixed4(0, 0, 0, 0);
				fixed4 reflection = fixed4(0, 0, 0, 0);
				if (_OrthographicCamera)
				{
					refract = tex2D(_GrabTexture, UNITY_PROJ_COORD(i.uvGrab));
					reflection = tex2D(_ReflectionTex, UNITY_PROJ_COORD(i.screenPos + half4(bumpReflect, 0, 0)));
				}
				else
				{
					refract = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvGrab));
					reflection = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.screenPos + half4(bumpReflect, 0, 0)));
				}
				col += refract * _RefractIntensity;
				col += reflection * _ReflectIntensity;

				float foamStep = 1.0f; // FoamStep is either 0 (close to shore) or 1 (not close to shore) based on some noisy threshold
				if (_OrthographicCamera)
				{
					float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos).r;
					float diff = (1 - i.screenPos.w / 1000.0) - depth; // The "1000" here is the far plane of the othographic camera, will want to set from code if we change it a bunch

					foamStep = step(_FoamDistance, diff - (snoise * _FoamDistance));
				}
				else
				{
					float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
					float diff = depth - i.screenPos.w; // Difference in depth from the plane verus the object behind it

					foamStep = step(_FoamDistance, diff - snoise);
				}


				UNITY_APPLY_FOG(i.fogCoord, col); // Unity applies the fog for us

				// Lighting
				half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz)); // Directional light source

				//float kA = min(_AmbientLightFactor, saturate(_AmbientLightFactor - step(1.0 - (length(_PlayerLightPosition - i.worldPos) / _PlayerCurrentLightRange), 0.0)));
				float kA = _AmbientLightFactor;

				float distance = length(float3(_PlayerLightPosition.xyz - i.worldPos));
				float falloff = 1.0 - (distance / 8.0);
				float cutoff = saturate(1.0 - floor(distance / _PlayerCurrentLightRange));
				float rampedFalloff = tex2D(_PlayerRampTex, half2(falloff, falloff)).r * 1.5;

				kA += kA * rampedFalloff * cutoff;


				fixed4 ambient = kA * (col);
				fixed4 diffuse = (1 - kA) * (col) * _LightColor0 * nl;
				return saturate(lerp(_FoamColor * pow(kA, 0.3), ambient + diffuse, saturate(foamStep - ripples))); // lerp in foam
            }

            ENDCG
        }
    }
}
