Shader "Custom/Psyllium ptop uv pmap"
{
    Properties
    {
		[NoScaleOffset]
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1)
    }
    SubShader
    {
		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		
        Blend One OneMinusSrcAlpha
		Lighting Off
        ZWrite Off
        Fog
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
            float4 UvParam;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Color;


            v2f vert (appdata v, uint i : SV_InstanceID)
            {
                v2f o;
                
				const int ivec = BoneVectorOffset + i * 3;
				const float4 buf0 = BoneVectorBuffer[ivec + 0];
				const float4 buf1 = BoneVectorBuffer[ivec + 1];
				const float4 buf2 = BoneVectorBuffer[ivec + 2];

				const float4 lvt = v.vertex;
                const float3 wpos0 = buf0.xyz;
                const float3 wpos1 = buf1.xyz;

                const int iofs = lvt.y + 0.5f;
				const float4 currentbuf = BoneVectorBuffer[ivec + (0+iofs)];
				const float3 wpos_current = currentbuf.xyz;
                const float size = currentbuf.w;

				const float3 eye =  (wpos_current - _WorldSpaceCameraPos);
				const float3 up_ =  (wpos1 - wpos0);
                const float3 up = up_;//any(up_) ? up_ : float3(0,1,0);
				const float3 side = normalize(cross(up, eye));
				const float3 edgeface = normalize(cross(eye, side));

				const float4 wvt = float4(wpos_current + (side * lvt.xxx + edgeface * lvt.zzz) * size, 1);
				//const float4x4 mt = float4x4(float4(side, 0), float4(up, 0), float4(edgeface, 0), wpos_current);
				//const float4 wvt = mul(lvt, mt);
                o.vertex = mul(UNITY_MATRIX_VP, wvt);
                
                const half2 uvspan = UvParam.xy;
                const half2 uvtick = uvspan * 0.01f;// / 100;
                const uint4 uvp = asuint(buf2.xxxx) >> uint4(0, 8, 16, 24) & 255;
                const half2 uvofs = uvp.xy * uvspan + uvtick;
                const half2 uvsize = uvp.zw * uvspan - uvtick - uvtick;
                o.uv = uvofs + v.uv * uvsize;
                
                //const fixed4 color = float4(asuint(buf2.yyyy) >> uint4(0, 8, 16, 24) & 255) * (1. / 255.);
                //o.color = color * 6;
                const fixed4 addcolor = float4(asuint(buf2.zzzz) >> uint4(0, 8, 16, 24) & 255) * (1. / 255.);
                const fixed4 blendcolor = float4(asuint(buf2.wwww) >> uint4(0, 8, 16, 24) & 255) * (1. / 255.);
	            o.color = fixed4(blendcolor.rgb * blendcolor.a, blendcolor.a);  //éñëOèÊéZ
	            o.color.rgb += addcolor.rgb * (1 + addcolor.a * 6);             //â¡éZê¨ï™í«â¡
                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 o;

                fixed4 tex = tex2D(_MainTex, i.uv);
                tex.rgb *= tex.a;
                o = tex * i.color;
                UNITY_APPLY_FOG(i.fogCoord, o);
                return o;
                //fixed4 col = tex2D(_MainTex, i.uv);
                //col.rgba *= i.color;
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
            }
            ENDCG
        }
    }
}
