Shader "Custom/Psyllium ptop"
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

				int ivec = BoneVectorOffset + i * 2;
				float4 buf0 = BoneVectorBuffer[ivec + 0];
				float4 buf1 = BoneVectorBuffer[ivec + 1];

                uint c = asuint(buf1.w);
                const fixed4 color = float4(c.xxxx >> uint4(24, 16, 8, 0) & 255) / 255.;
                float size = buf0.w;

				float4 lvt = v.vertex;
				float4 wpos0 = float4(buf0.xyz, 1.0f);
				float4 wpos1 = float4(buf1.xyz, 1.0f);
				float4 wpos = wpos0;//BoneVectorBuffer[ivec + (lvt.y+1)/2];

				float3 eye = wpos.xyz - _WorldSpaceCameraPos;
				float3 up = wpos1 - wpos0;
				float3 side = normalize(cross(up, eye)) * size;
				float3 edgeface = normalize(cross(eye, side)) * size;

				//float4 wvt = float4((wpos.xyz + side * lvt.xxx + edgeface * lvt.zzz) * size, 1);
				float4x4 mt = float4x4(float4(side, 0), float4(up, 0), float4(edgeface, 0), wpos);
				float4 wvt = mul(lvt, mt);

                o.vertex = mul(UNITY_MATRIX_VP, wvt);
                o.uv = v.uv;
                o.color = color * 6.;
                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= i.color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
