
Shader "Custom/HeightsGrid"
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
			// [0] x: flat height index offset as int, y: lv as int
			// [1] x,y,z: pos, w: scale * lv
			int	VectorLengthPerInstance;
			int BoneVectorOffset;

			sampler2D	_MainTex;
			
			StructuredBuffer<float> Heights;
			float4 DimInfo;// x,y: lengthInGrid, z: widthSpan

			static const float4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			float get_h(int ih, int offset)
			{
				int2 lengthInGrid = asint(DimInfo.xy);
				int widthSpan = asint(DimInfo.z);
				int2 i = int2(ih & (lengthInGrid.x-1), ih >> countbits(lengthInGrid.x-1));

				float h = Heights[offset + i.y * widthSpan + i.x];
				return h;
			}

			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				int ibase = BoneVectorOffset + i * VectorLengthPerInstance;
				int ih = asint(v.vertex.y);

				float4 tf = BoneVectorBuffer[ibase + 1];

				int lv = asint(tf.w);
				float4 info = BoneVectorBuffer[ibase + 0];
				int offset = asint(info.x);

				float whscale = tf.w;
				float3 lvt = float3(v.vertex.xz * whscale, get_h(ih, offset)).xzy;

				float3 wpos = tf.xyz;
				float4	wvt = UnityObjectToClipPos(wpos + lvt);

				o.vertex = wvt;
				o.uv = lvt.xz * (1.0f/16.0f);//v.uv;
				o.color = fixed4(1,1,1,1);//wvt.z * 0.2f + 0.8f);

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
