using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Abarabone.Geometry
{

	using Abarabone.Common.Extension;
	using Abarabone.Utilities;


	public static class TexturePacker
	{

		/// <summary>
		/// パックしたテクスチャと、ＵＶを差し替えた新しいメッシュを返す。
		/// ただしテクスチャが１つしかない場合は、元のテクスチャとメッシュを返す。
		/// </summary>
		public static (Texture2D atlas, Dictionary<Mesh, Mesh> newmeshes) PackTextureAndPairingMeshes(this IEnumerable<GameObject> targetObjects)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();
			var qSrcMesh = mmts.Select(x => x.mesh);
			var qSrcMats = mmts.Select(x => x.mats);

			var (atlas, qDstMesh) = packTextureAndTranslateMeshes_(qSrcMesh, qSrcMats);
			var dict = (qSrcMesh, qDstMesh).Zip().ToDictionary(x => x.x, x => x.y);

			return (atlas, dict);
		}

		/// <summary>
		/// 
		/// </summary>
		public static (Texture2D atlas, IEnumerable<Mesh> meshes) PackTextureAndTranslateMeshes(this IEnumerable<GameObject> targetObjects)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();
			var qMesh = mmts.Select(x => x.mesh);
			var qMats = mmts.Select(x => x.mats);

			return packTextureAndTranslateMeshes_(qMesh, qMats);
		}


		static (Texture2D atlas, IEnumerable<Mesh> translatedMeshes) packTextureAndTranslateMeshes_
			(IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss)
		{

			var uniqueTextures = queryUniqueTextures_(matss);
			if (uniqueTextures.IsSingle()) return (uniqueTextures.First(), meshes);

			var (atlas, uvOffsets) = packTexture_(uniqueTextures);


			var uvOffsetDict = makeUvOffsetDict_(uniqueTextures, uvOffsets);

			var qMeshNewUv = queryMeshNewUv_(meshes, matss, uvOffsetDict);

			var qDstMesh = makeTranslatedUvMeshes_(meshes, qMeshNewUv);

			return (atlas, qDstMesh);


			static Texture2D[] queryUniqueTextures_(IEnumerable<Material[]> matss) =>
				matss
					.SelectMany()
					.Select(mat => mat.mainTexture)
					.OfType<Texture2D>()
					.Distinct()
					.ToArray();

			static (Texture2D, Rect[]) packTexture_(IEnumerable<Texture2D> srcTextures)
			{
				//var dstTexture = new Texture2D( 0, 0 );
				var dstTexture = new Texture2D
					(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

				var uvRects = dstTexture.PackTextures
					(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

				return (dstTexture, uvRects);
			}

			static Dictionary<Texture2D, Rect> makeUvOffsetDict_
				(Texture2D[] uniqueTextures, Rect[] uvOffsets)
			{
				var uvOffsetDict = (uniqueTextures, uvOffsets).Zip()
					.ToDictionary(x => x.x, x => x.y);

				return uvOffsetDict;
			}

			static IEnumerable<IEnumerable<Vector2>> queryMeshNewUv_
				(IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss, Dictionary<Texture2D, Rect> uvOffsetDict)
			{
				var qSubmeshUv =
					from mesh in meshes
					select mesh.SubmeshVertices(mesh.uv)
					// 頂点は若い submesh の順に、隙間なく格納されていると仮定する
					;
				var qSubmeshTex =
					from mats in matss
					select
						from mat in mats
						select mat.mainTexture as Texture2D
					;

				var qMeshUvNew =
					from obj in (qSubmeshUv, qSubmeshTex).Zip()
					select
						from x in (obj.x, obj.y).Zip()
						let uvs = x.x
						let tex = x.y
						from uv in uvs
						select uv.ScaleUv(uvOffsetDict.GetOrDefault(tex, new Rect(0,0,1,1)))
					;

				return qMeshUvNew;
			}

			static IEnumerable<Mesh> makeTranslatedUvMeshes_
				(IEnumerable<Mesh> meshes, IEnumerable<IEnumerable<Vector2>> newUvs)
			{
				var qMeshUvTranslated =
					from x in (meshes, newUvs).Zip()
					let src = x.x
					let dst = cloneMesh_(src, x.y.ToArray())
					select dst
					;
				return qMeshUvTranslated;


				static Mesh cloneMesh_(Mesh srcmesh, Vector2[] newuv)
				{
					var go = new GameObject("dummy for mesh copy");
					var mf = go.AddComponent<MeshFilter>();
					mf.mesh = srcmesh;
					GameObject.Destroy(go);

					var newmesh = mf.mesh;
					newmesh.uv = newuv;

					return newmesh;
				}
			}
		}

	}
}

