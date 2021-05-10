Shader "Custom/Particle uv"
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
                const float size = buf1.z;
                const half2 uv = buf1.xy;

				const float4 lvt = v.vertex;
                const float3 wpos = buf0.xyz;

				const float3 eye =  (wpos - _WorldSpaceCameraPos);
				const float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				const float3 side = normalize(cross(up, eye));
				//const float3 edgeface = normalize(eye);//cross(eye, side));

				const float4 wvt = float4(wpos + (side * lvt.xxx + up * lvt.yyy) * size, 1);
				//const float4x4 mt = float4x4(float4(side, 0), float4(up, 0), float4(edgeface, 0), wpos_current);
				//const float4 wvt = mul(lvt, mt);

                o.vertex = mul(UNITY_MATRIX_VP, wvt);
                o.uv = v.uv;
                o.color = color * 6;
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
