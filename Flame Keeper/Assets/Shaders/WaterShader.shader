Shader "Fluids/Water"
{
	// These are exposed in the unity inspector
	Properties
	{
		_WaterTint("Water Tint", Color) = (0,0,1,1)

		_WaveHeight("Wave Height", float) = 0.2
		_WaveSpeed("Wave Speed", float) = 1.0
		_WaveFrequency("Wave Frequency", float) = 1.0

		_RefractDistortion("Refraction Distortion", float) = 1.0
		_ReflectDistortion("Reflection Distortion", float) = 0.1
		_ReflectIntensity("Reflection Intensity", float) = 0.5

		_FoamDistance("Foam Distance", float) = 1.0
		_FoamColor("Foam Color", Color) = (1,1,1,1)
		_FoamNoiseScale("Foam Noisiness", float) = 1

		_NoiseTex("Noise Map", 2D) = "white" {}

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

			// Tells unity which functions we want to use for the vertex / fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			// I dunno what this does, guess unity needs for fog or whatever
			#pragma multi_compile_fog

			// Have to define all our properties as variables for each pass
			sampler2D _GrabTexture;
			sampler2D _CameraDepthTexture; // Pre-set by unity
			half4 _WaterTint;
			float _WaveHeight;
			float _WaveSpeed;
			float _WaveFrequency;
			float _RefractDistortion;
			float _ReflectDistortion;
			float _ReflectIntensity;
			float _FoamDistance;
			half4 _FoamColor;
			float _FoamNoiseScale;
			sampler2D _NoiseTex;
			sampler2D _ReflectionTex;

			float4 _RippleCameraPosition;
			float _RippleCamSize;
			sampler2D _RippleTex;

			// Data we tell unity to give us for the vertex shader
            struct vertInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			// Data we create and pass into the fragment shader
            struct fragInput
            {
                UNITY_FOG_COORDS(0)
				float4 screenPos : TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float4 uvGrab : TEXCOORD3;

                float4 vertex : SV_POSITION;
            };

			// Vertex shader definition
            fragInput vert (vertInput v)
            {
                fragInput o;
				UNITY_INITIALIZE_OUTPUT(fragInput, o); // Initializes everything to 0


				// Raises each vertex in a wavy pattern
				v.vertex.y += sin((_Time.y * _WaveSpeed) + (v.vertex.x + v.vertex.z) * _WaveFrequency) * _WaveHeight;

				o.vertex = UnityObjectToClipPos(v.vertex); // Transform from object space to camera space
				o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Transform from object space to world space
				o.screenPos = ComputeScreenPos(o.vertex); // Compute position of vertex on the screen
				COMPUTE_EYEDEPTH(o.screenPos.w); // I'm storing z-depth of vertex in the last component of the screen position

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

                return o;
            }

			// Fragment shader definition
            fixed4 frag (fragInput i) : SV_Target
            {
				fixed4 col = _WaterTint;

				// Handle all ripples
				float ripples = 0;

				float2 uv = i.worldPos.xz - _RippleCameraPosition.xz;
				uv = uv / (_RippleCamSize * 2);
				uv += 0.5;
				ripples += tex2D(_RippleTex, uv).b;

				ripples = step(0.99, ripples * 3);
				float4 ripplesColored = ripples * _FoamColor;


				// Distort our uv coord a bit so we get a refraction and reflection effect
				half2 bumpRefract = normalize(half2(sin(i.uvGrab.x + _Time.y * _WaveSpeed * 0.9 + ripples), cos(i.uvGrab.y + _Time.y * _WaveSpeed + ripples)));
				half2 bumpReflect = normalize(half2(cos(i.screenPos.x + _Time.y * _WaveSpeed * 0.9 + ripples), sin(i.screenPos.y + _Time.y * _WaveSpeed + ripples))) * _ReflectDistortion;
				i.uvGrab.xy = i.uvGrab.xy + bumpRefract * i.vertex.z * _RefractDistortion;

				fixed4 refract = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvGrab)); // Faked refracted view behind plane
				fixed4 reflection = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.screenPos + half4(bumpReflect,0,0))); // Sample our reflection texture that was passed in

				fixed noise = tex2D(_NoiseTex, i.worldPos.xz * 0.1 + _Time.y * _WaveSpeed * 0.05).r; // Get a random value from noise map
				float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
				float diff = depth - i.screenPos.w; // Difference in depth from the plane verus the object behind it

				
				col = lerp(col, col+refract, saturate(diff * 0.8)); // Blend in refraction when difference in depth is high
				col += reflection * _ReflectIntensity; // Reflection is additive

				// foamStep is either 0 (close to shore) or 1 (not close to shore) based on some noisy threshold
				float foamStep = step(_FoamDistance, diff - noise * _FoamNoiseScale); 
				col = lerp(_FoamColor, col, foamStep); // Set foam color

                UNITY_APPLY_FOG(i.fogCoord, col); // Unity applies the fog for us

                return saturate(col + ripplesColored);
            }

            ENDCG
        }
    }
}
