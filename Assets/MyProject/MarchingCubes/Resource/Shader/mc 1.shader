Shader "Custom/mc"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"LightMode" = "ForwardBase"
		}
		LOD 100

		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
			//#pragma exclude_renderers d3d11 gles

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			//#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc" // _LightColor0 に対し
			
			
		#pragma enable_d3d11_debug_symbols


			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				half3 normal : NORMAL;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;


			StructuredBuffer<uint> cube_instances;
			Texture2DArray<uint> grid_cubeids;
			//StructuredBuffer<uint> grid_cubeids;


			float4 normals[155];

			//StructuredBuffer<uint4> cube_patterns;
			float4 cube_patterns[254][2];
			// [0] : vertex posision index for tringle { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
			// [1] : vertex normal index for vertex { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

			static const uint itri_to_ivtx = 0;
			static const uint ivtx_to_inml = 1;


			float4 cube_vtxs[12];
			// x: near vertex index (ortho1>>0 | ortho2>>8 | slant>>16)
			// y: near vertex offset ortho1 (x>>0 | y>>8 | z>>16)
			// z: near vertex offset ortho2 (x>>0 | y>>8 | z>>16)
			// w: pos(x>>0 | y>>8 | z>>16)


			float4 grids[512][2];
			// [0] : position as float3
			// [1] : near grid id
			// { x : back>>0 | up>>16  y : left>>0 | current>>16  z : right>>0 | down>>16  w : forward>>0 }

			static const uint grid_pos = 0;
			static const uint grid_near_id = 1;



			//static const int _32e0 = 1;
			//static const int _32e1 = 32;
			//static const int _32e2 = 32 * 32;
			//static const int _32e3 = 32 * 32 * 32;
			//static const int xspan = _32e0;
			//static const int yspan = _32e2;
			//static const int zspan = _32e1;
			//static const int grid_span = _32e3;
			//static const int4 cube_span = int4(xspan, yspan, zspan, grid_span);


			static const uint4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			
			uint unpack8bit_uint4_to_uint(uint4 packed_uint4, uint element_index, uint packed_index)
			{
				const uint iouter = element_index;
				const uint iinner = packed_index << 3;
				const uint element = dot(packed_uint4, element_mask_table[iouter]);
				return element >> iinner & 0xff;
			}
			uint unpack8bit_uint4_to_uint(uint4 packed_uint4, uint index)
			{
				const uint element_index = index >> 2;
				const uint packed_index = index & 0x3;
				return unpack8bit_uint4_to_uint(packed_uint4, element_index, packed_index);
			}

			uint unpack16bit_uint4_to_uint(uint4 packed_uint4, uint index)
			{
				const uint iouter = index >> 1;
				const uint iinner = (index & 1) << 4;
				const uint element = dot(packed_uint4, element_mask_table[iouter]);
				return element >> iinner & 0xffff;
			}

			uint3 unpack8bits_uint_to_uint3(uint packed3_uint)
			{
				return packed3_uint.xxx >> uint3(0, 8, 16) & 0xff;
			}
			uint3 unpack8bits_uint3_to_uint3(uint3 packed3_uint3, uint element_index)
			{
				const uint element = dot(packed3_uint3, element_mask_table[element_index].xyz);
				return unpack8bits_uint_to_uint3(element);
			}




			// cubeindex { x,y,z: cube inner 3d index  w: grid index }

			int3 get_cube_offset_near(uint4 icube, uint ivtx_in_cube, uint ortho_selector)
			{
				const uint3 offset_packed = asuint(cube_vtxs[ivtx_in_cube].xyz);
				const int3 offset = (int3)unpack8bits_uint3_to_uint3(offset_packed, ortho_selector) - 1;
				return offset;
			}

			uint4 get_cubeindex_near(uint4 cubeindex, int3 cube_offset)
			{
				const int3 cubeindex_outer = (int3)cubeindex.xyz + cube_offset;

				const int3 outer_offset = cubeindex_outer >> 5;
				const uint grid_near_selector = dot(outer_offset, int3(1, 2, 3)) + 3;
				
				const uint gridindex_current = cubeindex.w;
				const uint4 gridindex_near_packed = asuint(grids[gridindex_current][grid_near_id]);
				const uint gridindex_near = unpack16bit_uint4_to_uint(gridindex_near_packed, grid_near_selector);

				const uint3 cubeindex_inner = cubeindex_outer & 0x1f;
				
				return uint4(cubeindex_inner, gridindex_near);
			}

			uint get_cubeid_near(uint4 cubeindex)
			{
				const uint3 index = uint3(cubeindex.z * 32 + cubeindex.x, cubeindex.y, cubeindex.w);
				return grid_cubeids[index];

				//const int index = dot(cubeindex, cube_span);
				//return grid_cubeids[index];
			}

			float3 get_vtx_normal(uint cubeid, uint ivtx_in_cube)
			{
				const uint4 inml_packed = asuint(cube_patterns[cubeid][ivtx_to_inml]);
				const uint inml = unpack8bit_uint4_to_uint(inml_packed, ivtx_in_cube);
				return normals[inml];
			}

			float3 get_vtx_normal_near(uint4 cubeindex_current, uint2 ivtx, uint ortho_selector, out uint4 cubeindex_near)
			{
				const int3 offset = get_cube_offset_near(cubeindex_current, ivtx.x, ortho_selector);
				const uint4 cubeindex = get_cubeindex_near(cubeindex_current, offset);

				const uint cubeid = get_cubeid_near(cubeindex);
				const float3 normal = get_vtx_normal(cubeid, ivtx.y);

				cubeindex_near = cubeindex;
				return normal;
			}

			float3 get_and_caluclate_triangle_to_vertex_normal(uint cubeid_current, uint ivtx_current, uint4 cubeindex_current)
			{
				const uint ivtx_near_packed = asuint(cube_vtxs[ivtx_current].x);
				const uint4 ivtx = uint4(unpack8bits_uint_to_uint3(ivtx_near_packed), ivtx_current);

				uint4 cubeindex_near;
				const float3 nm0 = get_vtx_normal(cubeid_current, ivtx.w);
				const float3 nm1 = get_vtx_normal_near(cubeindex_current, ivtx.wx, 1, cubeindex_near);
				const float3 nm2 = get_vtx_normal_near(cubeindex_current, ivtx.wy, 2, cubeindex_near);
				const float3 nm3 = get_vtx_normal_near(cubeindex_near,    ivtx.wz, 1, cubeindex_near);

				return normalize(nm0 + nm1 + nm2 + nm3);
			}



			static const float3 vvvv[] = { {0,0,0}, {1,0,0}, {0,1,0} };
			v2f vert(appdata v, uint i : SV_InstanceID)
			{
				v2f o;

				const uint data = cube_instances[i];
				const uint cubeid = (data & 0xff) - 1;

				const uint4 ivtx_packed = asuint(cube_patterns[cubeid][itri_to_ivtx]);
				const uint ivtx_in_cube = unpack8bit_uint4_to_uint(ivtx_packed, v.vertex.y, v.vertex.x);

				const uint4 cubeindex = data.xxxx >> uint4(16, 21, 26, 8) & uint4(0x1f, 0x1f, 0x1f, 0xff);

				const float3 gridpos = grids[cubeindex.w][grid_pos];
				const int3 cubepos = (int3)cubeindex.xyz * int3(1, -1, -1);

				const uint cube_vtx_lpos_packed = asuint(cube_vtxs[ivtx_in_cube].w);
				const float3 cube_vtx_lpos = ((int3)unpack8bits_uint_to_uint3(cube_vtx_lpos_packed) - 1) * 0.5f;

				const float4 lvtx = float4(gridpos + cubepos + cube_vtx_lpos, 1.0f);
				//const float4 lvtx = float4(gridpos + cube_location_ltb + vvvv[v.vertex.x], 1.0f);

				o.vertex = mul(UNITY_MATRIX_VP, lvtx);//UnityObjectToClipPos(lvtx);


				const float3 normal = get_and_caluclate_triangle_to_vertex_normal(cubeid, ivtx_in_cube, cubeindex);
				const float3 worldNormal = normal;
				const fixed nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.color = _LightColor0 * nl;
				//// この処理をしないと陰影が強くつきすぎる
				//// https://docs.unity3d.com/ja/current/Manual/SL-VertexFragmentShaderExamples.html
				//// の「アンビエントを使った拡散ライティング」を参考
				o.color.rgb += ShadeSH9(half4(worldNormal, 1));

				o.normal = worldNormal;

				o.uv = half2(0,0);//lvtx.xy; TRANSFORM_TEX(lvtx.xyspan, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.color;// tex2D(_MainTexspan, i.uv) * i.color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

		ENDCG
	}
	}
}
