Shader "Custom/Psyllium"
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
			int	BoneLengthEveryInstance;
			int BoneVectorOffset;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Color;

            v2f vert (appdata v, uint i : SV_InstanceID)
            {
                v2f o;

				int ivec = BoneVectorOffset + i * 2;

				float4 wpos = BoneVectorBuffer[ivec + 0];
				float4 wdir = BoneVectorBuffer[ivec + 1];


				//float4x4 mat = unity_ObjectToWorld;
                //float3 barUp = mat._m01_m11_m21;
                //float3 barPos = mat._m03_m13_m23;
				float3 barUp = wdir.xyz;
				float3 barPos = wpos.xyz;

                // Y軸をロックして面をカメラに向ける姿勢行列を作る
                float3 cameraToBar = barPos - _WorldSpaceCameraPos;
                float3 barSide = normalize(cross(barUp, cameraToBar));
                //float3 barForward = normalize(cross(barSide, barUp));
				float3 offsetVec = normalize(cross(cameraToBar, barSide));

				//float3 barUp = normalize(cross(barForward, cameraToBar));
				//float3 barSide = normalize(cross(barUp, barForward));

				float4x4 mat = float4x4( float4(barSide,0), float4(barUp, 0), float4(offsetVec, 0), wpos );
    //            mat._m00_m10_m20 = barSide;
    //            mat._m01_m11_m21 = barUp;
    //            mat._m02_m12_m22 = barForward;
				//mat._m03_m13_m23 = wpos.xyz;
				//mat = transpose(mat);

				float4 vertex = v.vertex;// float4(v.vertex.xy, 0.0, 1.0);


                vertex = mul(vertex, mat);

                //float3 offsetVec = normalize(cross(cameraToBar, barSide));
                //vertex.xyz += offsetVec * v.vertex.z;

                o.vertex = mul(UNITY_MATRIX_VP, vertex);
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
