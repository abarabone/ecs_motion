Shader "Custom/Psyllium2"
{
    Properties
    {
		[NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" {}
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
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
			
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			StructuredBuffer<float4> BoneVectorBuffer;
			//int	BoneLengthEveryInstance;
			int BoneVectorOffset;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Color;


            v2f vert (appdata v, uint i : SV_InstanceID)
            {
                v2f o;

				int ivec = BoneVectorOffset + i * 2;

				half4 wpos = BoneVectorBuffer[ivec + 0];
				half4 wdir = BoneVectorBuffer[ivec + 1];

				half3 eye = wpos.xyz - _WorldSpaceCameraPos;
				half3 up = wdir.xyz;
				half3 side = normalize(cross(up, eye));
				half3 edgeface = normalize(cross(eye, side));
				
				half4x4 mt = half4x4(half4(side, 0), half4(up, 0), half4(edgeface, 0), wpos);
				half4 lvt = v.vertex;
				half4 wvt = mul(lvt, mt);

                o.vertex = mul(UNITY_MATRIX_VP, wvt );
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
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
