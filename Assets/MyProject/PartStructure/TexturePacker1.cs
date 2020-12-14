using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Abarabone.Geometry
{

	using Abarabone.Common.Extension;


	public static class TexturePacker
	{


		public static (Texture2D atlas, Dictionary<Mesh, Mesh> newmeshes) Pack(IEnumerable<GameObject> targetObjects)
        {

			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();


			var uniqueTextures = queryUniqueTextures_(mmts.Select(x => x.mats));

			var (atlas, uvOffsets) = packTexture_(uniqueTextures);


			var uvOffsetDict = makeUvOffsetDict_(uniqueTextures, uvOffsets);

			var qMeshNewUv = queryMeshNewUv_(mmts, uvOffsetDict);

			var meshPairDict = makeMeshPairDict_(mmts, qMeshNewUv);

			return (atlas, meshPairDict);


			static Texture2D[] queryUniqueTextures_(IEnumerable<Material[]> matss) =>
				matss
					.SelectMany()
					.Distinct()
					.Select(mat => mat.mainTexture)
					.OfType<Texture2D>()
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

				uvOffsetDict.Add(null, new Rect(0, 0, 1, 1));

				return uvOffsetDict;
			}

			static IEnumerable<IEnumerable<Vector2>> queryMeshNewUv_
				(IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, Dictionary<Texture2D, Rect> uvOffsetDict)
			{
				var qSubmeshUv =
					from mmt in mmts
					select mmt.mesh.SubmeshVertices(mmt.mesh.uv)
					// 頂点は若い submesh の順に、隙間なく格納されていると仮定する
					;
				var qSubmeshTex =
					from mmt in mmts
					select
						from mat in mmt.mats
						select mat.mainTexture as Texture2D
					;

				var qMeshUvNew =
					from obj in (qSubmeshUv, qSubmeshTex).Zip()
					select
						from x in (obj.x, obj.y).Zip()
						let uvs = x.x
						let tex = x.y
						from uv in uvs
						select uv.ScaleUv(uvOffsetDict[tex])
					;

				return qMeshUvNew;
			}

			static Dictionary<Mesh, Mesh> makeMeshPairDict_
				(IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, IEnumerable<IEnumerable<Vector2>> meshUvNew)
			{
				var qMeshPair =
					from x in (mmts, meshUvNew).Zip()
					let src = x.x.mesh
					let dst = cloneMesh_(x.x.mesh, x.y.ToArray())
					select (src, dst)
					;

				return qMeshPair.ToDictionary(x => x.src, x => x.dst);


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

