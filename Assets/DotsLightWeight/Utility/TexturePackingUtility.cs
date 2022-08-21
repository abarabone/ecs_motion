using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace DotsLite.Geometry
{

	using DotsLite.Common.Extension;
	using DotsLite.Utilities;


	/// <summary>
	/// アトラスと元テクスチャのハッシュ値から、ＵＶオフセット矩形を返す辞書。
	/// ハッシュ集合から矩形集合にも対応する。
	/// </summary>
	public class HashToRect
	{
		public Dictionary<(int atlas, int part), Rect> dict;

		public HashToRect()
		{
			this.dict = new Dictionary<(int atlas, int part), Rect>();
		}
		public HashToRect(TextureAtlasAndParameters texparams)
		{
			this.dict = (texparams.texhashes, texparams.uvRects).ToDictionary();
		}


		public Rect this[(int atlas, int part) hashes]
		{
			set => this.dict[hashes] = value;
			get => this.dict[hashes];
		}
		public Rect this[int atlas, int part]
        {
			set => this.dict[(atlas, part)] = value;
			get => this.dict[(atlas, part)];
		}

		public IEnumerable<Rect> this[IEnumerable<(int atlas, int part)> hashes]
		{
			set
			{
				foreach (var (hash, uv) in (hashes, value).Zip())
				{
					this[hash.atlas, hash.part] = uv;
				}
			}
            get
			{
				foreach (var hash in hashes)
				{
					yield return this[hash.atlas, hash.part];
				}
			}
		}

		//public static implicit operator
		//	Dictionary<(int atlas, int part), Rect>(HashToRect d) => d.dict;

		//public static implicit operator
		//	HashToRect(Dictionary<(int atlas, int part), Rect> d) => d != null ? new HashToRect { dict=d } : null;
	}


	/// <summary>
	/// モデル変換中などに、パラメータとして保持するテクスチャ情報。
	/// アトラス、元テクスチャハッシュ値の集合、ＵＶオフセット矩形の集合を保持する。
	/// </summary>
    public struct TextureAtlasAndParameters
    {
        public Texture2D atlas;
        public IEnumerable<(int atlas, int part)> texhashes;
        public IEnumerable<Rect> uvRects;

		//public Func<int, Rect> ToTexHashToUvRectFunc()
		//{
		//	var dict = new HashToRect { dict = (texhashes, uvRects).ToDictionary() };
		//	var atlashash = this.atlas.GetHashCode();
		//	return hash => dict[atlashash, hash];
		//}
}


    static class TexturePackingUtility
	{

		/// <summary>
		/// テクスチャの集合からアトラスを生成し、アトラスとＵＶオフセット配列を返す。
		/// </summary>
		static public (Texture2D atlas, Rect[] uvRects) ToAtlas(this IEnumerable<Texture2D> srcTextures)
		{
			//var dstTexture = new Texture2D( 0, 0 );
			var dstTexture = new Texture2D
				(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

			var uvRects = dstTexture.PackTextures
				(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

			return (dstTexture, uvRects);
		}

		/// <summary>
		/// テクスチャの集合から、アトラスとＵＶオフセット配列を返す。
		/// ただし、テクスチャが１つだけの時は、アトラス化せずに渡したテクスチャを返す。
		/// </summary>
		static public (Texture2D atlas, Rect[] uvRects) ToAtlasOrPassThrough(this IEnumerable<Texture2D> srcTextures)
		=>
			srcTextures.IsSingle()
				? (srcTextures.First(), new Rect[] { new Rect(0, 0, 1, 1) })
				: srcTextures.ToAtlas();



		/// <summary>
		/// 指定したゲームオブジェクト以下のヒエラルキーから、メインテクスチャの集合を返す。
		/// ただし、重複なし。
		/// </summary>
		static public IEnumerable<Texture2D> QueryUniqueTextures(this GameObject obj) =>
			obj.GetComponentsInChildren<Renderer>()
				.SelectMany(r => r.sharedMaterials)
				.QueryUniqueTextures();
			//objects.QueryMeshMatsTransform_IfHaving()
			//	.Select(x => x.mats)
			//	.SelectMany()
			//	.QueryUniqueTextures();

		/// <summary>
		/// マテリアルの集合から、メインテクスチャの集合を返す。
		/// ただし、重複なし。
		/// </summary>
		static public IEnumerable<Texture2D> QueryUniqueTextures(this IEnumerable<Material> mats) =>
			mats.Select(mat => mat.mainTexture)
				.Where(x => x != null)
				.OfType<Texture2D>()
				.Distinct();



		/// <summary>
		/// 重複なしテクスチャの集合から、TextureAtlasAndParameters を生成する。
		/// </summary>
		static public TextureAtlasAndParameters ToAtlasOrPassThroughAndParameters(this IEnumerable<Texture2D> uniqueTextures)
		{
			var texs = uniqueTextures.ToArray();

			var (atlas, uvRects) = texs.ToAtlasOrPassThrough();

			var qKeyHash = texs.queryKeyHashes_(atlas);

			return new TextureAtlasAndParameters
			{
				atlas = atlas,
				texhashes = qKeyHash.Append((atlas.GetHashCode(), 0)).ToArray(),
				uvRects = uvRects.Append(new Rect(0, 0, 1, 1)),
			};
		}
		static IEnumerable<(int, int)> queryKeyHashes_(this IEnumerable<Texture2D> uniqueTextures, Texture2D atlas)
		=>
			from tex in uniqueTextures
			select (atlas.GetHashCode(), tex.GetHashCode())
			;





		///// <summary>
		///// 
		///// </summary>v
		//static public (IEnumerable<(int, int)>, IEnumerable<Rect>) queryHashAndUvRectPairs
		//	(this (IEnumerable<Texture2D> uniqueTextures, IEnumerable<Rect> uvRects) x, Texture2D atlas)
		//{
		//	var qKeys =
		//		from tex in x.uniqueTextures
		//		select (atlas.GetHashCode(), tex.GetHashCode())
		//		;
		//	var qKeysWithEmpty = qKeys.Append((atlas.GetHashCode(), 0));
		//	var qValueWithEmpty = x.uvRects.Append(new Rect(0, 0, 1, 1));

		//	return (qKeysWithEmpty, qValueWithEmpty);
		//}




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
