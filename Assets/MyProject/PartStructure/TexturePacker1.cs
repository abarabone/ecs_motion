using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Abarabone.Geometry
{

	using Abarabone.Common.Extension;


	public static class TexturePacker1
	{


		public static (Texture atlas, Dictionary<Texture2D, Mesh> newmeshes) Pack(IEnumerable<GameObject> targetObjects)
        {

			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();

			var uniqueTextures = queryUniqueTextures_(mmts.Select(x => x.mats));

			var (atlas, uvOffsets) = packTexture_(uniqueTextures);

			var uvOffsetDict = (uniqueTextures, uvOffsets).Zip()
				.ToDictionary(x => x.x, x => x.y);
			uvOffsetDict.Add(null, new Rect(0,0,1,1));


			var qSubmeshUv =
				from mmt in mmts
				select mmt.mesh.SubmeshVertices(mmt.mesh.uv)
				;
			var qSubmeshTex =
				from mmt in mmts
				select
					from mat in mmt.mats
					select mat.mainTexture as Texture2D
				;

			var qSubmeshUvNew =
				from obj in (qSubmeshUv, qSubmeshTex).Zip()
				select
					from x in (obj.x, obj.y).Zip()
					let uvs = x.x
					let tex = x.y
					select
						from uv in uvs
						select uv.ScaleUv(uvOffsetDict[tex])
				;

			var qMesh = (mmts, qSubmeshUvNew).Zip()
				.Select(x => (src: x.x.mesh, dst: cloneMesh_(x.x.mesh, x.y.SelectMany().ToArray());

			return (atlas, newmeshes);


			static Texture2D[] queryUniqueTextures_(IEnumerable<Material[]> matss) =>
				matss
					.SelectMany()
					.Distinct()
					.Select(mat => mat.mainTexture)
					.OfType<Texture2D>()
					.ToArray();


			static (Texture, Rect[]) packTexture_(IEnumerable<Texture2D> srcTextures)
			{
				//var dstTexture = new Texture2D( 0, 0 );
				var dstTexture = new Texture2D
					(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

				var uvRects = dstTexture.PackTextures
					(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

				return (dstTexture, uvRects);
			}

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

