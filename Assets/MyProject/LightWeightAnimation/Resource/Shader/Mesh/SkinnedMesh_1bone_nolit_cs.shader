
Shader "Custom/SkinnedMesh_1bone_nolit_cs"
{
	
	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}

		_Color("Color", Color) = (1, 1, 1, 1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		LOD 200
		
		Pass
		{
			Lighting Off
			LOD 200
			

			Tags
			{
				"Queue"				= "Geometry"
				"IgnoreProjector"	= "True"
				"RenderType"		= "Opaque"
			//	"LightMode"			= "Vertex"
			}

			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			//#pragma multi_compile_fog
			#include "UnityCG.cginc"
			//#include "AutoLight.cginc"


			struct appdata
			{
				float4	vertex	: POSITION;
				float3	normal	: NORMAL;
				float2	uv		: TEXCOORD0;

				fixed4	boneIndex	: COLOR;
				float4	weights		: TEXCOORD1;

				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				fixed4 color	: COLOR;

			//	UNITY_FOG_COORDS(2)
			};


			//struct bone
			//{
			//	float4	position;
			//	float4	rotation;
			//};

			StructuredBuffer<float4> BoneVectorBuffer;
			int	BoneLengthEveryInstance;
			int BoneVectorOffset;

			fixed4		_Color;
			sampler2D	_MainTex;
			
			float4 rot( float4 v, float4 q )
			{
				float3 qv = cross(v.xyz, q.xyz) - v.xyz * q.w;
				float3 rv = v.xyz + 2.0f * cross(qv, q.xyz);

				return float4( rv, 0.0f );
			}
			
			//#ifdef UNITY_INSTANCING_ENABLED
			//	#define getInstanceId	unity_InstanceID
			//#else
			//	#define getInstanceId	0
			//#endif


			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				//UNITY_SETUP_INSTANCE_ID(v);

				//int i = getInstanceId;
				int vector_base = BoneVectorOffset + i * BoneLengthEveryInstance * 2;
				int ivec =  + vector_base + v.boneIndex.x * 2;

				float4 wpos = BoneVectorBuffer[ivec + 0];
				float4 wrot = BoneVectorBuffer[ivec + 1];

				float4	lvt = v.vertex;
				float4	rvt = rot( lvt, wrot );
				float4	tvt = rvt + wpos;
				float4	wvt = mul(UNITY_MATRIX_VP, float4(tvt.xyz, 1.0f));
				//float4	wvt = UnityObjectToClipPos(tvt);

				o.vertex = wvt;
				o.uv = v.uv;
				o.color = float4(1,1,1,1);

				return o;
			}

		
		
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				
				fixed4 col = fixed4(texcol * i.color.xyz, 1.0) * _Color;

			//	UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}

			ENDCG
			
		}
		
	}

}
