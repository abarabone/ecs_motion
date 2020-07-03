Shader "Custom/SimpleUnlitShader"
{
	Properties
	{
		// テクスチャタイリングとテクスチャオフセットのサポートを削除します
		// そのため、マテリアルインスペクターでそれらを非表示にします
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		
		_Color("Color", Color) = (1, 1, 1, 1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : VERTEX_COLOR;
			};

			fixed4	_Color;

			sampler2D _MainTex;
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(half4, aaa)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = UNITY_ACCESS_INSTANCED_PROP(Props, aaa);
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4	o = tex2D(_MainTex, i.uv) * i.color;
				return o;
			}

			ENDCG
		}
	}
}

