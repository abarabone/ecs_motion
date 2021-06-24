Shader "Custom/LineParticle uv blend"
{
	
	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}
		//_Color("Main Color", Color) = (1,1,1)
	}
	
	
	SubShader
	{
		//Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Tags{ "Queue" = "Opequre" "IgnoreProjector" = "True" "RenderType" = "Opequre" }
		//LOD 200
		
		Pass
		{
			
			Tags
			{
				"Queue" = "AlphaTest"
				//"Queue"="Geometry"
				"RenderType" = "TransparentCutout"
				//"RenderType"="Opaque"
				"IgnoreProjector" = "True"
				"LightMode" = "Vertex"
			}

			Lighting Off
			LOD 200
			
			Cull Back
			ZWrite On
			ZTest LEqual
			ColorMask RGBA


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
					fixed4 color : VERTEX_COLOR;
				};

				StructuredBuffer<float4> BoneVectorBuffer;
				int	VectorLengthPerInstance;
				int BoneVectorOffset;
				float4 UvParam;

				sampler2D _MainTex;
				//float4 _Color;

				float3 calculate_side(float3 lvt, float3 pos0, float3 pos1, float3 eye)
				{
					float3 up = pos1 - pos0;
					float3 side = cross(up, eye);
					return normalize(side);
				}
				v2f vert( appdata v, uint i : SV_InstanceID )
				{
					v2f o;

					int offset = BoneVectorOffset + i * VectorLengthPerInstance;
					int firstPoint = offset + 1;
					int3 ivec = firstPoint.xxx + v.index.xyz;
					
					float4 cur = BoneVectorBuffer[ivec.x];
					float3 wpos = cur.xyz;
					
					float size = BoneVectorBuffer[offset].x;
					float3 lvt = v.vertex.xyz * size;
					float3 eye = wpos - _WorldSpaceCameraPos.xyz;

					float3 ori0 = BoneVectorBuffer[ivec.y + 0].xyz;
					float3 fwd0 = BoneVectorBuffer[ivec.y + 1].xyz;
					float3 ori1 = BoneVectorBuffer[ivec.z + 0].xyz;
					float3 fwd1 = BoneVectorBuffer[ivec.z + 1].xyz;

					float3 eye0 = ori0 - _WorldSpaceCameraPos.xyz;
					float3 eye1 = fwd1 - _WorldSpaceCameraPos.xyz;

					float3 side0 = calculate_side(lvt, ori0, fwd0, eye0);
					float3 side1 = calculate_side(lvt, ori1, fwd1, eye1);

					float3 side = (side0 + side1) * 0.5f;
					float3 edgex = lvt.xxx * side;
					float3 edgez = lvt.zzz * normalize(cross(eye, side));
					float3 wvt = wpos + edgex + edgez;

					o.vertex = mul( UNITY_MATRIX_VP, float4(wvt, 1.0f) );
					
					const half2 uvspan = UvParam.xy;
					const half2 uvtick = uvspan * 0.01f;// / 100;
					const uint4 uvp = asuint(cur.yyyy) >> uint4(0, 8, 16, 24) & 255;
					const half2 uvofs = uvp.xy * uvspan + uvtick;
					const half2 uvsize = uvp.zw * uvspan - uvtick - uvtick;
					o.uv = uvofs + v.uv * uvsize;
					
					const fixed4 color = float4(asuint(cur.wwww) >> uint4(0, 8, 16, 24) & 255) * (1. / 255.);
					o.color = color;

					UNITY_TRANSFER_FOG( o, o.vertex );
					return o;
				}


				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 texcol = tex2D( _MainTex, i.uv );
					fixed4 col = fixed4( texcol * i.color );
					clip( col.a - 0.5f );// アルファテスト

					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;

					//fixed4 col = tex2D(_MainTex, i.uv);
					//col.rgba *= i.color;
					//UNITY_APPLY_FOG(i.fogCoord, col);
					//return col;
				}
				
				
			ENDCG
			
		}
		
	}
	
}