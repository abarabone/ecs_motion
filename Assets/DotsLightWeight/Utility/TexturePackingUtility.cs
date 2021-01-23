using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Abarabone.Geometry
{

	using Abarabone.Common.Extension;
	using Abarabone.Utilities;



	static class TexturePackingUtility
	{

		/// <summary>
		/// 
		/// </summary>
		static public (Texture2D atlas, Dictionary<int, Rect> texhashToUvRect)
			PackTextureAndToHashToUvRectDict(this IEnumerable<GameObject> objects)
		{
			var mats = objects.QueryMeshMatsTransform_IfHaving().Select(x => x.mats).SelectMany();
			var uniqueTextures = mats.QueryUniqueTextures().ToArray();

			var (atlas, uvRects) = uniqueTextures.IsSingle()
				? (uniqueTextures.First(), new Rect(0,0,1,1).AsEnumerable())
				: uniqueTextures.PackTexture();


			var uvRectsDict = (uniqueTextures, uvRects).ToHashToUvRectDict();

			return (atlas, uvRectsDict);
		}


		/// <summary>
		/// 
		/// </summary>
		static public IEnumerable<Texture2D> QueryUniqueTextures(this IEnumerable<Material> mats) =>
			mats.Select(mat => mat.mainTexture)
				.OfType<Texture2D>()
				.Distinct();

		/// <summary>
		/// 
		/// </summary>
		static public (Texture2D atlas, Rect[] uvRects) PackTexture(this IEnumerable<Texture2D> srcTextures)
		{
			//var dstTexture = new Texture2D( 0, 0 );
			var dstTexture = new Texture2D
				(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

			var uvRects = dstTexture.PackTextures
				(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

			return (dstTexture, uvRects);
		}

		/// <summary>
		/// 
		/// </summary>
		static public Dictionary<Texture2D, Rect> ToTextureToUvRectDict
			(this (IEnumerable<Texture2D> uniqueTextures, IEnumerable<Rect> uvRects) x)
		{
			var uvOffsetDict = x.Zip()
				.ToDictionary(x => x.src0, x => x.src1);

			return uvOffsetDict;
		}

		/// <summary>
		/// 
		/// </summary>
		static public Dictionary<int, Rect> ToHashToUvRectDict
			(this (IEnumerable<Texture2D> uniqueTextures, IEnumerable<Rect> uvRects) x)
		{
			var uvOffsetDict = x.Zip()
				.ToDictionary(x => x.src0?.GetHashCode() ?? default, x => x.src1);

			return uvOffsetDict;
		}



		// 以下は廃止予定

		static public (Vector2[][][] uvsss, int[][] texhashess) ToUvTranslateSource
			(this (IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss) x)
        {
			var qSubmeshUv =
				from mesh in x.meshes
				select mesh.SubmeshVertices(mesh.uv)
				// 頂点は若い submesh の順に、隙間なく格納されていると仮定する
				;
			var qSubmeshTex =
				from mats in x.matss
				select
					from mat in mats
					select mat.mainTexture?.GetHashCode() ?? default
				;
			return (qSubmeshUv.ToArrayRecursive3(), qSubmeshTex.ToArrayRecursive2());
		}
		static public IEnumerable<IEnumerable<Vector2>> QueryTranslatedUv
			(this (Vector2[][][] uvsss, int[][] texhashess) uvsrc, Dictionary<int, Rect> texhashToUvRect)
		{
			var qMeshUvNew =
				from obj in uvsrc.Zip()
				select
					from x in obj.Zip()
					let uvs = x.src0
					let tex = x.src1
					from uv in uvs
					select uv.ScaleUv(texhashToUvRect.GetOrDefault(tex, new Rect(0, 0, 1, 1)))
				;

			return qMeshUvNew;
		}

		static public IEnumerable<IEnumerable<Vector2>> QueryTranslatedUv
			(this IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss, Dictionary<Texture2D, Rect> texToUvRect)
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
					from x in obj.Zip()
					let uvs = x.src0
					let tex = x.src1
					from uv in uvs
					select uv.ScaleUv(texToUvRect.GetOrDefault(tex, new Rect(0, 0, 1, 1)))
				;

			return qMeshUvNew;
		}

		static public IEnumerable<Mesh> MakeNewUvMeshes
			(this IEnumerable<IEnumerable<Vector2>> newUvs, IEnumerable<Mesh> meshes)
		{
			var qMeshUvTranslated =
				from x in (meshes, newUvs).Zip()
				let src = x.src0
				let dst = cloneMesh_(src, x.src1.ToArray())
				select dst
				;
			return qMeshUvTranslated;


			// 無駄なゲームオブジェクトを作成しているので、もっとよいメッシュクローン法に変えるべき
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
