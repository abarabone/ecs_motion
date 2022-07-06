
Shader "Custom/mesh_nolit_cs_with_palette"
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

			//#pragma target 5.0
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

				//uint4	boneIndex	: BLENDINDICES;
			};
			
			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				fixed4 color	: COLOR;

			//	UNITY_FOG_COORDS(2)
			};


			StructuredBuffer<uint> ColorPalettes;


			//StructuredBuffer<float4> BoneVectorBuffer;
			//int	BoneLengthEveryInstance;
			//int BoneVectorOffset;
			StructuredBuffer<float4> BoneVectorBuffer;
			int	VectorLengthPerInstance;
			int BoneVectorOffset;

			fixed4		_Color;
			sampler2D	_MainTex;
			
			float4 rot( float4 v, float4 q )
			{
				float3 qv = cross(v.xyz, q.xyz) - v.xyz * q.w;
				float3 rv = v.xyz + 2.0f * cross(qv, q.xyz);

				return float4( rv, 0.0f );
			}


			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				int ibone = i;// * 2;//BoneLengthEveryInstance;
				int ivec = BoneVectorOffset + ibone * 2;

				float4 _wpos = BoneVectorBuffer[ivec + 0];
				float4 wrot = BoneVectorBuffer[ivec + 1];

				float ipalette = _wpos.w;
				float4 wpos = float4(_wpos.xyz, 1.0f);

				float4	lvt = v.vertex;
				float4	rvt = rot( lvt, wrot );
				float4	tvt = rvt + wpos;
				//float4	wvt = mul(UNITY_MATRIX_VP, float4(tvt.xyz, 1.0f));
				float4	wvt = UnityObjectToClipPos(tvt.xyz);

				uint c = ColorPalettes[ipalette];
				fixed4 color = float4(c & 0xff, c >> 8 & 0xff, c >> 16 & 0xff, c >> 24 & 0xff) / 255;

				o.vertex = wvt;
				o.uv = v.uv;
				o.color = color;

				return o;
			}

		
		
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				
			fixed4 col = fixed4(texcol * i.color) * _Color;

			//	UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}

			ENDCG
			
		}
		
	}

}
