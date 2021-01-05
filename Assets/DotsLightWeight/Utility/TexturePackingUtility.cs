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

		static public Texture2D[] QueryUniqueTextures(this IEnumerable<Material[]> matss) =>
			matss
				.SelectMany()
				.Select(mat => mat.mainTexture)
				.OfType<Texture2D>()
				.Distinct()
				.ToArray();

		static public (Texture2D, Rect[]) PackTexture(this IEnumerable<Texture2D> srcTextures)
		{
			//var dstTexture = new Texture2D( 0, 0 );
			var dstTexture = new Texture2D
				(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

			var uvRects = dstTexture.PackTextures
				(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

			return (dstTexture, uvRects);
		}

		static public Dictionary<Texture2D, Rect> MakeTextureToUvOffsetDict
			(this (IEnumerable<Texture2D> uniqueTextures, IEnumerable<Rect> uvOffsets) x)
		{
			var uvOffsetDict = x.Zip()
				.ToDictionary(x => x.src0, x => x.src1);

			return uvOffsetDict;
		}

		static public IEnumerable<IEnumerable<Vector2>> QueryTranslatedUv
			(this IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss, Dictionary<Texture2D, Rect> uvOffsetDict)
		{
			var qSubmeshUv =
				from mesh in meshes
				select mesh.SubmeshVertices(mesh.uv)
				// í∏ì_ÇÕé·Ç¢ submesh ÇÃèáÇ…ÅAåÑä‘Ç»Ç≠äiî[Ç≥ÇÍÇƒÇ¢ÇÈÇ∆âºíËÇ∑ÇÈ
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
					select uv.ScaleUv(uvOffsetDict.GetOrDefault(tex, new Rect(0, 0, 1, 1)))
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
