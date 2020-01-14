Shader "Custom/LineParticle"
{
	
	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1)
	}
	
	
	SubShader
	{
		//Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Tags{ "Queue" = "Opequre" "IgnoreProjector" = "True" "RenderType" = "Opequre" }
		//LOD 200
		
		Pass
		{
			
			Lighting Off
			LOD 200
			
			Tags
			{
				"Queue" = "AlphaTest"
				//"Queue"="Geometry"
				"RenderType" = "TransparentCutout"
				//"RenderType"="Opaque"
				"IgnoreProjector" = "True"
				"LightMode" = "Vertex"
			}

			AlphaTest Greater 0.5
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
					fixed4 index : COLOR;	// wpos, dirp0, dirp-1, dirp+1
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

				float3 transform_x(float3 lvt, float3 pos0, float3 pos1, float3 eye)
				{
					float3 up = pos1 - pos0;
					float3 side = cross(up, eye);
					return lvt.xxx * normalize(side);
					//float3 up = pos1 - pos0;
					//float3 side = cross(up, eye);
					//float3 forward = cross(eye, side);
					//float3x3 mt = float3x3(normalize(up), normalize(side), normalize(forward));
					//return mul(lvt, mt);
				}
				v2f vert( appdata v, uint i : SV_InstanceID )
				{
					v2f o;

					int offset = BoneVectorOffset + i * BoneLengthEveryInstance;
					int4 ivec = offset.xxxx + v.index;

					float3 wpos = BoneVectorBuffer[ivec.x].xyz;

					float3 posbase = BoneVectorBuffer[ivec.y].xyz;
					float3 posfwd = BoneVectorBuffer[ivec.z].xyz;
					float3 posbak = BoneVectorBuffer[ivec.w].xyz;
					float3 lvt = v.vertex.xyz;

					float3 eye = wpos - _WorldSpaceCameraPos.xyz;
					float3 xfwd = transform_x(lvt, posbase, posfwd, eye);
					float3 xbak = transform_x(lvt, posbase, posbak, eye);

					float3 wvt = wpos + xfwd;// (wpos + xfwd) * 0.5f + (wpos + xbak) * 0.5f;
					//float3 wvt = wpos + xvt;//lerp(xfwd, xvt, step(xfwd + xbak, 0.5f));

					o.vertex = mul( UNITY_MATRIX_VP, float4(wvt, 1.0f) );
					o.uv = v.uv;
					UNITY_TRANSFER_FOG( o, o.vertex );

					return o;
				}


				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 texcol = tex2D( _MainTex, i.uv );
					fixed4 col = fixed4( texcol * _Color );
					//clip( col.a - 0.1f );

					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
				
			ENDCG
			
		}
		
	}
	
}