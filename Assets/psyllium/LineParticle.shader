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
					float4 vertex : POSITION;			// y : posIndex / z : edgeVolume
					float4 uvAndDirIndex : TEXCOORD0;	// x,y : u,v / z,w : dirIndex0, dirIndex1 
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

				
				v2f vert( appdata v, uint i : SV_InstanceID )
				{
					v2f o;

					int nodeIndex = v.vertex.y;
					int inode = i * BoneLengthEveryInstance + nodeIndex;
					int ivec = BoneVectorOffset + inode;

					float4 wpos = BoneVectorBuffer[ivec];


					int dirIndex0 = v.uvAndDirIndex.z;
					int dirIndex1 = v.uvAndDirIndex.w;
					int idir0 = BoneVectorOffset + dirIndex0;
					int idir1 = BoneVectorOffset + dirIndex1;

					float3 wdir = normalize(BoneVectorBuffer[idir1].xyz - BoneVectorBuffer[idir0].xyz);


					float3 eye = wpos.xyz - _WorldSpaceCameraPos;
					float3 up = wdir.xyz;
					float3 side = normalize(cross(up, eye));

					float3 lvt = v.vertex;
					float3 wvt = wpos + lvt.xxx * side;

					o.vertex = mul( UNITY_MATRIX_VP, wvt );
					
					o.uv = v.uvAndDirIndex.xy;

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