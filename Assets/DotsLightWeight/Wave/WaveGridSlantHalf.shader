
Shader "Custom/WaveGridSlantHalf"
{
	
	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}
	}
	
	
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		
        Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		//ZWrite Off
		Fog
		{
			Mode Off
		}
		Cull Off

		LOD 200
		
		Pass
		{
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
			};
			
			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				fixed4 color	: COLOR;

				UNITY_FOG_COORDS(2)
			};

			
			StructuredBuffer<float4> BoneVectorBuffer;
			int	VectorLengthPerInstance;
			int BoneVectorOffset;

			sampler2D	_MainTex;
			
			static const uint4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			float unmask(float4 vh, int imask)
			{
				const float4 mask = element_mask_table[imask];
				return dot(vh, mask);
			}
			int4 unpack4(int p)
			{
				return int4((p >> 0) & 0xff, (p >> 8) & 0xff, (p >> 16) & 0xff, (p >> 24) & 0xff);
			}
			float4 get_h4(int4 ih, int ibase)
			{
				const int4 i = ibase + ih >> 2;
				const float4 vhul = BoneVectorBuffer[i.x];
				const float4 vhur = BoneVectorBuffer[i.y];
				const float4 vhdl = BoneVectorBuffer[i.z];
				const float4 vhdr = BoneVectorBuffer[i.w];

				const int4 imask = i & 3;
				return float4(unmask(vhul, imask.x), unmask(vhur, imask.y), unmask(vhdl, imask.z), unmask(vhdr, imask.w));
			}

			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				const int ibase = BoneVectorOffset + i * VectorLengthPerInstance;
				const int inext = ibase + VectorLengthPerInstance;

				const int4 ih4 = unpack4(asint(v.vertex.y));
				const float4 h4 = get_h4(ih4, ibase);
				const float h = (h4.x + h4.y + h4.z + h4.w) * 0.25f;
				
				const float whscale = BoneVectorBuffer[inext - 2].w;

				const float3 wpos = BoneVectorBuffer[inext - 1].xyz;
				const float3 lvt = float3(v.vertex.xz * whscale, h4.x).xzy;

				const float4 wvt = UnityObjectToClipPos(wpos + lvt);

				o.vertex = wvt;
				o.uv = lvt.xz * (1.0f/16.0f);//v.uv;
				o.color = float4(1,1,1,1);

				return o;
			}

		
		
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				
				fixed4 col = texcol * i.color;

				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}

			ENDCG
			
		}
		
	}

}
