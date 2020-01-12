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
				// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
			//	#pragma exclude_renderers d3d11 xbox360 gles
			//	#pragma only_renderers d3d9 opengl gles
			//	#pragma glsl
				#pragma vertex vert
				#pragma fragment frag
			//	#pragma target 3.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				
				struct vtxpos
				{
					half3	position : POSITION;
					half2	texcoord : TEXCOORD0;
					half4	color : COLOR;
				};
				
				struct v2f
				{
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					fixed4	col : COLOR;
					UNITY_FOG_COORDS(2)
				};
				


				half4	d[8];
				
				half4	p[240];
				
				
				half3 rot_( half3 tpos, int4 i )
				{
					half3x3	mtRot;

					half3	dir = normalize( p[i.x].xyz - p[i.y].xyz );

					half3	eye = normalize( _WorldSpaceCameraPos - p[i.w].xyz );

					half3	h = cross( dir, eye );

					half3	v = cross( h, eye );

					mtRot[0] = eye;
					mtRot[1] = v;
					mtRot[2] = h;	// ラインの水平を正面（ｚ）に向かせたい…のか？ｗ

					return mul( tpos, mtRot );
				}

				half3 rot( half3 tpos, int4 i )
				{
					half3	dir = normalize( p[i.x].xyz - p[i.y].xyz );

					half3	eye = normalize( _WorldSpaceCameraPos - p[i.w].xyz );

					half3	h = cross( dir, eye );

					return tpos.zzz * h;
				}



				v2f vert( vtxpos v )
				{
					
					v2f o;
					
					
					//half3 i = v.color.rga * 255;
					int4	i = D3DCOLORtoUBYTE4( v.color.bgra );

					half4	pi = p[i.w];
					
					half4	pallet = d[pi.w];
					
					
					half3	tpos = v.position * pallet.w;
					
					half3	rpos = rot( tpos, i );
					
					half4	pos = half4( pi.xyz + rpos, 1.0f );
					
					o.pos = mul( UNITY_MATRIX_VP, pos );
					
					
					o.uv = v.texcoord;

					o.col = fixed4( pallet.xyz, 1.0f );

					UNITY_TRANSFER_FOG( o, o.pos );

					return o;
					
				}

				
				sampler2D _MainTex;

				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 texcol = tex2D( _MainTex, i.uv );

					fixed4 col = fixed4( texcol *i.col );

					clip( col.a - 0.1f );

					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
				
			ENDCG
			
		}
		
	}
	
}