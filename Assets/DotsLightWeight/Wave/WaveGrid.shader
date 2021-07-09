
Shader "Custom/WaveGrid"
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
			
			static const float4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			float get_h(int ih, int ibase)
			{
				float4 vh = BoneVectorBuffer[ibase + (ih >> 2)];
				float4 mask = element_mask_table[ih & 3];
				return dot(vh, mask);
			}

			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				int ibase = BoneVectorOffset + i * VectorLengthPerInstance;
				int inext = ibase + VectorLengthPerInstance;
				int ih = asint(v.vertex.y);

				float4 additional = BoneVectorBuffer[inext - 1];

				float whscale = additional.w;
				float3 wpos = additional.xyz;
				float3 lvt = float3(v.vertex.xz * whscale, get_h(ih, ibase)).xzy;

				float4	wvt = UnityObjectToClipPos(wpos + lvt);

				o.vertex = wvt;
				o.uv = lvt.xz * (1.0f/16.0f);//v.uv;
				o.color = fixed4(1,1,1,wvt.z * 0.2f + 0.8f);

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
