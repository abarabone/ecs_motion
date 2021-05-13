// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
        //Blend SrcAlpha OneMinusSrcAlpha
        //Blend One One
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

                const fixed4 color = float4(asuint(buf0.w).xxxx >> uint4(24, 16, 8, 0) & 255) * (1. / 255.);
                const half2 uv = buf1.xy;
                const half2 dir = buf1.zw;
                const half2x2 rot = half2x2(half2(dir.x, -dir.y), dir.yx);
                
                const half2 roted = mul(rot, v.vertex.xy);
				const half4 lvt = half4(roted, 0, 0);
                const half3 wpos = buf0.xyz;

                //const half4 vpos = mul(UNITY_MATRIX_V, half4(wpos, 1));
                //o.vertex = mul(UNITY_MATRIX_P, vpos + lvt);
                
                const half3x3 mv = half3x3(UNITY_MATRIX_V[0].xyz, UNITY_MATRIX_V[1].xyz, UNITY_MATRIX_V[2].xyz);
                const half4 _vt = half4(mul(lvt.xyz, mv), 0);
                o.vertex = mul(UNITY_MATRIX_VP, half4(wpos, 1) + _vt);

                o.uv = v.uv;
                o.color = color;// * 6;
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
