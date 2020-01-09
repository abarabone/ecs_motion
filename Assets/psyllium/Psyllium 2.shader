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


			half3 rot(half3 lpos, half3 wpos, half3 wdir)
			{
				half3 weye = normalize(_WorldSpaceCameraPos - wpos);
				half3 h = cross(wdir, weye);
				return lpos.xxx * h;
			}

            v2f vert (appdata v, uint i : SV_InstanceID)
            {
                v2f o;

				int ivec = BoneVectorOffset + i * 2;

				float4 wpos = BoneVectorBuffer[ivec + 0];
				float4 wdir = BoneVectorBuffer[ivec + 1];

                float4 vertex = float4(v.vertex.xy, 0.0, 1.0);
				half3 rpos = rot(vertex.xyz, wpos.xyz, wdir.xyz);

				float4 pos = float4(wpos.xyz + v.vertex.xyz, 1.0f);


                //float3 offsetVec = normalize(cross(cameraToBar, barSide));
                //vertex.xyz += offsetVec * v.vertex.z;

                o.vertex = mul(UNITY_MATRIX_VP, pos);
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
