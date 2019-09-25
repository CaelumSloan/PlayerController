#if !defined(DREAM_SHADOWS_INCLUDED)
	#define DREAM_SHADOWS_INCLUDED

	#include "UnityCG.cginc"

	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	#if defined(SHADOWS_CUBE)

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float3 lightVec : TEXCOORD0;
		};

		v2f shadowVert(appdata v) 
		{
			v2f o;

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.lightVec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz;

			return o;
		}

		float4 shadowFrag(v2f i) : SV_TARGET
		{
			float depth = length(i.lightVec) + unity_LightShadowBias.x;
			depth = depth * _LightPositionRange.w;
			return UnityEncodeCubeShadowDepth(depth);
		}

	#else

		float4 shadowVert(appdata v) : SV_POSITION
		{
			float4 position = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);

			return UnityApplyLinearShadowBias(position);
		}

		half4 shadowFrag() : SV_Target
		{
			return 0;
		}

	#endif

#endif