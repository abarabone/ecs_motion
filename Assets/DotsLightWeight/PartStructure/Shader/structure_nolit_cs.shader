
Shader "Custom/structure_nolit_cs"
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
				uint4	part_index : COLOR;
			};
			
			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				fixed4 color	: COLOR;

			//	UNITY_FOG_COORDS(2)
			};


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
			
			

			static const uint4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			
			uint get_part_bit(int i_instance, uint2 part_id)
			{
				const int ioffset = part_id.y >> 2;
				const int ielement = part_id.y & 0x3;
				const uint bitmask = 1 << part_id.x;
				
				const uint4 xyzw = asuint(BoneVectorBuffer[i_instance + ioffset]);
				const uint element = dot(xyzw, element_mask_table[ielement]);
				return element & bitmask;
			}
			
			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;
				
				const int vector_offset_per_instance = 4;
				const int vector_length_in_bone = 2;
				const int total_vector_length = vector_length_in_bone * 1 + vector_offset_per_instance;
				const int i_vector_base = BoneVectorOffset + i * total_vector_length;
				const int ivec = i_vector_base + vector_offset_per_instance;

				const float4 wpos = BoneVectorBuffer[ivec + 0];
				const float4 wrot = BoneVectorBuffer[ivec + 1];

				const float4	lvt = v.vertex;
				const float4	rvt = rot( lvt, wrot );
				const float4	tvt = rvt + wpos;
				//const float4	wvt = mul(UNITY_MATRIX_VP, float4(tvt.xyz, 1.0f));
				const float4	wvt = UnityObjectToClipPos(tvt.xyz);

				//const float alive = get_part_bit(i_vector_base, v.part_index.rg) == 0 ? 1 : 0;
				const float alive = 1 - any(get_part_bit(i_vector_base, v.part_index.rg));

				o.vertex = wvt * alive;
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
