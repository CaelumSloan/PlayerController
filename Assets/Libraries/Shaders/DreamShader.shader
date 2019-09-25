Shader "Custom/DreamShader"
{
	Properties
	{
		_Color("Color", Color) = (.8980, 0.682353, 1, 1)
		_GradientColor("GradientColor", Color) = (.8980, 0.682353, 1, 1)

		_MainTex ("Texture", 2D) = "white" {}
		_Steps("Steps", Range(1,10)) = 2
		_StepSmoothing("StepSmoothing", Range(0,.1)) = .01
		_BackLightStrength("BackLightStrength", Range(0,1)) = 1
		_BackLightWidth("BackLightWidth", Range(0,2)) = 0.716
		_BackLightSmoothing("BackLightSmoothing", Range(0,.1)) = .01
		_BackLightExtension("BackLightExtension", Range(0,1)) = 0.1

		_HighlightColor("HighlightColor", Color) = (1,1,1,1)
		_HighlightScale("HighlightScale", Range(0,.1)) = .01
		_Noise("NoiseTex", 2D) = "white" {}
		_RandomStrength("RandomStrength", Range(0,2)) = 1
		_NoiseScaling("NoiseScaling", Range(.0001, 2)) = 1
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		//Base
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}

			ZWrite On

			CGPROGRAM
				#pragma target 3.0

				#pragma multi_compile _ SHADOWS_SCREEN
				#pragma multi_compile_fog

				#include "DreamShaderInclude.cginc"
				#pragma vertex vert
				#pragma fragment frag
			ENDCG
		}

		//Additive
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			Blend One One

			ZWrite Off

			CGPROGRAM
				#pragma target 3.0

				#pragma multi_compile_fwdadd_fullshadows
				#pragma multi_compile_fog

				#include "DreamShaderInclude.cginc"
				#pragma vertex vert
				#pragma fragment frag
			ENDCG
		}

		//Meta (Emmision)
		/*
		Pass
		{
			Name "META"
			Tags {"LightMode" = "Meta"}
			Cull Off

			CGPROGRAM
				#include "UnityStandardMeta.cginc"
				#pragma vertex vert_meta
				#pragma fragment frag_meta_custom
				
				#pragma shader_feature _EMISSION

				fixed4 frag_meta_custom(v2f_meta i) : SV_Target
				{
					// Colors                
					fixed4 col = fixed4(1,0,0,1); // The emission color

					// Calculate emission
					UnityMetaInput metaIN;
					UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
					metaIN.Albedo = col.rgb * 5;
					metaIN.Emission = col.rgb * 5;
					return UnityMetaFragment(metaIN);

					//return col * 5;
				}
			ENDCG
		}
		*/

		//Highlight
		Pass
		{

			// draw only back faces
			Cull Front

			CGPROGRAM
				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag

				// Properties (don't forget to add to Properties block!!!)
				uniform float4 _HighlightColor;
				uniform float _HighlightScale;
				uniform float _RandomStrength;
				sampler2D _Noise;
				sampler2D _MainTex;
				uniform float _NoiseScaling;

				// we'll need all of this information later!
				struct vertexInput
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float3 texCoord : TEXCOORD0;
					float4 color : TEXCOORD1;
				};

				// since we're just drawing a flat color,
				// we only need the vertex position in our vertex output.
				struct vertexOutput
				{
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					//float noise : TEXCOORD1;
				};

				vertexOutput vert(vertexInput input)
				{
					vertexOutput output;

					// normal data is provided in float3, but need float4 form to do math
					// with the input vertex, which is a float4
					float4 newPos = input.vertex;
					float4 normal4 = float4(input.normal, 0.0);
					float3 normal = normalize(mul(normal4, unity_WorldToObject).xyz);
					float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

					float4 screenPos = ComputeScreenPos(input.vertex);
					output.worldPos = mul(unity_ObjectToWorld, input.vertex);

					float lightDot = 1 - dot(WorldSpaceViewDir(input.vertex), UnityObjectToWorldNormal(input.normal)); //saturate(dot(normal, lightDir));
					lightDot *= saturate(dot(normal, lightDir));

					float lookup = fmod(length(output.worldPos) * _NoiseScaling, 1);
					float noiseAmount = tex2Dlod(_Noise, float4(lookup, 0, 0, 0));
					noiseAmount = (2 * noiseAmount) - 1;
					noiseAmount *= _RandomStrength;

					// scale the vertex along the normal direction
					newPos += float4(input.normal, 0.0) * lightDot * _HighlightScale + noiseAmount;

					output.pos = UnityObjectToClipPos(newPos); // was newPos

					return output;
				}

				float4 frag(vertexOutput input) : COLOR
				{
					return _HighlightColor;
				}
			ENDCG
		}

		//Shadows
		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
				#pragma target 3.0

				#pragma multi_compile_shadowcaster

				#include "DreamShadows.cginc"
				#pragma vertex shadowVert
				#pragma fragment shadowFrag

			ENDCG
		}
	}
}	
