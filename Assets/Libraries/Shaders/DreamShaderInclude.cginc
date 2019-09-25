#if !defined(MY_LIGHTING_INCLUDED)
	#define DREAM_SHADER_INCLUDED


	//#include "UnityStandardBRDF.cginc"
	#include "UnityPBSLighting.cginc"
	#include "AutoLight.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
		float3 worldNormal : TEXCOORD1;
		float3 viewDir: TEXCOORD2;
		float4 screenPos : TEXCOORD3;
		float3 worldPos : TEXCOORD4;

		SHADOW_COORDS(5)
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _Color;
	float4 _GradientColor;
	int _Steps;
	float _StepSmoothing;

	float _BackLightStrength;

	float _BackLightWidth;
	float _BackLightSmoothing;
	float _BackLightExtension;

	float4 _HighlightColor;

	v2f vert(appdata v)
	{
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.viewDir = WorldSpaceViewDir(v.vertex);
		o.screenPos = ComputeScreenPos(o.pos);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);

		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.normal = v.normal;
		o.worldNormal = UnityObjectToWorldNormal(v.normal);
		TRANSFER_SHADOW(o);

		return o;
	}

	float LightAmount(float baseAmount)
	{
		//Normalize between 0 and 1. 
		//float lightAmount = (baseAmount + 1.00) / 2.00;

		float lightAmount = baseAmount;

		if (lightAmount < 0)
			return 0;

		float final = 0;

		float stepIndex = floor((_Steps + 1.00) * lightAmount);
		float stepLength = (1.00 / (_Steps + 1.00));
		float stepStartAmount = stepIndex * stepLength;
		float nextStepStartAmount = (stepIndex + 1.00) * stepLength;
		float middleOfStepAmount = ((nextStepStartAmount - stepStartAmount) / 2.00) + stepStartAmount;

		if (lightAmount <= middleOfStepAmount)
		{
			if (stepIndex == 0)
			{
				final = 0;
			}
			else
			{
				float initialOutputOffset = (stepIndex - 1) / _Steps;
				final = initialOutputOffset + (smoothstep(stepStartAmount - _StepSmoothing, stepStartAmount + _StepSmoothing, lightAmount) / _Steps);
			}
		}
		else
		{
			if (stepIndex == _Steps)
			{
				final = 1;
			}
			else
			{
				float initialOutputOffset = stepIndex / _Steps;
				final = initialOutputOffset + (smoothstep(nextStepStartAmount - _StepSmoothing, nextStepStartAmount + _StepSmoothing, lightAmount) / _Steps);
			}
		}

		return final;
	}

	float3 ApplyFog(float3 color, v2f i) 
	{
		float viewDistance = length(_WorldSpaceCameraPos - i.worldPos);
		UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
		return lerp(unity_FogColor, color, saturate(unityFogFactor)).rgb;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		i.normal = normalize(i.normal);
		i.worldNormal = normalize(i.worldNormal);
		i.viewDir = normalize(i.viewDir);

		#if defined(POINT) || defined(SPOT)
			float3 lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
		#else
			float3 lightDir = _WorldSpaceLightPos0.xyz;
		#endif

		float lightNormalDot = dot(lightDir.xyz, i.worldNormal);

		#if defined(POINT) || defined(SPOT)
			UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
			lightNormalDot = attenuation * 4* ((lightNormalDot + 1.00) / 2.00);
			lightNormalDot = (2.00 * lightNormalDot) - 1.00;
		#endif

		float lightAmount = LightAmount(lightNormalDot);

		#if(SHADOWS_SCREEN)
			//float shadowAmount = 1;													//No shadows
			//float shadowAmount = SHADOW_ATTENUATION(i);
			//float shadowAmount = UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos); // Doesn't work.
			//float shadowAmount = tex2D(_ShadowMapTexture, i.screenPos / i.vertex.w);

			float shadowAmount = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).r;
		#else
			float shadowAmount = 1;
		#endif

		lightAmount = lightAmount * shadowAmount;

		float backLightNormalDot = 1 - dot(i.viewDir, i.worldNormal);
		float backLightAmount = backLightNormalDot * pow(lightNormalDot, 1 - _BackLightExtension);
		backLightAmount = smoothstep(1 - _BackLightWidth - _BackLightSmoothing, 1 - _BackLightWidth + _BackLightSmoothing, backLightAmount);
		backLightAmount = backLightAmount * _BackLightStrength;

		float3 baseCol = lerp(_Color, _GradientColor, i.screenPos.y / i.screenPos.w);
		float4 texCol = tex2D(_MainTex, i.uv);

		float3 lightCol = lightAmount * _LightColor0.rgb;

		float3 ambientLightCol = ShadeSH9(half4(i.normal, 1));
		float3 backLightCol = backLightAmount.rrr;

		#if defined(POINT) || defined(SPOT)
			float3 combinedLightCol = ambientLightCol + lightCol;
		#else	
			float3 combinedLightCol = ambientLightCol + lightCol + backLightCol;
		#endif

		float3 baseMergeTexCol = lerp(baseCol.rgb, texCol.rgb, texCol.a);

		float3 finalColor = baseMergeTexCol * combinedLightCol;
		//#if defined(FOG_LINEAR)
		//	finalColor = ApplyFog(finalColor, i).rgb;
		//#endif

		return float4(finalColor, 1);
		//return finalColor * float4(i.normal,1) + .5; // Show Normals
	}
#endif