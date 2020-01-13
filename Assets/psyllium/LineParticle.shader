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
					half4 index : COLOR;	// wpos, dirp0, dirp-1, dirp+1
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
					float3 up = normalize(pos1 - pos0);
					float3 side = normalize(cross(up, eye));
					return lvt.xxx * side;
				}
				v2f vert( appdata v, uint i : SV_InstanceID )
				{
					v2f o;

					int4 ivec = i.xxxx * BoneLengthEveryInstance.xxxx + v.index;

					float4 wpos = BoneVectorBuffer[ivec.x];

					float4 posbase = BoneVectorBuffer[ivec.y];
					float4 posfwd = BoneVectorBuffer[ivec.z];
					float4 posbak = BoneVectorBuffer[ivec.w];

					float4 lvt = v.vertex;
					float3 eye = wpos.xyz - _WorldSpaceCameraPos;
					float3 xfwd = transform_x(lvt, posbase, posfwd, eye);
					float3 xbak = transform_x(lvt, posbase, posbak, eye);

					float3 wvt = wpos + xfwd*0.5f + xbak*0.5f;

					o.vertex = mul( UNITY_MATRIX_VP, wvt );
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