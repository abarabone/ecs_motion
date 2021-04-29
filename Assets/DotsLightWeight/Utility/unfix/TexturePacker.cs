using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DotsLite.Geometry
{

	using DotsLite.Common.Extension;
	using DotsLite.Utilities;


	public static class TexturePacker
	{

		/// <summary>
		/// パックしたテクスチャと、ＵＶを差し替えた新しいメッシュを返す。
		/// ただしテクスチャが１つしかない場合は、元のテクスチャとメッシュを返す。
		/// </summary>
		public static (Texture2D atlas, Dictionary<Mesh, Mesh> oldToNewMesh) PackTextureAndPairingMeshes
			(this IEnumerable<GameObject> targetObjects)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();
			var qSrcMesh = mmts.Select(x => x.mesh);
			var qSrcMats = mmts.Select(x => x.mats);

			var (atlas, qDstMesh) = packTextureAndTranslateMeshes_(qSrcMesh, qSrcMats);
			var dict = (qSrcMesh, qDstMesh).Zip().ToDictionary(x => x.src0, x => x.src1);

			return (atlas, dict);
		}

		/// <summary>
		/// 
		/// </summary>
		public static (Texture2D atlas, IEnumerable<Mesh> meshes) PackTextureAndTranslateMeshes
			(this IEnumerable<GameObject> targetObjects)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(targetObjects).ToArray();
			var qMesh = mmts.Select(x => x.mesh);
			var qMats = mmts.Select(x => x.mats);

			return packTextureAndTranslateMeshes_(qMesh, qMats);
		}


		static (Texture2D atlas, IEnumerable<Mesh> translatedMeshes) packTextureAndTranslateMeshes_
			(IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss)
		{

			var uniqueTextures = matss.SelectMany().QueryUniqueTextures();
			if (uniqueTextures.IsSingle()) return (uniqueTextures.First(), meshes);

			var (atlas, uvOffsets) = uniqueTextures.ToAtlasOrPassThrough();


			var uvOffsetDict = (uniqueTextures, uvOffsets).ToDictionary();

			var qNewUv = meshes.QueryTranslatedUv(matss, uvOffsetDict);

			var qDstMesh = qNewUv.MakeNewUvMeshes(meshes);

			return (atlas, qDstMesh);
		}
	}

}

