// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Custom/SkinnedMesh_1bone"
{
	
	Properties
	{
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
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
				"Queue"="Geometry"
				"IgnoreProjector"="False"
				"RenderType"="Opaque"
				"LightMode"="Vertex"
			}
			
			Cull Back
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			Fog {}
			
			CGPROGRAM
			//	#pragma only_renderers d3d9 opengl gles
			//	#pragma glsl
				#pragma vertex vert3
				#pragma fragment frag
			//	#pragma target 3.0
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"


				struct vtxpos
				{
					half4	vertex : POSITION;
					half3	normal : NORMAL;
					half2	texcoord : TEXCOORD0;
					half4	color : COLOR;
				};
				
				struct v2f
				{
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					fixed3	color : COLOR;
					UNITY_FOG_COORDS(2)
				};
								
				
				half3 rot( half3 v, half4 q )
				{
					half3	qv = cross( v, q.xyz ) - v * q.w;
					return v + 2.0f * cross( qv, q.xyz );
				}
				
				v2f vert2(
					vtxpos p,
					uniform fixed4	r[125],
					uniform fixed4	t[125]
				)
				{
					v2f o;
					
					half i = p.color.a * 255;
						
						half3	rpos = rot( p.vertex.xyz, r[i] );
						half4	tpos = half4( rpos, 0.0f ) + t[i];
						
					o.pos = UnityObjectToClipPos( tpos );
					
					o.uv = p.texcoord;
					
					return o;
				}
				
				
				
				half3 iShadeVertexLights( half4 vertex, half3 normal )
				{
					half3 viewpos = mul( UNITY_MATRIX_MV, vertex ).xyz;
					half3 viewN = mul( (half3x3)UNITY_MATRIX_IT_MV, normal );
					half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
					
					half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
					half lengthSq = dot( toLight, toLight );
					half atten = 1.0 / ( 1.0 + lengthSq * unity_LightAtten[0].z );
					half diff = max( 0, dot(viewN, normalize(toLight)) );
					lightColor += unity_LightColor[0].rgb * (diff * atten);
					
					return lightColor;
				}
				


				
				half4	_Color;
				half4x4	m[32];

				v2f vert3( vtxpos p )
				{
					v2f o;
					
					half i = p.color.x * 255;
					
					half4	pos = mul( m[i], p.vertex );
					
					o.pos = mul( UNITY_MATRIX_VP, pos );
					
					o.uv = p.texcoord;
					
					o.color.rgb = iShadeVertexLights( p.vertex, p.normal );
					
					o.color *= _Color;
					

					UNITY_TRANSFER_FOG( o, o.pos );

					return o;
				}
				
				
				
				sampler2D _MainTex;
				
				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 texcol = tex2D(_MainTex, i.uv);
					
					fixed4 col = fixed4( texcol * i.color, 1.0 );// * _Color;
					
					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
			ENDCG
			
		}
		
	}
	
	 
	FallBack "Diffuse"
}
