﻿Shader "Custom/Psyllium ptop"
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

			//#pragma target 5.0
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
				fixed4 color : VERTEX_COLOR;
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

				const int ivec = BoneVectorOffset + i * 2;
				const float4 buf0 = BoneVectorBuffer[ivec + 0];
				const float4 buf1 = BoneVectorBuffer[ivec + 1];

                const fixed4 color = float4(asuint(buf1.w).xxxx >> uint4(24, 16, 8, 0) & 255) / 255.;
                const float size = buf0.w;

				const float4 lvt = v.vertex;
                const float3 wpos0 = buf0.xyz;
                const float3 wpos1 = buf1.xyz;

                const int iofs = lvt.y + 0.5f;
				const float3 wpos_current = BoneVectorBuffer[ivec + (0+iofs)].xyz;

				const float3 eye =  (wpos_current - _WorldSpaceCameraPos);
				const float3 up =  (wpos1 - wpos0);
				const float3 side = normalize(cross(up, eye));
				const float3 edgeface = normalize(cross(eye, side));

				const float4 wvt = float4(wpos_current + (side * lvt.xxx + edgeface * lvt.zzz) * size, 1);
				//const float4x4 mt = float4x4(float4(side, 0), float4(up, 0), float4(edgeface, 0), wpos_current);
				//const float4 wvt = mul(lvt, mt);

                o.vertex = mul(UNITY_MATRIX_VP, wvt);
                o.uv = v.uv;
                o.color = color * 3;//6;
                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgba *= i.color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
