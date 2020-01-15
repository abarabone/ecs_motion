Shader "Custom/LinePsyllium"
{

	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Blend SrcAlpha One
		Lighting Off ZWrite Off Fog
		{
			Mode Off
		}

		Pass
		{
			CGPROGRAM

				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				//#include "AutoLight.cginc"


				struct appdata
				{
					float4 vertex : POSITION;	// z : edgeVolume
					half2 uv : TEXCOORD0;
					fixed4 index : COLOR;	// world pos, dir origin 0, dir origin 1
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				StructuredBuffer<float4> BoneVectorBuffer;
				int	BoneLengthEveryInstance;
				int BoneVectorOffset;

				sampler2D _MainTex;
				float4 _Color;

				float3 calculate_side(float3 lvt, float3 pos0, float3 pos1, float3 eye)
				{
					float3 up = pos1 - pos0;
					float3 side = cross(up, eye);
					return normalize(side);
				}
				v2f vert(appdata v, uint i : SV_InstanceID)
				{
					v2f o;

					int offset = BoneVectorOffset + i * BoneLengthEveryInstance;
					int3 ivec = offset.xxx + v.index.xyz;

					float3 wpos = BoneVectorBuffer[ivec.x].xyz;

					float3 lvt = v.vertex.xyz;
					float3 eye = wpos - _WorldSpaceCameraPos.xyz;

					float3 ori0 = BoneVectorBuffer[ivec.y + 0].xyz;
					float3 fwd0 = BoneVectorBuffer[ivec.y + 1].xyz;
					float3 ori1 = BoneVectorBuffer[ivec.z + 0].xyz;
					float3 fwd1 = BoneVectorBuffer[ivec.z + 1].xyz;

					float3 side0 = calculate_side(lvt, ori0, fwd0, eye);
					float3 side1 = calculate_side(lvt, ori1, fwd1, eye);

					float3 side = (side0 + side1) * 0.5f;
					float3 edgex = lvt.xxx * side;
					float3 edgez = lvt.zzz * normalize(cross(eye, side0));//
					float3 wvt = wpos + edgex + edgez;

					o.vertex = mul(UNITY_MATRIX_VP, float4(wvt, 1.0f));
					o.uv = v.uv;
					UNITY_TRANSFER_FOG(o, o.vertex);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					col.rgb *= _Color;
					col.rgb *= 6.;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}

			ENDCG
		}
			
	}
	
}